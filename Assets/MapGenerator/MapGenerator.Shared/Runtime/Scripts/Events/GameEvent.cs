using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace LaForge.MapGenerator
{
    public class GameEvent<T0, T1> : ScriptableObject
    {
        private List<System.Action<T0, T1>> actions = new List<System.Action<T0, T1>>();

        public void Raise(T0 a, T1 b)
        {
            foreach (System.Action<T0, T1> listener in actions)
            {
                listener.Invoke(a, b);
            }
        }

        public void AddAction(System.Action<T0, T1> listener)
        {
            actions.Add(listener);
        }

        public void RemoveAction(System.Action<T0, T1> listener)
        {
            actions.Remove(listener);
        }

        public void RemoveActions()
        {
            actions.Clear();
        }
    }
}
