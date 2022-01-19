using System.Collections.Generic;
using UnityEngine;
using LaForge.MapGenerator;
using LaForge.MapGenerator.SimpleJSON;

public class ChangeGoalsArgs
{
    public string mapPath;
}

public class ChangeGoalsOptions : OptionsInterface
{
    private ChangeGoalsArgs changeGoalsArgs;
    
    public ChangeGoalsOptions(ref ChangeGoalsArgs args)
    {
        changeGoalsArgs = args;
    }

    public override bool Parse(ref List<string> args)
    {
        bool parsedWithoutError = true;

        options.Add("General");
        options.Add("load=", "Path to map folder.", (string folderPath) =>
        {
            changeGoalsArgs.mapPath = folderPath;
            SpawnGoalGenerator spawnGoalGenerator = GameObject.FindObjectOfType<SpawnGoalGenerator>();

            string path = System.IO.Path.Combine(folderPath, MapSerializer.CONFIG_FILE);
            var json = JSON.Parse(System.IO.File.ReadAllText(path)); 
            if (json[spawnGoalGenerator.GetType().Name] != null)
                JsonUtility.FromJsonOverwrite(json[spawnGoalGenerator.GetType().Name].ToString(), spawnGoalGenerator);
        });

        args = options.Parse(args);

        return parsedWithoutError;
    }

    public override bool ValidateArgs(List<string> args)
    {
        if (args.Count == 1 && (args[0] == "-h" || args[0] == "--help"))
            return true;

        if (!args.Contains("--load"))
        {
            Debug.LogError("change-goals command requires a map to be loaded through the --load parameter");
            return false;
        }

        return true;
    }

    public override string Name()
    {
        return "General";
    }
}
