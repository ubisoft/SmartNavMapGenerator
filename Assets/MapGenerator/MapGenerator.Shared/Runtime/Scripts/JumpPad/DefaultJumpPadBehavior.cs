using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LaForge.MapGenerator;

public class DefaultJumpPadBehavior : JumpPadEventListener
{
    protected override void OnJumpPadEnter(JumpPad jumpPad, Collision collision)
    {
        Debug.Log($"{collision?.gameObject.name} entered the jump pad {jumpPad.name}");
    }
}
