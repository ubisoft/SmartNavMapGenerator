using System.Collections;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;

namespace LaForge.MapGenerator
{
    public abstract class CollisionEventListener<T> : GameEventListener<T, Collision>
    {
    }
}
