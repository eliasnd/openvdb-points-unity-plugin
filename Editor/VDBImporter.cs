using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.AssetImporters;
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
            Debug.Log("Import");
            GetAbsoluteAssetPath(ctx);

            OpenVDBPointsData pd = ImportAsPointsData(absoluteAssetPath);

            // GameObject gameObject = new GameObject();
            // OpenVDBPointsRenderer renderer = gameObject.AddComponent<OpenVDBPointsRenderer>();
            // renderer.data = pd;

            // ctx.AddObjectToAsset("prefab", gameObject);
            ctx.AddObjectToAsset("data", pd);
            // ctx.SetMainObject(gameObject);
            ctx.SetMainObject(pd);
        }

        OpenVDBPointsData ImportAsPointsData(string path)
        {
            OpenVDBPointsData pd = ScriptableObject.CreateInstance<OpenVDBPointsData>();
            pd.Init(path);
            // pd.FilePath = path;
            return pd;
        }
    }
}