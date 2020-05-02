#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// code taken from
// https://forum.unity.com/threads/solution-save-a-prefab-with-a-generated-mesh-es-without-creating-assets.463496/

namespace PrefabGeneration
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    public class SerializeMesh : MonoBehaviour
    {
        [HideInInspector] [SerializeField] Vector2[] uv;
        [HideInInspector] [SerializeField] Vector3[] vertices;
        [HideInInspector] [SerializeField] int[] triangles;
        [HideInInspector] [SerializeField] bool serialized = false;
        // Use this for initialization

        void Awake()
        {
            if (serialized)
            {
                GetComponent<MeshFilter>().mesh = Rebuild();
            }
        }

        void Start()
        {
            if (serialized) return;

            Serialize();
        }

        public void Serialize()
        {
            var mesh = GetComponent<MeshFilter>().mesh;

            uv = mesh.uv;
            vertices = mesh.vertices;
            triangles = mesh.triangles;

            serialized = true;
        }

        public Mesh Rebuild()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SerializeMesh))]
    class SerializeMeshEditor : Editor
    {
        SerializeMesh _obj;

        void OnSceneGUI()
        {
            _obj = (SerializeMesh)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Rebuild"))
            {
                if (_obj)
                {
                    _obj.gameObject.GetComponent<MeshFilter>().mesh = _obj.Rebuild();
                }
            }

            if (GUILayout.Button("Serialize"))
            {
                if (_obj)
                {
                    _obj.Serialize();
                }
            }
        }
    }
#endif
}