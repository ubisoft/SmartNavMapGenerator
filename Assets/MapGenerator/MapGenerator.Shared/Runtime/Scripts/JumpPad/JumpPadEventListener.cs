using System.Collections;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;

namespace LaForge.MapGenerator
{
    public abstract class JumpPadEventListener : CollisionEventListener<JumpPad>
    {
        protected override void RegisterEvents()
        {
            JumpPadEvent jumpPadEvent = Resources.Load<JumpPadEvent>("Events/JumpPadEvent");
            jumpPadEvent.AddAction(OnJumpPadEnter);
        }

        protected abstract void OnJumpPadEnter(JumpPad jumpPad, Collision collision);
    }
}
