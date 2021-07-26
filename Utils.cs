using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace OpenVDBPointsUnity
{
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct Point
    {
        public Vector3 pos;
        public Color col;
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct UInt32_3
    {
        public UInt32 x, y, z;
    }

    /* [StructLayout(LayoutKind.Sequential), Serializable]
    public struct Tree3<T>
    {
        public T[] l0;
        public T[] l1;
        public T[] l2;
    } */
}
