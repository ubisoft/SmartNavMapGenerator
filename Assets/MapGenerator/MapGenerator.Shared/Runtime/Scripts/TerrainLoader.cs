using UnityEngine;
using UnityEditor;

namespace LaForge.MapGenerator
{
    public class TerrainLoader : MonoBehaviour
    {
        public GameObject Map;
        private Mesh currentMesh;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private Material mat;
        private const string DEFAULT_GRADIENT = "Map Gradients/default";
        private Texture2D texGradient;

        // Use this for initialization
        private void Awake()
        {
            if (!Map)
                Map = this.gameObject;

            if ((meshRenderer = Map.GetComponent<MeshRenderer>()) == null)
                meshRenderer = Map.AddComponent<MeshRenderer>();

            if (!Application.isBatchMode)
            {
                mat = new Material(Shader.Find("MapGenerator.Shared/Terrain"));
                LoadGradientFromResources(DEFAULT_GRADIENT);
                meshRenderer.material = mat;
            }

            currentMesh = new Mesh();
            currentMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            if ((meshFilter = Map.GetComponent<MeshFilter>()) == null)
            {
                meshFilter = Map.AddComponent<MeshFilter>();
                meshFilter.mesh = currentMesh;
            }

            if ((meshCollider = Map.GetComponent<MeshCollider>()) == null)
            {
                meshCollider = Map.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = currentMesh;
            }
        }
        public void LoadTerrain(Mesh newMesh)
        {
            ChangeMesh(newMesh);
        }

        public void LoadTerrain(string meshPath)
        {
            Mesh newMesh = MeshSerializer.ReadMesh(System.IO.File.ReadAllBytes(meshPath));
            newMesh.name = System.IO.Path.GetFileNameWithoutExtension(meshPath);
            LoadTerrain(newMesh);
        }

        public void LoadGradient(string path)
        {
            byte[] data = System.IO.File.ReadAllBytes(path);
            texGradient = new Texture2D(50, 1, TextureFormat.RGBA32, false);
            texGradient.LoadImage(data);
        }

        public void LoadGradientFromResources(string name)
        {
            texGradient = Resources.Load<Texture2D>(name);
        }

        private void ChangeMesh(Mesh newMesh)
        {
            currentMesh.Clear();
            currentMesh.name = newMesh.name;
            currentMesh.vertices = newMesh.vertices;
            currentMesh.triangles = newMesh.triangles;
            currentMesh.uv = newMesh.uv;
            currentMesh.uv2 = newMesh.uv2;
            currentMesh.colors = newMesh.colors;
            currentMesh.RecalculateBounds();
            currentMesh.RecalculateNormals();
            meshCollider.enabled = false;
            meshCollider.enabled = true;
        }

        private void Update()
        {
            if (!Application.isBatchMode)
                mat.SetTexture("_MainTex", texGradient);
        }

        public bool HasMesh()
        {
            return meshFilter.sharedMesh.vertexCount > 0;
        }

        public byte[] SerializeTerrain()
        {
            return MeshSerializer.WriteMesh(meshFilter.sharedMesh, false);
        }
    }
}
