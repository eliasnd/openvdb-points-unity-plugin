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
            var dataWatch = new System.Diagnostics.Stopwatch();
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            if (data == null || !data.Init)
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
                indexBuffer = new ComputeBuffer((int)data.Count, sizeof(int));
                indexBuffer.SetData<int>(visiblePoints);

                // Initialize material
                Debug.Log(Shader.Find("Custom/PointBuffer"));
                mat = new Material(Shader.Find("Custom/PointBuffer"));
                mat.hideFlags = HideFlags.DontSave;
                mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1));
                mat.SetBuffer("_PointBuffer", pointBuffer);
                mat.SetBuffer("_AccumulatedBuffer", accumulatedPointBuffer);
                mat.SetBuffer("_IndexBuffer", indexBuffer);
            }

            Matrix4x4 mvp = Camera.current.projectionMatrix * Camera.current.worldToCameraMatrix * transform.localToWorldMatrix;

            // Only need to update vertices if using VDB functionality
            if (frustumCulling || lodAccumulation || occlusionCulling) 
            {
                dataWatch.Start();

                data.PopulateTreeMask(
                    transform.localToWorldMatrix.transpose,
                    Camera.current.worldToCameraMatrix.transpose,
                    Camera.current.projectionMatrix.transpose,
                    frustumCulling, lodAccumulation, occlusionCulling, layer1Mask, layer2Mask, leafNodeMask
                );

                dataWatch.Stop();
                Debug.Log("Populate tree mask time: " + dataWatch.ElapsedMilliseconds);
                dataWatch.Reset();
                dataWatch.Start();

                visibleCount = data.PopulateVisibleIndices(visiblePoints, layer1Mask, layer2Mask, leafNodeMask);

                dataWatch.Stop();
                Debug.Log("Populate visible indices time: " + dataWatch.ElapsedMilliseconds);

                indexBuffer.SetData<int>(visiblePoints);
                // Debug.Log(visibleCount);
                mat.SetInt("_UseIndexBuffer", 1);
                // Debug.Log("C#");
                // Debug.Log(mvp);

                // Debug.Log(Camera.current.projectionMatrix * Camera.current.worldToCameraMatrix * transform.localToWorldMatrix);

                /* Vector3[] corners = {
                    new Vector3(0.000000f, 0.000000f, 93.020159f),
                    new Vector3(0.000000f, 0.000000f, 124.019308f),
                    new Vector3(0.000000f, 30.999150f, 93.020159f ),
                    new Vector3(0.000000f, 30.999150f, 124.019308f ),
                    new Vector3(30.999150f, 0.000000f, 93.020159f ),
                    new Vector3(30.999150f, 0.000000f, 124.019308f ),
                    new Vector3(30.999150f, 30.999150f, 93.020159f ),
                    new Vector3(30.999150f, 30.999150f, 124.019308f )
                };


                Debug.Log("Corners clip space:");
                for (int i = 0; i < 8; i++) {
                    // Debug.Log("Start");
                    // Debug.Log(corners[i]);
                    Vector4 clip = mvp * new Vector4(corners[i].x, corners[i].y, corners[i].z, 1);
                    // Debug.Log("Clipped");
                    // Debug.Log(clip);
                    Vector3 ndc = new Vector3(clip.w / clip.z, clip.x / clip.z, clip.y / clip.z);
                    Debug.Log(corners[i].ToString() + " --> " + ndc.ToString());
                } */
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

            oldData = data;

            watch.Stop();
            Debug.Log("Render time: " + watch.ElapsedMilliseconds);
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
