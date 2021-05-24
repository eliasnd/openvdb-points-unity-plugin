using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace OpenVDBPointsUnity
{
    [InitializeOnLoad]
    class OpenVDBPointsDataManager
    {

        /*#if UNITY_64
            static Dictionary<string, long> map;
        #else
            static Dictionary<string, int> map;
        #endif*/

        static Dictionary<int, IntPtr> map;

        static OpenVDBPointsDataManager()
        {
            OpenVDBPointsAPI.Initialize();

            map = new Dictionary<int, IntPtr>();

            foreach (string guid in AssetDatabase.FindAssets("t:OpenVDBPointsData", new string[] {"Assets"}))
            {
                OpenVDBPointsData asset = AssetDatabase.LoadAssetAtPath<OpenVDBPointsData>(AssetDatabase.GUIDToAssetPath(guid));
                
                map.Add(asset.GetInstanceID(), OpenVDBPointsAPI.Load(asset.FilePath));
                Debug.Log($"{asset.GetInstanceID()}: {map[asset.GetInstanceID()]}");
            }
        }

        public static void Register(int id, string filePath)
        {
            map.Add(id, OpenVDBPointsAPI.Load(filePath));
        }

        public static IntPtr Get(int id)
        {
            return map[id];
        }
        
    }
}