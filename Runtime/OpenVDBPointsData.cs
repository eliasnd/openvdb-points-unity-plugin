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
        public string FilePath { get; private set; }
        
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

        [SerializeField] bool init = false;
        public uint visibleCount;

        #endregion 

        /* public void OnEnable()
        {
            OpenVDBPointsAPI.Initialize();
        } */

        private IntPtr GridRef()
        {
            return OpenVDBPointsDataManager.Get(this.GetInstanceID());
        }

        public void Init(string filePath)
        {
            FilePath = filePath;
            Debug.Log("Calling register on id " + this.GetInstanceID() + ", file " + filePath);
            OpenVDBPointsDataManager.Register(this.GetInstanceID(), filePath);
            init = true;
        }

        public int UpdateVertices(NativeArray<Vertex> verts, Camera cam = null)
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
            if (init) 
                OpenVDBPointsAPI.FinalizeGrid(GridRef()); 

        }
    }
}