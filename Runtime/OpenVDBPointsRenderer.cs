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

        // Visualization properties
        [SerializeField] Color pointColor;
        [SerializeField] float pointSize;

        #endregion

        #region unserialized
        OpenVDBPointsData oldData;
        bool init = false;

        NativeArray<Vertex> vertices;
        // NativeArray<Vector3> vertices;
        ComputeBuffer buffer;
        uint visibleCount;

        Material mat;

        #endregion
        // C++ properties

        // [HideInInspector]


        void OnRenderObject()
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
                data.PopulateVertices(vertices);
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
            // if (frustumCulling || lodAccumulation) 
                // data.UpdateVertices(vertices, Camera.current);

            mat.SetPass(0);
            mat.SetMatrix("_Transform", transform.localToWorldMatrix);
            if (pointSize != 0)
                mat.SetFloat("_PointSize", pointSize);
            // mat.SetBuffer("_Buffer", buffer);
            // Graphics.DrawProcedural(mat, MeshTopology.Points, (int)data.visibleCount, 1);
            Graphics.DrawProceduralNow(MeshTopology.Points, (int)buffer.count, 1);


            oldData = data;
        }

        void OnDisable()
        {
            Debug.Log("Disable");
            if (buffer != null)
                buffer.Release();
            if (vertices.IsCreated)
                vertices.Dispose();
            init = false;
        }
    }
}
