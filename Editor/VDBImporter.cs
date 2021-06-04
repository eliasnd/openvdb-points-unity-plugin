using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


using System.Linq;
using System;

// TODO make an abstract base class that this inherits from
namespace OpenVDBPointsUnity
{
    [ScriptedImporter(1, "vdb")]
    public class VDBImporter : BaseVDBImporter
    {

        public override void OnImportAsset(AssetImportContext ctx)
        {
            GetAbsoluteAssetPath(ctx);
            OpenVDBPoints points = new OpenVDBPoints(absoluteAssetPath);
            points.Load();
            uint count = points.Count;
            Debug.Log(string.Format("Total Points: {0}", count.ToString()));

            var gameObject = new GameObject();
            Mesh mesh = GenerateMesh(points);

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/OpenVDBPoints/Editor/Materials/DefaultPoint.mat"); // new Material(Shader.Find("Custom/Point"));;

            ctx.AddObjectToAsset("prefab", gameObject);
            if (mesh != null) ctx.AddObjectToAsset("mesh", mesh);

            // ctx.SetMainObject(gameObject);
        }

        public Mesh GenerateMesh(OpenVDBPoints points) 
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = points.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;

            (Vector3[] verts, Color32[] cols) arrs = points.GenerateArrays();
            Vector3[] vertices = arrs.verts;
            mesh.SetVertices(vertices);
            Color32[] colors = arrs.cols;
            mesh.SetColors(colors);

            mesh.SetIndices(
                Enumerable.Range(0, vertices.Length).ToArray(),
                MeshTopology.Points, 0
            );

            return mesh;
        }
    }
}
