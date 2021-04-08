using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
    
namespace OpenVDBPointsUnity 
{
    [Serializable]
    public class OpenVDBPointsData : ScriptableObject
    {
        /// <summary>The absolute path to the .vdb file on disk.</summary>
        /// <remarks>Note- must be an absolute path.</remarks>
        public string FilePath { get; set; }
        
        public string Name { get; private set; }
        /// <summary>The default grid name used to access the PointDataGrid.</summary>
        // private const string gridName = "";
        [SerializeField] private string gridName = "Points";
        /// <summary>Pointer to native SharedPointDataGrid.</summary>
        [SerializeField] private IntPtr gridRef;

        [SerializeField] private uint count = 0;
        [SerializeField] private bool countCalculated = false;

        public uint Count
        {
            get
            {
                if (!countCalculated)
                    if (gridRef != IntPtr.Zero)
                        count = OpenVDBPointsAPI.GetCount(gridRef);
                    else
                        throw new Exception("A PointDataGrid must be loaded in order to get a point count!");
                return count;
            }
        }

        public OpenVDBPointsData(string filePath)
        {
            FilePath = filePath;
            OpenVDBPointsAPI.Initialize();
            gridRef = OpenVDBPointsAPI.Load(FilePath);
        }

        ~OpenVDBPointsData()
        {
            Debug.Log("Delete");
            if (gridRef != IntPtr.Zero)
            {
                OpenVDBPointsAPI.FinalizeGrid(gridRef);
            }
        }
    }
}