using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
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
            OpenVDBPointsData pd = new OpenVDBPointsData(absoluteAssetPath);

            GameObject gameObject = new GameObject();
            OpenVDBPointsRenderer renderer = gameObject.AddComponent<OpenVDBPointsRenderer>();
            renderer.data = pd;

            // OpenVDBPointsData test = ScriptableObject.CreateInstance<OpenVDBPointsData>();
            // Debug.Log(test);

            ctx.AddObjectToAsset("prefab", gameObject);
            ctx.AddObjectToAsset("data", pd);
            ctx.SetMainObject(gameObject);

            Debug.Log(renderer.data);

            Debug.Log("Here");

        }
    }
}
