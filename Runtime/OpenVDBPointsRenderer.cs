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
        ComputeBuffer accumulatedPointBuffer;
        Point[] points;

        ComputeBuffer leafNodeOffsetBuffer;

        // Mask
        NativeArray<int> layer1Mask;
        NativeArray<int> layer2Mask;
        NativeArray<int> leafNodeMask;
        ComputeBuffer leafNodeMaskBuffer;
        NativeArray<int> visiblePoints;

        public ComputeShader computeShader;
        int kernelHandle;
        ComputeBuffer indexBuffer;
        int visibleCount;

        Material mat;

        #endregion
        // C++ properties

        // [HideInInspector]

        public void OnRenderObject()
        {
            var dataWatch = new System.Diagnostics.Stopwatch();
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            if (data == null || !data.Init || computeShader == null)
                return;

            if (!init || oldData != data) {

                Debug.Log("Initializing");

                // Lazy init
                init = true;

                pointBuffer = new ComputeBuffer((int)data.Count, System.Runtime.InteropServices.Marshal.SizeOf(new Point()));
                pointBuffer.SetData<Point>(data.Points);

                accumulatedPointBuffer = new ComputeBuffer((int)(data.TreeShape.x + data.TreeShape.y + data.TreeShape.z), System.Runtime.InteropServices.Marshal.SizeOf(new Point()));
                accumulatedPointBuffer.SetData<Point>(data.AccumulatedPoints);
                
                // Test contents of point buffer
                points = new Point[(int)data.Count];
                pointBuffer.GetData(points);
                Debug.Log(points[0].pos);

                // Initialize mask
                layer1Mask = new NativeArray<int>((int)data.TreeShape.x, Unity.Collections.Allocator.Persistent);
                layer2Mask = new NativeArray<int>((int)data.TreeShape.y, Unity.Collections.Allocator.Persistent);
                leafNodeMask = new NativeArray<int>((int)data.TreeShape.z, Unity.Collections.Allocator.Persistent);
                visiblePoints = new NativeArray<int>((int)data.Count, Unity.Collections.Allocator.Persistent);

                // Initialize buffers
                indexBuffer = new ComputeBuffer((int)data.Count, sizeof(int), ComputeBufferType.Append);
                // indexBuffer = new ComputeBuffer((int)data.Count, sizeof(int));
                // indexBuffer.SetData<int>(visiblePoints);

                int[] debug = new int[(int)data.TreeShape.z];

                leafNodeOffsetBuffer = new ComputeBuffer((int)(data.TreeShape.z), sizeof(int));
                leafNodeOffsetBuffer.SetData<int>(data.LeafNodeOffsets);

                leafNodeOffsetBuffer.GetData(debug);

                leafNodeMaskBuffer = new ComputeBuffer((int)(data.TreeShape.z), sizeof(int));
                leafNodeMaskBuffer.SetData<int>(leafNodeMask);

                leafNodeMaskBuffer.GetData(debug);


                // Initialize material
                Debug.Log(Shader.Find("Custom/PointBuffer"));
                mat = new Material(Shader.Find("Custom/PointBuffer"));
                mat.hideFlags = HideFlags.DontSave;
                mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1));
                mat.SetBuffer("_PointBuffer", pointBuffer);
                mat.SetBuffer("_AccumulatedBuffer", accumulatedPointBuffer);
                mat.SetBuffer("_IndexBuffer", indexBuffer);

                // Initialize compute shader
                // computeShader = (ComputeShader)Resources.Load("Runtime/Shaders/VisiblePoints.compute");
                kernelHandle = computeShader.FindKernel("CSMain");
                computeShader.SetInts("_GroupDimensions", (int)Mathf.Ceil(data.TreeShape.z / 64.0f), 1, 1);
                computeShader.SetInts("_TreeShape", (int)data.TreeShape.x, (int)data.TreeShape.y, (int)data.TreeShape.z);
                computeShader.SetBuffer(kernelHandle, "_LeafNodeOffsets", leafNodeOffsetBuffer);
                computeShader.SetBuffer(kernelHandle, "_LeafNodeMask", leafNodeMaskBuffer);
                computeShader.SetBuffer(kernelHandle, "_IndexBuffer", indexBuffer);
            }

            Matrix4x4 mvp = Camera.current.projectionMatrix * Camera.current.worldToCameraMatrix * transform.localToWorldMatrix;

            // Only need to update vertices if using VDB functionality
            if (frustumCulling || lodAccumulation || occlusionCulling) 
            {
                // Camera cam = Camera.main; // Uncomment this to visualize frustum culling in Scene view
                Camera cam = Camera.current; 

                dataWatch.Start();

                data.PopulateTreeMask(
                    transform.localToWorldMatrix.transpose,
                    cam.worldToCameraMatrix.transpose,
                    cam.projectionMatrix.transpose,
                    frustumCulling, lodAccumulation, occlusionCulling, layer1Mask, layer2Mask, leafNodeMask
                );

                leafNodeMaskBuffer.SetData<int>(leafNodeMask);

                visibleCount = data.CountVisiblePoints(layer1Mask, layer2Mask, leafNodeMask);

                dataWatch.Stop();
                Debug.Log("Populate tree mask time: " + dataWatch.ElapsedMilliseconds);
                dataWatch.Reset();
                dataWatch.Start();

                // visibleCount = data.PopulateVisibleIndices(visiblePoints, layer1Mask, layer2Mask, leafNodeMask);
                // indexBuffer.SetData<int>(visiblePoints);
                computeShader.Dispatch(kernelHandle, (int)Mathf.Ceil(data.TreeShape.z / 64.0f), 1, 1);
                // int[] indices = new int[visibleCount];
                // indexBuffer.GetData(indices);

                dataWatch.Stop();
                Debug.Log("Populate visible indices time: " + dataWatch.ElapsedMilliseconds);

                mat.SetInt("_UseIndexBuffer", 1);
            }
            else
            {
                visibleCount = (int)data.Count;
                mat.SetInt("_UseIndexBuffer", 0);
            }


            // visibleCount = (int)data.Count;
            // mat.SetInt("_UseIndexBuffer", 0);

            mat.SetPass(0);
            mat.SetMatrix("_Transform", transform.localToWorldMatrix);
            if (pointSize != 0)
                mat.SetFloat("_PointSize", pointSize);

            dataWatch.Reset();
            dataWatch.Start();

            Graphics.DrawProceduralNow(MeshTopology.Points, visibleCount, 1);

            dataWatch.Stop();
            Debug.Log("Draw time for " + visibleCount + " points: " + dataWatch.ElapsedMilliseconds);

            // Test contents of point buffer
            // pointBuffer.GetData(points);

            indexBuffer.SetCounterValue(0);
            oldData = data;

            watch.Stop();
            Debug.Log("Rendering " + visibleCount + " points in " + watch.ElapsedMilliseconds + "ms");
        }

        void Dispose()
        {
            Debug.Log("Disable");
            if (pointBuffer != null)
                pointBuffer.Release();
            if (accumulatedPointBuffer != null)
                accumulatedPointBuffer.Release();
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

        public void OnDisable()
        {
            Dispose();
        }

        public void OnDestroy()
        {
            Dispose();
        }
    }
}
