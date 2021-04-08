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

        /* public void Init()
        {
            if (points == null || init)
                return;

            vertices = new NativeArray<Vertex>((int)points.Count, Allocator.Temp);

            if (frustumCulling)
                visibleCount = points.PopulateVertices(vertices, Camera.main);
            else
                visibleCount = points.PopulateVertices(vertices);

            mesh = new Mesh();
            mesh.SetVertexBufferParams((int)points.Count, new[]{
                new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            });

            mesh.SetVertexBufferData(vertices, 0, 0, (int)points.Count);

            mesh.SetIndices(
                Enumerable.Range(0, (int)visibleCount).ToArray(),
                MeshTopology.Points, 0
            );

            init = true;
        }

        void OnRenderObject()
        {
            if (points == null)
            {
                Debug.Log("Null");
                return;
            }

            Graphics.DrawMeshNow(mesh, transform.position, transform.rotation);

        } */
    }
}