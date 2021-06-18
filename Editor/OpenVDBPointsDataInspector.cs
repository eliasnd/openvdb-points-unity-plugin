using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

namespace OpenVDBPointsUnity
{
    [CustomEditor(typeof(OpenVDBPointsData))]
    public class OpenVDBPointsDataInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            uint count = ((OpenVDBPointsData)target).Count;
            EditorGUILayout.LabelField("Point Count", count.ToString());

            string path = ((OpenVDBPointsData)target).FilePath;
            EditorGUILayout.LabelField("File Path", path);
        }
        
    }
}