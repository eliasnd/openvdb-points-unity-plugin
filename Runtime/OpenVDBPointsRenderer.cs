using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using UnityEditor.AssetImporters;
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
        [SerializeField] bool init;

        private uint visibleCount;
        private Mesh mesh;
        Material mat;

        public void Init()
        {
            Mesh mesh = new Mesh();

            mesh.indexFormat = data.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;

            VertexAttributeDescriptor[] layout = new[] {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4)
            };

            mesh.SetVertexBufferParams((int)data.Count, layout);
            mesh.SetVertexBufferData(data.GetVertices(), 0, 0, (int)data.Count);

            mesh.SetIndices(
                Enumerable.Range(0, (int)data.Count).ToArray(),
                MeshTopology.Points, 0
            );

            MeshFilter filter = gameObject.GetComponent<MeshFilter>();
            if (filter == null)
                filter = gameObject.AddComponent<MeshFilter>();

            filter.mesh = mesh;

            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = gameObject.AddComponent<MeshRenderer>();

            renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/OpenVDBPoints/Editor/Materials/DefaultPoint.mat");

            init = true;
        }

        void Update()
        {
            if (data != null && !init)
                Init();
        }
    }
}
