#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class SerializeMesh : MonoBehaviour
{
    [HideInInspector] [SerializeField] string meshName;
    [HideInInspector] [SerializeField] Vector2[] uv;
    [HideInInspector] [SerializeField] Vector2[] uv2;
    [HideInInspector] [SerializeField] Vector3[] verticies;
    [HideInInspector] [SerializeField] int[] triangles;
    [HideInInspector] [SerializeField] bool serialized = false;
    // Use this for initialization

    void Awake()
    {
        if (serialized)
        {
            Mesh mesh = Rebuild();
            GetComponent<MeshFilter>().sharedMesh = mesh;
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.sharedMesh = mesh;
        }
    }

    void Start()
    {
        if (serialized) return;

        Serialize();
    }

    public void Serialize()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;

        uv = mesh.uv;
        uv2 = mesh.uv2;
        verticies = mesh.vertices;
        triangles = mesh.triangles;
        meshName = mesh.name;

        serialized = true;
    }

    public Mesh Rebuild()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.uv2 = uv2;
        mesh.name = meshName;

        mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SerializeMesh))]
class SerializeMeshEditor : Editor
{
    SerializeMesh obj;

    void OnSceneGUI()
    {
        obj = (SerializeMesh)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Rebuild"))
        {
            if (obj)
            {
                obj.gameObject.GetComponent<MeshFilter>().mesh = obj.Rebuild();
            }
        }

        if (GUILayout.Button("Serialize"))
        {
            if (obj)
            {
                obj.Serialize();
            }
        }
    }
}
#endif
