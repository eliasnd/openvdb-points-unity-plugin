using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
// using UnityEditor.SceneView;
using Unity.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Linq;

namespace OpenVDBPointsUnity
{
    /// <summary>Native interface for OpenVDB Points Module</summary> 
    [Serializable]
    public sealed class OpenVDBPoints : ScriptableObject
    {
        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        private const string libraryName = "libopenvdb-points-unity";
        #else 
        private const string libraryName = "openvdb-points-unity";
        #endif

        /// <summary>The absolute path to the .vdb file on disk.</summary>
        /// <remarks>Note- must be an absolute path.</remarks>
        public string FilePath { get; set; }
        public string Name { get; private set; }
        /// <summary>The default grid name used to access the PointDataGrid.</summary>
        // private const string gridName = "";
        private const string gridName = "Points";
        /// <summary>Pointer to native SharedPointDataGrid.</summary>
        private IntPtr gridRef; 

        private uint count = 0;
        private bool countCalculated = false;
        public uint Count
        {
            get
            {
                if (!countCalculated)
                {
                    GetCount();
                }
                return count;
            }
        }
        private uint visibleCount = 0;  // Visible vertices for frustum culling
        
        public NativeArray<Vertex> Vertices { get; private set; }
        // unsafe private void* vertPtr;


        /// <summary>
        /// Constructor that takes an absolute path to the .vdb 
        /// file containing the PointDataGrid and initializes OpenVDB. 
        /// </summary>
        /// <param name="filePath">Absolute path to the .vdb file</param>
        public OpenVDBPoints(string filePath)
        {
            FilePath = filePath;
            Initialize();
        }
        /// <summary>Parameterless constructor that initializes OpenVDB</summary>
        public OpenVDBPoints() { Initialize(); }

        /// <summary>
        /// Finalizer that deletes the <see cref="gridRef">pointer</see> 
        /// to the SharedPointDataGrid reference.
        /// </summary>
        ~OpenVDBPoints()
        {
            Debug.Log("Delete");
            if (gridRef != IntPtr.Zero)
            {
                destroySharedPointDataGridReference(gridRef);
            }
        }

        /// <summary>Initializes OpenVDB.</summary>
        private void Initialize()
        {
            openvdbInitialize();
        }
        /// <summary> Loads a PointDataGrid from <see cref="FilePath" />.</summary>
        /// <param name="gridName">The name of the grid to load.</param>
        /// <param name="cb">The optional <see cref="LoggingCallback">callback</see> for logging native messages.</param>
        /// <remarks> <see cref="FilePath"/> must be set in order to load the grid.</remarks>
        /// <exception cref="Exception">The file is not found, or <see cref="FilePath"/> is not set.</exception>
        public void Load(string name = null, LoggingCallback cb = null)
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                throw new Exception("FilePath must be set in order to load a PointDataGrid");
            }
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException(string.Format("{0} could not be found!", FilePath));
            }
            string grid = name == null ? gridName : name;
            LoggingCallback logger = cb == null ? LogMessage : cb;
            gridRef = readPointGridFromFile(FilePath, grid, logger);
        }

        public Mesh InitializeMesh(bool useFrustumCulling = false)
        {
            int intCount = (int)Count;  // bad -- should create two arrays and probably meshes for counts > max int

            Vertices = new NativeArray<Vertex>(intCount, Allocator.Temp);
            unsafe {
                // vertPtr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(Vertices);

                if (useFrustumCulling)
                {
                    visibleCount = populateVertices(
                        gridRef,
                        Camera.main.worldToCameraMatrix * Camera.main.projectionMatrix, 
                        //vertPtr,  
                        Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(Vertices),
                        LogMessage
                    );
                }
                else
                {
                    visibleCount = populateVertices(
                        gridRef,
                        Matrix4x4.zero, 
                        //vertPtr,  
                        Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(Vertices),
                        LogMessage
                    );
                }
            }        

            Mesh mesh = new Mesh();
            mesh.SetVertexBufferParams(intCount, new []{
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            });

            mesh.SetVertexBufferData(Vertices, 0, 0, intCount);

            mesh.SetIndices(
                Enumerable.Range(0, (int)visibleCount).ToArray(),
                MeshTopology.Points, 0
            );

            return mesh;
        }

        /// <summary>
        /// The total number of points contained in a PointDataGrid referenced by <see cref="gridRef"/>.
        /// </summary>
        /// <returns>The total point count.</returns>
        private void GetCount()
        {
            if (gridRef != IntPtr.Zero)
            {
                count = getPointCountFromGrid(gridRef);
            }
            else
            {
                throw new Exception("A PointDataGrid must be loaded in order to get a point count!");
            }
        }
        /// <summary> Converts an unordered point cloud from a .ply file to VDB format. </summary>
        /// <param name="filename">The absolute path to the .ply file. </param>
        /// <param name="outfile">The absolute path to the .vdb file. </param>
        /// <param name="cb">The optional <see cref="LoggingCallback">callback</see> for logging native messages.</param>
        /// <returns>True if the file was successfully converted,  false  if not. </returns>
        /// <remarks>Currently only supports vertex positions (float) and colors (uint8) </remarks>
        public static bool ConvertPLYToVDB(string filename, string outfile, LoggingCallback cb = null)
        {
            LoggingCallback logger = cb == null ? LogMessage : cb;
            openvdbInitialize();
            return convertPLYToVDB(filename, outfile, logger);
        }
        /// <summary> Default <see cref="LoggingCallback">callback</see> for logging native messages. </summary> 
        private static void LogMessage(string message)
        {
            Debug.Log(message);
        }

        public void UpdateCulling()
        {
            unsafe {
                visibleCount = populateVertices(
                    gridRef,
                    Camera.main.worldToCameraMatrix * Camera.main.projectionMatrix,  
                    //vertPtr,
                    Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(Vertices),
                    LogMessage
                );
            }
        }
        
        #region native
        /// <summary>
        /// Callback signature for logging messages from the native side.
        /// </summary>
        /// <param name="message">The message generated by the native plugin.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LoggingCallback(string message);
        /// <summary>Wrapper for openvdb::initialize.</summary>
        [DllImport(libraryName)]
        private static extern void openvdbInitialize();
        /// <summary>Loads a PointDataGrid from a file.</summary>
        /// <param name="filename">Absolute path to a .vdb file containing the grid.</param>
        /// <param name="gridName">The name of the grid to load.</param>
        /// <param name="cb">The <see cref="LoggingCallback">callback</see> for logging native messages.</param>
        /// <returns>A pointer to the SharedPointDataGridReference on the native side. </returns>
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readPointGridFromFile(string filename, string gridName, LoggingCallback cb);
        /// <summary>The total number of points in a PointDataGrid.</summary>
        /// <param name="gridRef">Pointer to the SharedPointDataGridReference on the native side.</summary>
        /// <returns>The total number of points in the grid.</returns>
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint getPointCountFromGrid(IntPtr gridRef);
        /// <summary>
        /// Deletes a SharedPointDataGridReference and its corresponding PointDataGrid.
        /// </summary>
        /// <param name="gridRef">Pointer to the SharedPointDataGridReference on the native side.</summary>
        [DllImport(libraryName)]
        private static extern void destroySharedPointDataGridReference(IntPtr gridRef);
        /// <summary> Converts an unordered point cloud from a .ply file to VDB format. </summary>
        /// <param name="filename">The absolute path to the .ply file. </param>
        /// <param name="outfile">The absolute path to the .vdb file. </param>
        /// <param name="cb">The <see cref="LoggingCallback">callback</see> for logging native messages.</param>
        /// <returns>True if the file was successfully converted,  false  if not. </returns>
        /// <remarks>Currently only supports vertex positions (float) and colors (uint8) </remarks>
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool convertPLYToVDB(string filename, string outfile, LoggingCallback callback);

        [DllImport(libraryName)]
        private static extern IntPtr generatePointArrayFromPointGrid(IntPtr gridRef, LoggingCallback cb);

        [DllImport(libraryName)]
        private static extern IntPtr generateColorArrayFromPointGrid(IntPtr gridRef);

        [DllImport(libraryName)]
        private static extern IntPtr arraysToPointGrid(IntPtr positionArr, IntPtr colorArr, int count);

        [DllImport(libraryName)]
        unsafe private static extern uint populateVertices(IntPtr gridRef, Matrix4x4 cameraMat, void* verts, LoggingCallback cb);

        #endregion
    }
}
