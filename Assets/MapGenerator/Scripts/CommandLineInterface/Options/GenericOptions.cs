using UnityEngine;
using Mono.Options;
using System.Collections.Generic;
using System.Linq;

public abstract class GenericOptions<T> : OptionsInterface where T : MonoBehaviour 
{
    protected T obj;

    public GenericOptions(T monoBehavior)
    {
        obj = monoBehavior;
    }
}
