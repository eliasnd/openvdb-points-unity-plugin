using System.Linq;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

namespace OpenVDBPointsUnity 
{
    [ExecuteInEditMode]
    public sealed class OpenVDBPointsRenderer : MonoBehaviour 
    {
        // C++ properties
        [SerializeField] bool frustumCulling;
        [SerializeField] bool lodAccumulation;

        // Visualization properties
        [SerializeField] Color pointColor;
        [SerializeField] float pointSize;

        // [HideInInspector]
        public OpenVDBPointsData data;

        private bool init = false;
        private NativeArray<Vertex> vertices;
        unsafe private void* vertPtr;
        private uint visibleCount;
        // private Mesh mesh;
        ComputeBuffer buffer;
        Material mat;

        void OnRenderObject()
        {
            if (data == null)
                return;

            if (!init) {

                // Lazy init
                init = true;

                // Initialize vertex arr and buffer
                vertices = new NativeArray<Vertex>((int)data.Count, Allocator.Temp);
                buffer = new ComputeBuffer((int)data.Count, sizeof(float) * 3);
                buffer.SetData<Vertex>(vertices);

                // Initialize material
                mat = new Material(Shader.Find("Custom/Point"));
                mat.hideFlags = HideFlags.DontSave;
                mat.EnableKeyword("_COMPUTE_BUFFER");
                mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1));
                mat.SetBuffer("_PointBuffer", buffer);


            }

            // Only need to update vertices if using VDB functionality
            if (frustumCulling || lodAccumulation) 
                data.UpdateVertices(vertices, Camera.main);

            mat.SetMatrix("_Transform", transform.localToWorldMatrix);
            Graphics.DrawProceduralNow(MeshTopology.Points, (int)data.visibleCount, 1);



        }
    }
}