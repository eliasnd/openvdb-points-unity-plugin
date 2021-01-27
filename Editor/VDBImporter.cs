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
            meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));;

            ctx.AddObjectToAsset("prefab", gameObject);
            if (mesh != null) ctx.AddObjectToAsset("mesh", mesh);

            Texture2D tex = GenerateTexture(points);
            ctx.AddObjectToAsset("tex", tex);

            // ctx.SetMainObject(gameObject);
        }

        public Mesh GenerateMesh(OpenVDBPoints points) 
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = points.GenerateVertexArray();
            // Set mesh name to grid name here
            mesh.SetVertices(vertices);

            Color[] colors = points.GenerateColorArray();

            mesh.SetColors(colors);

            mesh.SetIndices(
                Enumerable.Range(0, vertices.Length).ToArray(),
                MeshTopology.Points, 0
            );

            return mesh;
        }

        // Might be better to have an explicit colormap like pcx, but for now easier to work with one
        public Texture2D GenerateTexture(OpenVDBPoints points)
        {
            uint count = points.Count;

            Vector3[] vertices = points.GenerateVertexArray();
            Color[] colors = points.GenerateColorArray();

            int texSize = Mathf.CeilToInt(Mathf.Sqrt(2 * count));
            float sizeFactor = count / (texSize * texSize); // First pixel will contain proportion of pixels that are filled


            // First pixel of texture represents scaling factor
            // Points then stored in order: normalized coord, color

            Texture2D tex = new Texture2D(texSize, texSize);

            Color[] pixels = new Color[texSize * texSize];

            for (int i = 0; i < colors.Length-1; i+=2)
            {
                Vector3 vert = vertices[i % count];
                pixels[i] = new Color(vert.x, vert.y, vert.z);
                pixels[i+1] = colors[i % count];
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return tex;
        }

        public void ReadTexture(Texture2D tex, Vector3[] vertices, Color[] colors)
        {
            HashSet<(Vector3 vec, Color col)> pointSet = new HashSet<(Vector3, Color)>();
            
            Color[] pixels = tex.GetPixels();

            for (int i = 0; i < pixels.Length-1; i += 2)
                pointSet.Add((new Vector3(pixels[i].r, pixels[i].g, pixels[i].b), pixels[i+1]));
            
            List<Vector3> vertexList = new List<Vector3>();
            List<Color> colorList = new List<Color>();

            foreach ((Vector3 vec, Color col) in pointSet)
            {
                vertexList.Add(vec);
                colorList.Add(col);
            }

            vertices = vertexList.ToArray();
            colors = colorList.ToArray();
        }
    }
}
