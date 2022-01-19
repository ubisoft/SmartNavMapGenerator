using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;

namespace LaForge.MapGenerator
{
    public abstract class GameEventListener<T0, T1> : MonoBehaviour
    {
        protected List<GameEvent<T0, T1>> Events = new List<GameEvent<T0, T1>>();

        protected abstract void RegisterEvents();

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            foreach (GameEvent<T0, T1> gameEvent in Events)
            {
                gameEvent.RemoveActions();
            }
        }
    }
}
