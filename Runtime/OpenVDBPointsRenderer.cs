using System;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

namespace OpenVDBPointsUnity 
{
    [ExecuteInEditMode]
    public sealed class OpenVDBPointsRenderer : MonoBehaviour 
    {
        #region public
        public OpenVDBPointsData data;

        #endregion

        #region serialized
        [SerializeField] bool frustumCulling;
        [SerializeField] bool lodAccumulation;
        [SerializeField] bool occlusionCulling;

        // Visualization properties
        [SerializeField] Color pointColor;
        [SerializeField] float pointSize;

        #endregion

        #region unserialized
        OpenVDBPointsData oldData;
        bool init = false;

        ComputeBuffer pointBuffer;

        // Mask
        NativeArray<int> layer1Mask;
        NativeArray<int> layer2Mask;
        NativeArray<int> leafNodeMask;
        NativeArray<int> visiblePoints;
        ComputeBuffer indexBuffer;
        int visibleCount;

        Material mat;

        #endregion
        // C++ properties

        // [HideInInspector]

        public void OnRenderObject()
        {
            if (data == null)
                return;

            if (!init || oldData != data) {

                // Lazy init
                init = true;

                pointBuffer = new ComputeBuffer((int)data.Count, System.Runtime.InteropServices.Marshal.SizeOf(new Point()));
                pointBuffer.SetData<Point>(data.Points);

                // Initialize mask
                layer1Mask = new NativeArray<int>((int)data.TreeShape.x, Unity.Collections.Allocator.Persistent);
                layer2Mask = new NativeArray<int>((int)data.TreeShape.y, Unity.Collections.Allocator.Persistent);
                leafNodeMask = new NativeArray<int>((int)data.TreeShape.z, Unity.Collections.Allocator.Persistent);

                visiblePoints = new NativeArray<int>((int)data.Count, Unity.Collections.Allocator.Persistent);
                indexBuffer = new ComputeBuffer((int)data.Count, sizeof(int));
                indexBuffer.SetData<int>(visiblePoints);

                // Initialize material
                mat = new Material(Shader.Find("Custom/PointBuffer"));
                mat.hideFlags = HideFlags.DontSave;
                mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1));
                mat.SetBuffer("_PointBuffer", pointBuffer);
                mat.SetBuffer("_IndexBuffer", indexBuffer);
            }

            // Only need to update vertices if using VDB functionality
            if (frustumCulling || lodAccumulation || occlusionCulling) 
            {
                data.PopulateTreeMask(Camera.current.worldToCameraMatrix * Camera.current.projectionMatrix, frustumCulling, lodAccumulation, occlusionCulling, layer1Mask, layer2Mask, leafNodeMask);
                visibleCount = data.PopulateVisibleIndices(visiblePoints, layer1Mask, layer2Mask, leafNodeMask);
                mat.SetInt("_UseIndexBuffer", 1);
            }
            else
            {
                visibleCount = (int)data.Count;
                mat.SetInt("_UseIndexBuffer", 0);
            }

            mat.SetPass(0);
            mat.SetMatrix("_Transform", transform.localToWorldMatrix);
            if (pointSize != 0)
                mat.SetFloat("_PointSize", pointSize);

            Graphics.DrawProceduralNow(MeshTopology.Points, visibleCount, 1);

            oldData = data;
        }

        void OnDisable()
        {
            Debug.Log("Disable");
            if (pointBuffer != null)
                pointBuffer.Release();
            if (indexBuffer != null)
                indexBuffer.Release();
            if (layer1Mask.IsCreated)
                layer1Mask.Dispose();
            if (layer2Mask.IsCreated)
                layer2Mask.Dispose();
            if (leafNodeMask.IsCreated)
                leafNodeMask.Dispose();
            if (visiblePoints.IsCreated)
                visiblePoints.Dispose();

            init = false;
        }
    }
}
