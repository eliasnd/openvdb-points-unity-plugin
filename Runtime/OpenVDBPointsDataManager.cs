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
        // static Dictionary<int, IntPtr> map;
        static List<IntPtr> refs;

        static OpenVDBPointsDataManager()
        {
            // Debug.Log("Manager");
            OpenVDBPointsAPI.Initialize();

            // Maps object ids to gridPtrs
            // map = new Dictionary<int, IntPtr>();
            refs = new List<IntPtr>();

            foreach (string guid in AssetDatabase.FindAssets("t:OpenVDBPointsData"))
            {
                // Debug.Log("Asset");
                OpenVDBPointsData asset = AssetDatabase.LoadAssetAtPath<OpenVDBPointsData>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset.FilePath == null || asset.FilePath == "")
                    continue;
                
                // Populate map with existing OpenVDBPointsData objects
                // Debug.Log($"{asset.GetInstanceID()}: {asset.FilePath}");
                // map.Add(asset.GetInstanceID(), OpenVDBPointsAPI.Load(asset.FilePath));
                asset.SetID(Register(asset.FilePath));
            }
        }

        // Register new object created during session
        /* public static void Register(int id, string filePath)
        {
            Debug.Log($"Added {id}, {filePath}");
            map.Add(id, OpenVDBPointsAPI.Load(filePath));
        } */

        public static int Register(string filePath)
        {
            refs.Add(OpenVDBPointsAPI.Load(filePath));
            // Debug.Log("Now refs has count " + refs.Count);
            return refs.Count-1;
        }

        // Get gridPtr from object id
        public static IntPtr Get(int id)
        {
            try {
                return refs[id];
            } catch {
                throw new Exception($"id {id} not registered");
            }
        }

        
    }
}