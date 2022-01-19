using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LaForge.MapGenerator;

public class Cluster
{
    private List<JumpPadProperties> _jumpPads;
    private Vector2 _center;
    private float _averageHeight;

    public List<JumpPadProperties> JumpPads
    {
        get
        {
            return _jumpPads;
        }
    }

    public Vector2 Center
    {
        get
        {
            return _center;
        }
    }

    public float AverageHeight
    {
        get
        {
            return _averageHeight;
        }
    }

    public Cluster()
    {
        _jumpPads = new List<JumpPadProperties>();
    }

    public void AddJumpPad(JumpPadProperties jumpPad)
    {
        _jumpPads.Add(jumpPad);
        if (_jumpPads.Count > 1)
        {
            _center = (_jumpPads.Count * _center + new Vector2(jumpPad.Position.x, jumpPad.Position.z)) / (_jumpPads.Count + 1);
            _averageHeight = (_jumpPads.Count * _averageHeight + jumpPad.Height) / (_jumpPads.Count + 1);
        }
        else
        {
            _center = new Vector2(jumpPad.Position.x, jumpPad.Position.z);
            _averageHeight = jumpPad.Height;
        }
    }
}
