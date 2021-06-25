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
        public string FilePath; // { get; private set; }
        
        public string Name; // { get; private set; }

        public uint Count
        {
            get
            {
                if (!countCalculated)
                {
                    IntPtr gridRef = GridRef();
                    if (gridRef != IntPtr.Zero) {
                        count = OpenVDBPointsAPI.GetCount(gridRef);
                        countCalculated = true;
                    }
                    else
                        throw new Exception("A PointDataGrid must be loaded in order to get a point count!");
                }
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
        /* #if UNITY_64
            [SerializeField] long gridRef;
        #else
            [SerializeField] int gridRef;
        #endif */

        [SerializeField] uint count = 0;
        [SerializeField] bool countCalculated = false;

        public uint visibleCount;

        #endregion 

        #region unserialized
        [SerializeField] bool init = false;
        [SerializeField] int id = -1;

        #endregion

        /* public void OnEnable()
        {
            OpenVDBPointsAPI.Initialize();
        } */

        private IntPtr GridRef()
        {
            if (!init)
                throw new Exception("A point cloud must be loaded to get a grid ref");
            return OpenVDBPointsDataManager.Get(id);
        }

        public void Init(string filePath)
        {
            Debug.Log("Init");
            if (filePath == "" || filePath == null)
                throw new Exception("A file path is required to populate point data");
            FilePath = filePath;
            Debug.Log(FilePath);
            // id = this.GetInstanceID();
            // OpenVDBPointsDataManager.Register(id, filePath);
            id = OpenVDBPointsDataManager.Register(filePath);
            init = true;
        }

        public void SetID(int id)
        {
            this.id = id;
        }

        public int UpdateVertices(NativeArray<Vertex> verts, Camera cam = null)
        // public int UpdateVertices(NativeArray<Vector3> verts, Camera cam = null)
        {
            if (!init)
                throw new Exception("A point cloud must be loaded to update vertices");

            if (cam == null) {
                OpenVDBPointsAPI.PopulateVertices(GridRef(), verts);
                return (int)Count;
            }
            else
                return (int)OpenVDBPointsAPI.PopulateVertices(GridRef(), verts, cam);
        }

        public void OnDisable()
        {
            if (init) {
                OpenVDBPointsAPI.FinalizeGrid(GridRef()); 
                // OpenVDBPointsDataManager.Deregister(this.GetInstanceID());
            }

            // init = false;

        }
    }
}