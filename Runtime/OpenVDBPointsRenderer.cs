using System;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

namespace OpenVDBPointsUnity 
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct VDBRenderingData
    {
        public IntPtr gridRef;
        public void* vertexPtr;
        public Matrix4x4 cam;
        public bool frustumCulling;
        public bool lod;
        public OpenVDBPointsAPI.LoggingCallback cb;
    }

    enum CustomRenderEvent
    {
        // 3245 is a random number I made up.
        // I figured it could be useful to send an event id
        // to the native plugin which corresponds to when
        // the mesh is being rendered in the render pipeline.
        AfterForwardOpaque = 3245,
    }

    [ExecuteInEditMode]
    public sealed class OpenVDBPointsRenderer : MonoBehaviour 
    {
        #region public
        public OpenVDBPointsData data;

        #endregion

        #region serialized
        [SerializeField] bool frustumCulling;
        [SerializeField] bool lodAccumulation;

        // Visualization properties
        [SerializeField] Color pointColor;
        [SerializeField] float pointSize;

        #endregion

        #region unserialized
        OpenVDBPointsData oldData;
        bool init = false;

        uint visibleCount;
        NativeArray<Vertex> vertices;
        // NativeArray<Vector3> vertices;
        ComputeBuffer pointBuffer;
        VDBRenderingData renderingData;
        IntPtr renderingDataPtr;
        CommandBuffer cmdBuffer;

        Material mat;

        #endregion
        // C++ properties

        // [HideInInspector]


        void Init()
        {
            if (vertices.IsCreated)
                vertices.Dispose();
            vertices = new NativeArray<Vertex>((int)data.Count, Allocator.Persistent);

            pointBuffer = new ComputeBuffer((int)data.Count, sizeof(float) * 7);
            pointBuffer.SetData<Vertex>(vertices);

            renderingDataPtr = Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(new VDBRenderingData()));

            mat = new Material(Shader.Find("Custom/PointBuffer"));
            mat.hideFlags = HideFlags.DontSave;
            mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1));
            mat.SetBuffer("_Buffer", pointBuffer);

            init = true;
        }

        unsafe void OnRenderObject()
        {
            if (data != null && (!init || oldData != data))
                Init();

            renderingData = new VDBRenderingData();
            renderingData.gridRef = data.GridRef();
            renderingData.vertexPtr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(vertices);
            renderingData.cam = Camera.current.projectionMatrix * Camera.current.worldToCameraMatrix;
            renderingData.frustumCulling = frustumCulling;
            renderingData.lod = lodAccumulation;
            renderingData.cb = OpenVDBPointsAPI.LogMessage;
            Marshal.StructureToPtr(renderingData, renderingDataPtr, true);

            cmdBuffer = new CommandBuffer();
            cmdBuffer.IssuePluginEventAndData(OpenVDBPointsAPI.GetRenderCallback(), (int)CustomRenderEvent.AfterForwardOpaque, renderingDataPtr);
            cmdBuffer.SetComputeBufferData(pointBuffer, vertices);
            cmdBuffer.DrawProcedural(Matrix4x4.identity, mat, 0, MeshTopology.Points, (int)visibleCount);

            Graphics.ExecuteCommandBuffer(cmdBuffer);

            oldData = data;
        }

        /* void OnRenderObject()
        {
            if (data == null)
                return;

            if (!init || oldData != data) {

                // Lazy init
                init = true;

                // Initialize vertex arr and buffer
                if (vertices.IsCreated)
                    vertices.Dispose();
                vertices = new NativeArray<Vertex>((int)data.Count, Allocator.Persistent);
                // vertices = new NativeArray<Vector3>((int)data.Count, Allocator.Temp);
                data.UpdateVertices(vertices);
                Debug.Log(vertices[0]);

                buffer = new ComputeBuffer((int)data.Count, System.Runtime.InteropServices.Marshal.SizeOf(new Vertex()));
                buffer.SetData<Vertex>(vertices);
                // buffer = new ComputeBuffer((int)data.Count, sizeof(float) * 3);
                // buffer.SetData<Vector3>(vertices);
                
                // vertices = new NativeArray<Vector3>((int)data.Count, Allocator.Temp);
                // pointbuffer = new ComputeBuffer((int)data.Count, sizeof(float) * 3);
                // pointbuffer.SetData<Vector3>(vertices);

                // Initialize material
                Debug.Log(Shader.Find("Custom/PointBuffer"));
                mat = new Material(Shader.Find("Custom/PointBuffer"));
                mat.hideFlags = HideFlags.DontSave;
                mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1));
                mat.SetBuffer("_Buffer", buffer);
            }

            // Only need to update vertices if using VDB functionality
            if (frustumCulling || lodAccumulation) 
                data.UpdateVertices(vertices, Camera.current);

            mat.SetPass(0);
            mat.SetMatrix("_Transform", transform.localToWorldMatrix);
            if (pointSize != 0)
                mat.SetFloat("_PointSize", pointSize);
            // mat.SetBuffer("_Buffer", buffer);
            // Graphics.DrawProcedural(mat, MeshTopology.Points, (int)data.visibleCount, 1);
            Graphics.DrawProceduralNow(MeshTopology.Points, (int)buffer.count, 1);


            oldData = data;
        } */

        void OnDisable()
        {
            Debug.Log("Disable");
            if (pointBuffer != null)
                pointBuffer.Release();
            if (cmdBuffer != null)
                cmdBuffer.Release();
            if (vertices.IsCreated)
                vertices.Dispose();
            init = false;
        }
    }
}
