using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LaForge.MapGenerator;
using LaForge.MapGenerator.SimpleJSON;

public class ChangeGoalsCommand : CommandInterface
{
    private ChangeGoalsArgs changeGoalsArgs = new ChangeGoalsArgs();
    private string mapName;
    private SpawnGoalGenerator spawnGoalGenerator;
    public ChangeGoalsCommand() : base("change-goals", "Command to generate new goals for an existing map.")
    {
        ChangeGoalsOptions generalCLI = new ChangeGoalsOptions(ref changeGoalsArgs);
        optionsDict.Add(generalCLI.GetType().Name, generalCLI);

        SpawnGoalGenerator spawnGoalGenerator = GameObject.FindObjectOfType<SpawnGoalGenerator>();
        SpawnGoalGeneratorOptions spawnGoalGeneratorCLI = new SpawnGoalGeneratorOptions(spawnGoalGenerator);
        optionsDict.Add(spawnGoalGeneratorCLI.GetType().Name, spawnGoalGeneratorCLI);
    }

    protected override void Setup()
    {
        spawnGoalGenerator = GameObject.FindObjectOfType<SpawnGoalGenerator>();
    }

    public void LoadMap()
    {
        GameObject mapGo = MapSerializer.LoadMap(changeGoalsArgs.mapPath);
        if (mapGo == null)
            throw new System.Exception("Error while loading a map.");
        mapName = mapGo.name;
    }

    public void GoalGeneration()
    {
        if (spawnGoalGenerator.TotalGoals > 0)
        {
            spawnGoalGenerator.Clear();
            spawnGoalGenerator.GenerateNavMesh();
            List<Vector3[]> spawnGoals = spawnGoalGenerator.GeneralGoals();

            string configName = MapSerializer.CONFIG_FILE;

            JSONNode mapConfig = JSON.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(changeGoalsArgs.mapPath, configName)));
            mapConfig.Remove(spawnGoalGenerator.GetType().Name);
            mapConfig.Add(spawnGoalGenerator.GetType().Name, JSON.Parse(JsonUtility.ToJson(spawnGoalGenerator)));
            System.IO.File.WriteAllText(System.IO.Path.Combine(changeGoalsArgs.mapPath, configName), mapConfig.ToString(4));

            JSONObject json = MapSerializer.SerializeSpawnGoals(spawnGoals);
            System.IO.File.WriteAllText(System.IO.Path.Combine(changeGoalsArgs.mapPath, MapSerializer.SPAWN_GOALS_FILE), json.ToString(4));
        }

    }

    protected override bool ParseConcrete(ref List<string> args)
    {
        bool parsedWithoutError = true;
        // skip unity parameters
        foreach (OptionsInterface optionsInterface in optionsDict.Values)
        {
            parsedWithoutError &= optionsInterface.ValidateAndParse(ref args);
        }

        return parsedWithoutError;
    }

    public override IEnumerator Execute()
    {
        // Coroutine designed to wait 10 frames (time to rebuild the colliders) before we generate the goals. 
        // We have to do this otherwise the raycasts done don't hit anything.
        try
        {
            LoadMap();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
            yield break;
        }

        for (int goalIx = 0; goalIx < 10; goalIx++)
        {
            yield return null;
        }

        try
        {
            GoalGeneration();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
            yield break;
        }

        Debug.Log("Saved output to:" + changeGoalsArgs.mapPath);
    }

    protected override string Usage()
    {
        return "<MapGenerator.exe|MapGenerator> change-goals --load <path/to/map/json> --spawn-goals <x> [OPTIONS]";
    }

    public override string Description()
    {
        return "Command to generate new goals for an existing map.";
    }
}
