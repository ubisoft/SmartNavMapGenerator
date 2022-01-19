using UnityEngine;
using UnityEngine.Events;

namespace LaForge.MapGenerator
{
    public class JumpPad : MonoBehaviour
    {
        public JumpPadProperties jumpPadProperties;
        private JumpPadEvent _jumpPadEvent;

        private void Awake()
        {
            _jumpPadEvent = Resources.Load<JumpPadEvent>("Events/JumpPadEvent");
        }

        private void OnCollisionEnter(Collision collision)
        {
            _jumpPadEvent?.Raise(this, collision);
        }
    }
}
