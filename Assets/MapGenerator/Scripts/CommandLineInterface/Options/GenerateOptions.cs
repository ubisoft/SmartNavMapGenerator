using System.Collections.Generic;
using UnityEngine;
using LaForge.MapGenerator;
using LaForge.MapGenerator.SimpleJSON;

public class GenerateArgs
{
    public string output = "./";
    public int nbMaps = 1;
    public string mapName = "";
}

public class GenerateOptions : OptionsInterface
{
    protected GenerateArgs generalArgs;

    public GenerateOptions(ref GenerateArgs args)
    {
        generalArgs = args;
    }

    public override bool Parse(ref List<string> args)
    {
        bool parsedWithoutError = true;

        options.Add("General");
        options.Add("load=", "Load generation parameters from a map json config file or map folder.", (string path) =>
        {
            GameObject.FindObjectOfType<MapGenerator>().LoadConfiguration(path);
        });
        options.Add("nb-maps=", "How many maps to generate.", 
            (int nbmaps) => generalArgs.nbMaps = nbmaps);
        options.Add("output=", "Folder where to output maps.\n[Default: ./]", 
            (string path) => generalArgs.output = path);
        options.Add("name=", "Set a map name instead of generating a random one.\n[Default: '']",
            (string mapName) => generalArgs.mapName = mapName);

        args = options.Parse(args);

        return parsedWithoutError;
    }

    public override bool ValidateArgs(List<string> args)
    {
        return true;
    }

    public override string Name()
    {
        return "General";
    }
}
