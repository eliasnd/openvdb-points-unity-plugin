using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace OpenVDBPointsUnity
{
    [InitializeOnLoad]
    public static class OpenVDBPointsAPI
    {
        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        private const string libraryName = "libopenvdb-points-unity";
        #else
        private const string libraryName = "openvdb-points-unity";
        #endif

        #region api

        private static string gridName = "Points";

        static OpenVDBPointsAPI()
        {
            openvdbInitialize();
        }
        /// <summary>Initializes OpenVDB.</summary>
        public static void Initialize()
        {
            openvdbInitialize();
        }

        /// <summary> Loads a PointDataGrid from <see cref="FilePath" />.</summary>
        /// <param name="gridName">The name of the grid to load.</param>
        /// <param name="cb">The optional <see cref="LoggingCallback">callback</see> for logging native messages.</param>
        /// <remarks> <see cref="FilePath"/> must be set in order to load the grid.</remarks>
        /// <exception cref="Exception">The file is not found, or <see cref="FilePath"/> is not set.</exception>
        public static IntPtr Load(string filePath, string name = null, LoggingCallback cb = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("{0} could not be found!", filePath));
            }
            string grid = name == null ? gridName : name;
            LoggingCallback logger = cb == null ? LogMessage : cb;
            return readPointDataFromFile(filePath, grid, logger);
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

        public static uint GetCount(IntPtr gridRef)
        {
            return getPointCountFromGrid(gridRef);
        }

        unsafe public static int PopulateVertices(IntPtr gridRef, NativeArray<Point> verts)
        {
            unsafe
            {
                return populatePoints(gridRef, NativeArrayUnsafeUtility.GetUnsafePtr(verts));
            }
        }

        public static void FinalizeGrid(IntPtr gridRef)
        {
            destroyPointData(gridRef);
        }

        public static UInt32_3 GetTreeShape(IntPtr gridRef)
        {
            return getTreeShape(gridRef);
        }

        public static void PopulateTreeOffsets(IntPtr gridRef, NativeArray<int> layer1Offsets, NativeArray<int> layer2Offsets, NativeArray<int> leafNodeOffsets)
        {
            unsafe
            {
                populateTreeOffsets(
                    gridRef, 
                    Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(layer1Offsets), 
                    Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(layer2Offsets), 
                    Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(leafNodeOffsets)
                );
            }
        }

        public static void PopulateTreeMask(IntPtr gridRef, Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection, bool frustumCulling, bool lod, bool occlusionCulling, NativeArray<int> layer1Offsets, NativeArray<int> layer2Offsets, NativeArray<int> leafNodeOffsets)
        {
            unsafe
            {
                populateTreeMask(
                    gridRef, 
                    model,
                    view,
                    projection,
                    frustumCulling,
                    lod,
                    occlusionCulling,
                    Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(layer1Offsets), 
                    Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(layer2Offsets), 
                    Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(leafNodeOffsets),
                    LogMessage
                );
            }
        }

        /// <summary> Default <see cref="LoggingCallback">callback</see> for logging native messages. </summary> 
        private static void LogMessage(string message)
        {
            Debug.Log(message);
        }

        #endregion

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
        private static extern IntPtr readPointDataFromFile(string filename, string gridName, LoggingCallback cb);
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
        private static extern void destroyPointData(IntPtr gridRef);
        /// <summary> Converts an unordered point cloud from a .ply file to VDB format. </summary>
        /// <param name="filename">The absolute path to the .ply file. </param>
        /// <param name="outfile">The absolute path to the .vdb file. </param>
        /// <param name="cb">The <see cref="LoggingCallback">callback</see> for logging native messages.</param>
        /// <returns>True if the file was successfully converted,  false  if not. </returns>
        /// <remarks>Currently only supports vertex positions (float) and colors (uint8) </remarks>
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool convertPLYToVDB(string filename, string outfile, LoggingCallback callback);

        [DllImport(libraryName)]
        unsafe private static extern int populatePoints(IntPtr gridRef, void* points);

        [DllImport(libraryName)]
        unsafe private static extern UInt32_3 getTreeShape(IntPtr gridRef);

        [DllImport(libraryName)]
        unsafe private static extern void populateTreeOffsets(IntPtr gridRef, void* layer1Offsets, void* layer2Offsets, void* leafNodeOffsets);

        [DllImport(libraryName)]
        unsafe private static extern void populateTreeMask(IntPtr gridRef, Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection, bool frustumCulling, bool lod, bool occlusionCulling, void* layer1Offsets, void* layer2Offsets, void* leafNodeOffsets, LoggingCallback callback);
        #endregion
    }
}