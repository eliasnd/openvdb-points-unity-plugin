using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace OpenVDBPointsUnity
{
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct Vertex
    {
        public Vector3 pos;
        public Color32 color;
    }
}