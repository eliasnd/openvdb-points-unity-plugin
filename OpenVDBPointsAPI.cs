using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;

namespace OpenVDBPointsUnity
{
    public static class OpenVDBPointsAPI
    {
        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        private const string libraryName = "libopenvdb-points-unity";
        #else
        private const string libraryName = "openvdb-points-unity";
        #endif

        #region api

        private static string gridName = "Points";

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
            return readPointGridFromFile(filePath, grid, logger);
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

        unsafe public static uint PopulateVertices(IntPtr gridRef, NativeArray<Vertex> verts)
        {
            unsafe
            {
                return populateVertices(
                    gridRef,
                    Matrix4x4.zero,
                    Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(verts),
                    LogMessage
                );
            }
        }

        // Frustum culling
        unsafe public static uint PopulateVertices(IntPtr gridRef, NativeArray<Vertex> verts, Camera cam)
        {
            unsafe
            {
                return populateVertices(
                    gridRef,
                    cam.worldToCameraMatrix * cam.projectionMatrix,
                    Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(verts),
                    LogMessage
                );
            }
        }

        public static void FinalizeGrid(IntPtr gridRef)
        {
            destroySharedPointDataGridReference(gridRef);
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