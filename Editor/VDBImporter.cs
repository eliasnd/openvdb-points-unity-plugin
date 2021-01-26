using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor.Experimental.AssetImporters;
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
            meshRenderer.sharedMaterial = new Material(Shader.Find("Specular"));;

            ctx.AddObjectToAsset("prefab", gameObject);
            if (mesh != null) ctx.AddObjectToAsset("mesh", mesh);

            // ctx.SetMainObject(gameObject);
        }

        public Mesh GenerateMesh(OpenVDBPoints points) 
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = points.GenerateVertexArray();
            // Set mesh name to grid name here
            mesh.SetVertices(vertices);

            Color[] colors = new Color[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
                colors[i] = Color.Lerp(Color.red, Color.green, vertices[i].y);

            mesh.SetColors(colors);

            mesh.SetIndices(
                Enumerable.Range(0, vertices.Length).ToArray(),
                MeshTopology.Points, 0
            );

            return mesh;
        }
    }
}
