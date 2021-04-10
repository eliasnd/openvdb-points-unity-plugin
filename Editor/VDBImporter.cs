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

            GameObject gameObject = new GameObject();
            OpenVDBPointsData pd = ImportAsPointsData(absoluteAssetPath);

            // OpenVDBPointsRenderer renderer = gameObject.AddComponent<OpenVDBPointsRenderer>();
            // renderer.data = pd;

            ctx.AddObjectToAsset("prefab", gameObject);
            ctx.AddObjectToAsset("data", pd);
            ctx.SetMainObject(gameObject);
        }

        OpenVDBPointsData ImportAsPointsData(string path)
        {
            OpenVDBPointsData pd = ScriptableObject.CreateInstance<OpenVDBPointsData>();
            pd.Load(path);
            // pd.FilePath = path;
            return pd;
        }
    }
}
