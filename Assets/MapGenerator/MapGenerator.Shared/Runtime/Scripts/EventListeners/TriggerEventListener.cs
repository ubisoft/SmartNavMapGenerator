using System.Collections;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;

namespace LaForge.MapGenerator
{
    public abstract class TriggerEventListener<T> : GameEventListener<T, Collider>
    {
    }
}
