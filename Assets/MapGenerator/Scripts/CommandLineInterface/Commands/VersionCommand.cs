using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VersionCommand : CommandInterface
{
    public VersionCommand() : base("version", "Map Generator version")
    {
    }

    protected override void Setup()
    {
    }

    protected override bool ParseConcrete(ref List<string> args)
    {
        return true;
    }

    public override IEnumerator Execute()
    {
        Debug.Log($"LaForge Map Generator\nv{Application.version}");
        yield return null;
    }

    protected override string Usage()
    {
        return "<MapGenerator.exe|MapGenerator> version";
    }

    public override string Description()
    {
        return "Map Generator version";
    }
}
