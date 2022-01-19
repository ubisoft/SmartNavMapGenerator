using UnityEngine;

namespace LaForge.MapGenerator
{
    public class DefaultGroundBehavior : GroundEventListener
    {
        protected override void OnGroundEnter(Ground ground, Collision collision)
        {
            Debug.Log($"{collision?.gameObject.name} entered the {ground.GroundProperties.Type} ground {ground.name}.");
        }

        protected override void OnGroundExit(Ground ground, Collision collision)
        {
            Debug.Log($"{collision?.gameObject.name} exited the {ground.GroundProperties.Type} ground {ground.name}.");
        }

        protected override void OnGroundStay(Ground ground, Collision collision)
        {
            Debug.Log($"{collision?.gameObject.name} stayed on the {ground.GroundProperties.Type} ground {ground.name}.");
        }
    }
}
