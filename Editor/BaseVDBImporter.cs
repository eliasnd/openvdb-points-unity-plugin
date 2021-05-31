using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

using System.Linq;
using System.IO;
namespace OpenVDBPointsUnity
{
    public abstract class BaseVDBImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        
        protected string absoluteAssetPath;

        protected void GetAbsoluteAssetPath(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            // TODO should probably repace with regex
            List<string> folders = Application.dataPath.Split('/').ToList();
            folders.RemoveAt(folders.Count - 1);
            string projectDir = string.Join("/", folders.ToArray());
            absoluteAssetPath = string.Format("{0}/{1}", projectDir, ctx.assetPath);
        }

        public virtual void LogMessage(string message)
        {
            Debug.Log(message);
        }
    }

}