using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LaForge.MapGenerator;
using LaForge.MapGenerator.SimpleJSON;

public class GenerateCommand : CommandInterface
{
    public GenerateArgs generalArgs = new GenerateArgs();
    private string mapSavePath = "";

    public GenerateCommand() : base("generate", "Command to generate a map and goals.")
    {
        GenerateOptions generalCLI = new GenerateOptions(ref generalArgs);
        optionsDict.Add(generalCLI.GetType().Name, generalCLI);

        TerrainGenerator terrainGenerator = GameObject.FindObjectOfType<TerrainGenerator>();
        TerrainGeneratorOptions terrainGeneratorCLI = new TerrainGeneratorOptions(terrainGenerator);
        optionsDict.Add(terrainGeneratorCLI.GetType().Name, terrainGeneratorCLI);

        BuildingGenerator buildingGenerator = GameObject.FindObjectOfType<BuildingGenerator>();
        BuildingGeneratorOptions buildingGeneratorCLI = new BuildingGeneratorOptions(buildingGenerator);
        optionsDict.Add(buildingGeneratorCLI.GetType().Name, buildingGeneratorCLI);

        JumppadGenerator jumppadGenerator = GameObject.FindObjectOfType<JumppadGenerator>();
        JumpPadGeneratorOptions jumpPadGeneratorCLI = new JumpPadGeneratorOptions(jumppadGenerator);
        optionsDict.Add(jumpPadGeneratorCLI.GetType().Name, jumpPadGeneratorCLI);

        GroundGenerator groundGenerator = GameObject.FindObjectOfType<GroundGenerator>();
        GroundGeneratorOptions groundGeneratorCLI = new GroundGeneratorOptions(groundGenerator);
        optionsDict.Add(groundGeneratorCLI.GetType().Name, groundGeneratorCLI);

        SpawnGoalGenerator spawnGoalGenerator = GameObject.FindObjectOfType<SpawnGoalGenerator>();
        SpawnGoalGeneratorOptions spawnGoalGeneratorCLI = new SpawnGoalGeneratorOptions(spawnGoalGenerator);
        optionsDict.Add(spawnGoalGeneratorCLI.GetType().Name, spawnGoalGeneratorCLI);
    }

    protected override void Setup()
    {
    }

    public void GenerateMap()
    {
        MapGenerator mapRandomizer = GameObject.FindObjectOfType<MapGenerator>();
        mapRandomizer.Generate(generalArgs.mapName);

        mapSavePath = System.IO.Path.Combine(generalArgs.output, mapRandomizer.GetMapName());
    }

    public void SaveMap()
    {
        MapGenerator mapRandomizer = GameObject.FindObjectOfType<MapGenerator>();
        mapRandomizer.SaveMap(mapSavePath, true);
        mapRandomizer.TerrainGenerator.seed += 1;
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
        for (int i = 0; i < generalArgs.nbMaps; i++)
        {
            try
            {
                GenerateMap();
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
                SaveMap();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
                yield break;
            }

            Debug.Log("Saved output to:" + mapSavePath);
        }
    }

    protected override string Usage()
    {
        return "<MapGenerator.exe|MapGenerator> generate [OPTIONS]";
    }

    public override string Description()
    {
        return "Command to generate a map which consist of a 3D terrain and possibly buildings. It's also possible to generate goals at the same time.";
    }
}
