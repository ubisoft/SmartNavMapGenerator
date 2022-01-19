using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LaForge.MapGenerator
{
    [System.Serializable]
    public class GroundProperties : System.IEquatable<GroundProperties>
    {
        public enum GroundType
        {
            Lava,
            Water,
            Unknown
        }

        [ReadOnly]
        [SerializeField]
        private GroundType _type;

        private Material _material;

        public GroundType Type => _type;

        public Material Material => _material;

        public GroundProperties(GroundType type)
        {
            _type = type;
            LoadMaterial();
        }

        public void LoadMaterial()
        {
            _material = Resources.Load<Material>($"Materials/{_type}");
        }

        public bool Equals(GroundProperties other)
        {
            return this.Type == other.Type;
        }
    }
}
