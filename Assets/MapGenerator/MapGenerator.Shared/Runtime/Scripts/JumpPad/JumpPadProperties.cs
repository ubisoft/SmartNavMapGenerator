using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LaForge.MapGenerator
{
    [System.Serializable]
    public class JumpPadProperties : System.IEquatable<JumpPadProperties>
    {
        [SerializeField]
        [ReadOnly]
        private float _height;
        [SerializeField]
        [ReadOnly]
        private Vector3 _position;

        public float Height
        {
            get
            {
                return _height;
            }
        }

        public Vector3 Position
        {
            get
            {
                return _position;
            }
        }

        public JumpPadProperties(float height, Vector3 position)
        {
            _height = height;
            _position = position;
        }

        public bool Equals(JumpPadProperties other)
        {
            return this._height == other._height && this._position == other._position;
        }
    }
}
