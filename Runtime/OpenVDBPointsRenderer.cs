using UnityEngine;

namespace OpenVDBPointsUnity 
{
    [ExecuteInEditMode]
    public sealed class OpenVDBPointsRenderer : MonoBehaviour 
    {
        // C++ properties
        [SerializeField] bool frustumCulling;
        [SerializeField] bool voxelize;     // To-do

        // Visualization properties
        [SerializeField] Color pointColor;
        [SerializeField] float pointSize;

        // [HideInInspector]
        [SerializeField] public OpenVDBPoints points;

        private bool init = false;
        private Mesh mesh;

        void OnRenderObject()
        {
            if (points == null)
            {
                Debug.Log("Null");
                return;
            }

            if (!frustumCulling)
            {
                if (!init)
                {
                    mesh = points.InitializeMesh();
                    init = true;
                }

                Graphics.DrawMeshNow(mesh, transform.position, transform.rotation);
            }
        }
    }
}