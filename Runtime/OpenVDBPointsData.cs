using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using System.Collections.Generic;
    
namespace OpenVDBPointsUnity 
{
    public class OpenVDBPointsData : ScriptableObject//, ISerializationCallbackReceiver
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

        public NativeArray<Point> Points { get; private set; }

        public NativeArray<int> Layer1Offsets { get; private set; }
        public NativeArray<int> Layer2Offsets { get; private set; }
        public NativeArray<int> LeafNodeOffsets { get; private set; }

        public UInt32_3 TreeShape { get; private set; }

        #endregion

        #region serialized

        [SerializeField] uint count = 0;
        [SerializeField] bool countCalculated = false;
        [SerializeField] bool init = false;


        #endregion 

        #region unserialized

        public IntPtr gridRef { get; private set; }

        #endregion

        public void Awake()
        {
            if (FilePath != null && FilePath != "")
                OnEnable();
        }

        // Possible eventual memory leak -- should make sure grid not already loaded
        public void OnEnable()
        {
            Debug.Log("OnEnable");

            if (FilePath == "" || FilePath == null)
                return;
                // throw new Exception("A file path is required to populate point data. Consider using Init(string filePath) instead.");

            gridRef = OpenVDBPointsAPI.Load(FilePath);

            // Populate vertex array
            Points = new NativeArray<Point>((int)Count, Unity.Collections.Allocator.Persistent);
            OpenVDBPointsAPI.PopulateVertices(gridRef, Points);

            // Populate tree data
            TreeShape = OpenVDBPointsAPI.GetTreeShape(gridRef);

            Layer1Offsets = new NativeArray<int>((int)TreeShape.x, Unity.Collections.Allocator.Persistent);
            Layer2Offsets = new NativeArray<int>((int)TreeShape.y, Unity.Collections.Allocator.Persistent);
            LeafNodeOffsets = new NativeArray<int>((int)TreeShape.z, Unity.Collections.Allocator.Persistent);

            OpenVDBPointsAPI.PopulateTreeOffsets(gridRef, Layer1Offsets, Layer2Offsets, LeafNodeOffsets);

            init = true;
        }

        public NativeArray<int>[] GenerateTreeMask(Matrix4x4 camera, bool frustumCulling, bool lod, bool occlusionCulling)
        {
            NativeArray<int> layer1Mask = new NativeArray<int>((int)TreeShape.x, Unity.Collections.Allocator.Persistent);
            NativeArray<int> layer2Mask = new NativeArray<int>((int)TreeShape.y, Unity.Collections.Allocator.Persistent);
            NativeArray<int> leafNodeMask = new NativeArray<int>((int)TreeShape.z, Unity.Collections.Allocator.Persistent);

            OpenVDBPointsAPI.PopulateTreeMask(gridRef, camera, frustumCulling, lod, occlusionCulling, layer1Mask, layer2Mask, leafNodeMask);

            return new NativeArray<int>[] {layer1Mask, layer2Mask, leafNodeMask};
        }

        public void PopulateTreeMask(Matrix4x4 camera, bool frustumCulling, bool lod, bool occlusionCulling, NativeArray<int> layer1Mask, NativeArray<int> layer2Mask, NativeArray<int> leafNodeMask)
        {
            OpenVDBPointsAPI.PopulateTreeMask(gridRef, camera, frustumCulling, lod, occlusionCulling, layer1Mask, layer2Mask, leafNodeMask);
        }
        
        public int PopulateVisibleIndices(NativeArray<int> target, NativeArray<int> layer1Mask, NativeArray<int> layer2Mask, NativeArray<int> leafNodeMask)
        {
            int i = 0;

            for (int n = 0; n < (int)TreeShape.z-1; n++)
            {
                if (leafNodeMask[n] == 0)
                    continue;

                for (int j = LeafNodeOffsets[n]; j < LeafNodeOffsets[n+1]; j++)
                {
                    target[i] = j;
                    i++;
                }
            }

            if (leafNodeMask[(int)TreeShape.z-1] != 0)
                for (int j = LeafNodeOffsets[(int)TreeShape.z-1]; j < Count; j++)
                {
                    target[i] = j;
                    i++;
                }

            return i;
        }

        public int CountVisiblePoints(NativeArray<int> layer1Mask, NativeArray<int> layer2Mask, NativeArray<int> leafNodeMask)
        {
            int c = 0;
            for (int l1 = 0; l1 < (int)TreeShape.x; l1++)
            {
                if (layer1Mask[l1] == 0)
                    continue;
                
                for (int l2 = Layer1Offsets[l1]; l2 < (l1 == (int)TreeShape.x-1 ? (int)TreeShape.y : Layer1Offsets[l1+1]); l2++)
                {
                    if (layer2Mask[l2] == 0)
                        continue;

                    for (int l3 = Layer2Offsets[l2]; l3 < (l2 == (int)TreeShape.y-1 ? (int)TreeShape.z : Layer2Offsets[l2+1]); l3++)
                    {
                        if (leafNodeMask[l3] == 0)
                            continue;

                        c += (l3 == (int)TreeShape.z-1 ? (int)Count : LeafNodeOffsets[l3+1]) - LeafNodeOffsets[l3];
                    }
                }
            }

            return c;
        }

        public void OnBeforeSerialize()
        {
            Dispose();
        }

        public void OnAfterDeserialize()
        {

        }

        void Dispose()
        {
            Debug.Log("Dispose");
            OpenVDBPointsAPI.FinalizeGrid(gridRef);
            Points.Dispose();
            Layer1Offsets.Dispose();
            Layer2Offsets.Dispose();
            LeafNodeOffsets.Dispose();
        }

        public void OnDisable()
        {
            Debug.Log("Disable");
            Dispose();

        }

        public void OnDestroy()
        {
            Debug.Log("Destroy");
            Dispose();
        }
    }
}