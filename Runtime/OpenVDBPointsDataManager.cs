using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace OpenVDBPointsUnity
{
    /*
        A class for managing gridPtrs in a single editor session (hopefully will work at runtime too)
        Stores an internal map from object instanceids to gridPtrs loaded at editor launch
        InitializeOnLoad attribute calls static constructor as soon as session launches
    */
    [InitializeOnLoad]
    class OpenVDBPointsDataManager
    {
        static Dictionary<int, IntPtr> map;

        static OpenVDBPointsDataManager()
        {
            OpenVDBPointsAPI.Initialize();

            // Maps object ids to gridPtrs
            map = new Dictionary<int, IntPtr>();

            foreach (string guid in AssetDatabase.FindAssets("t:OpenVDBPointsData", new string[] {"Assets"}))
            {
                OpenVDBPointsData asset = AssetDatabase.LoadAssetAtPath<OpenVDBPointsData>(AssetDatabase.GUIDToAssetPath(guid));
                
                // Populate map with existing OpenVDBPointsData objects
                map.Add(asset.GetInstanceID(), OpenVDBPointsAPI.Load(asset.FilePath));
                Debug.Log($"{asset.GetInstanceID()}: {map[asset.GetInstanceID()]}");
            }
        }

        // Register new object created during session
        public static void Register(int id, string filePath)
        {
            map.Add(id, OpenVDBPointsAPI.Load(filePath));
        }

        // Get gridPtr from object id
        public static IntPtr Get(int id)
        {
            return map[id];
        }
        
    }
}