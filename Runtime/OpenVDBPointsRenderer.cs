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
        [SerializeField] bool voxelize;     // To-do

        // Visualization properties
        [SerializeField] Color pointColor;
        [SerializeField] float pointSize;

        // [HideInInspector]
        public OpenVDBPointsData data;

        private bool init = false;
        private NativeArray<Vertex> vertices;
        unsafe private void* vertPtr;
        private uint visibleCount;
        private Mesh mesh;

        public void Init()
        {
            /* if (data == null || init)
                return;

            vertices = new NativeArray<Vertex>((int)data.Count, Allocator.Temp);

            if (frustumCulling)
                visibleCount = data.PopulateVertices(vertices, Camera.main);
            else
                visibleCount = data.PopulateVertices(vertices);

            mesh = new Mesh();
            mesh.SetVertexBufferParams((int)data.Count, new[]{
                new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            });

            mesh.SetVertexBufferData(vertices, 0, 0, (int)data.Count);

            mesh.SetIndices(
                Enumerable.Range(0, (int)visibleCount).ToArray(),
                MeshTopology.Points, 0
            );

            init = true; */
        }

        void OnRenderObject()
        {
            if (!init)
                Init();

            Graphics.DrawMeshNow(mesh, transform.position, transform.rotation);

        }
    }
}