using System.Collections;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;

namespace LaForge.MapGenerator
{
    public abstract class GroundEventListener : CollisionEventListener<Ground>
    {

        protected override void RegisterEvents()
        {
            GroundEvent enter = Resources.Load<GroundEvent>("Events/GroundEnterEvent");
            enter.AddAction(OnGroundEnter);

            GroundEvent stay = Resources.Load<GroundEvent>("Events/GroundStayEvent");
            stay.AddAction(OnGroundStay);

            GroundEvent exit = Resources.Load<GroundEvent>("Events/GroundExitEvent");
            exit.AddAction(OnGroundExit);
        }

        protected abstract void OnGroundEnter(Ground ground, Collision collision);
        protected abstract void OnGroundStay(Ground ground, Collision collision);
        protected abstract void OnGroundExit(Ground ground, Collision collision);
    }
}
