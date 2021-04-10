using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
    
namespace OpenVDBPointsUnity 
{
    public class OpenVDBPointsData : ScriptableObject
    {
        #region public

        /// <summary>The absolute path to the .vdb file on disk.</summary>
        /// <remarks>Note- must be an absolute path.</remarks>
        [SerializeField] public string FilePath; // { get; set; }
        
        [SerializeField] public string Name; // { get; private set; }

        [SerializeField] public uint Count
        {
            get
            {
                if (!countCalculated)
                    if (new IntPtr(gridRef) != IntPtr.Zero) {
                        count = OpenVDBPointsAPI.GetCount(new IntPtr(gridRef));
                        countCalculated = true;
                    }
                    else
                        throw new Exception("A PointDataGrid must be loaded in order to get a point count!");
                return count;
                
            }
        }

        #endregion

        #region serialized

        /// <summary>The default grid name used to access the PointDataGrid.</summary>
        // private const string gridName = "";
        // [SerializeField] string gridName = "Points";
        /// <summary>Pointer to native SharedPointDataGrid.</summary>
        // [SerializeField] IntPtr gridRef;
        #if UNITY_64
            [SerializeField] long gridRef;
        #else
            [SerializeField] int gridRef;
        #endif

        [SerializeField] uint count = 0;
        [SerializeField] bool countCalculated = false;

        #endregion 

        /* public void OnEnable()
        {
            OpenVDBPointsAPI.Initialize();
        } */

        public void Load(string filePath)
        {
            FilePath = filePath;
            #if UNITY_64
                gridRef = OpenVDBPointsAPI.Load(FilePath).ToInt64();
            #else
                gridRef = OpenVDBPointsAPI.Load(FilePath).ToInt32();
            #endif
            Debug.Log(gridRef);
        }

        /* public uint PopulateVertices(NativeArray<Vertex> verts, Camera cam = null)
        {
            if (gridRef != IntPtr.Zero) {
                if (cam == null)
                    return OpenVDBPointsAPI.PopulateVertices(gridRef, verts);
                else
                    return OpenVDBPointsAPI.PopulateVertices(gridRef, verts, cam);
            }
            else
                throw new Exception("A PointDataGrid must be loaded in order to get a point count!"); 
        }

        public void OnDisable()
        {
            // if (gridRef != IntPtr.Zero)
                // OpenVDBPointsAPI.FinalizeGrid(gridRef);
        } */
    }
}