using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LaForge.MapGenerator
{
    public class Ground : MonoBehaviour
    {
        [SerializeField]
        private GroundProperties _groundProperties;
        private GroundEvent _enterEvent;
        private GroundEvent _stayEvent;
        private GroundEvent _exitEvent;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private static GameObject _groundPrefab;

        public GroundProperties GroundProperties
        {
            get => _groundProperties;
            set
            {
                _groundProperties = value;
                UpdateMaterial();
            }
        }

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _enterEvent = Resources.Load<GroundEvent>("Events/GroundEnterEvent");
            _stayEvent = Resources.Load<GroundEvent>("Events/GroundStayEvent");
            _exitEvent = Resources.Load<GroundEvent>("Events/GroundExitEvent");
        }

        private void OnCollisionEnter(Collision collision)
        {
            _enterEvent?.Raise(this, collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            _stayEvent?.Raise(this, collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            _exitEvent?.Raise(this, collision);
        }

        public void UpdateMaterial()
        {
            _groundProperties.LoadMaterial();
            _meshRenderer.material = _groundProperties.Material;
        }

        public byte[] Serialize()
        {
            return MeshSerializer.WriteMesh(_meshFilter.sharedMesh, false);
        }

        public static GameObject Load(string path)
        {
            Mesh newMesh = MeshSerializer.ReadMesh(System.IO.File.ReadAllBytes(path));
            newMesh.name = System.IO.Path.GetFileNameWithoutExtension(path);
            return CreateGameObject(newMesh);
        }

        public static GameObject CreateGameObject(Mesh mesh)
        {
            if (!_groundPrefab)
                _groundPrefab = Resources.Load<GameObject>("Prefabs/Ground");
            GameObject gameObject = Instantiate(_groundPrefab);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.Optimize();
            gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size -= new Vector3(0, 0.01f, 0);
            gameObject.AddComponent<NavMeshObstacle>().carving = true;
            return gameObject;
        }
    }
}
