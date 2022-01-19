using Mono.Options;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;

public class BuildingGeneratorOptions : GenericOptions<BuildingGenerator>
{
    public BuildingGeneratorOptions(BuildingGenerator generator) : base(generator)
    {

    }

    public override string Name()
    {
        return "Building Generator";
    }

    public override bool Parse(ref List<string> argsList)
    {
        bool returnValue = true;

        options.Add("Building Generation");
        options.Add("buildings:", $"Whether or not to generate buildings throughout the map. For each building, a flat platform is also generated under it.\n[Default: {obj.GenerateBuildings}]",
            (string strPlatforms) => returnValue &= ParseFlag("buildings", strPlatforms, out obj.GenerateBuildings));

        options.Add("min-buildings=", $"Minimum number of buildings in the map. If the platforms are non-overlapping, the genrator could have to settle for less. A warning will be logged when it happens.\n[Default: {obj.PlatformGenerationStep.MinNbPlatforms}]",
            (int min) => obj.PlatformGenerationStep.MinNbPlatforms = min);

        options.Add("max-buildings=", $"Max number of buildings in the map.\n[Default: {obj.PlatformGenerationStep.MaxNbPlatforms}]",
            (int max) => obj.PlatformGenerationStep.MaxNbPlatforms = max);

        options.Add("platform-inner-radius=", $"Size of the inner radius of a platform.\n[Default: {obj.PlatformGenerationStep.InnerRadius}]",
            (float radius) => obj.PlatformGenerationStep.InnerRadius = radius);

        options.Add("platform-outer-radius=", $"Size of the outer radius of a platform.\n[Default: {obj.PlatformGenerationStep.OuterRadius}]",
            (float radius) => obj.PlatformGenerationStep.OuterRadius = radius);

        options.Add("platform-min-height=", $"Minimum height of a platform.\n[Default: {obj.PlatformGenerationStep.MinHeight}]",
            (float height) => obj.PlatformGenerationStep.MinHeight = height);

        options.Add("platform-max-height=", $"Maximum height of a platform.\n[Default: {obj.PlatformGenerationStep.MaxHeight}]",
            (float height) => obj.PlatformGenerationStep.MaxHeight = height);

        options.Add("platform-warping=", $"How flat the platforms should be from 0 to 1. 1 being completely flat.\n[Default: {obj.PlatformGenerationStep.PlatformsWarping}]",
            (float warping) => obj.PlatformGenerationStep.PlatformsWarping = warping);

        options.Add("non-overlapping-platforms=", $"Can the platforms overlap or not. This also applies to the buidlings. The inner radius is used to compute the overlap. If non-overlapping, it's possible that the minimum number of buildings is not respected. A warning will be logged when it happens.\n[Default: {obj.PlatformGenerationStep.NonOverlapping}]",
    (bool nonOverlappingPlatforms) => obj.PlatformGenerationStep.NonOverlapping = nonOverlappingPlatforms);

        options.Add("platform-tries=", $"Number of tries to fit a platform in the map before settling for less.\n[Default: {obj.PlatformGenerationStep.Tries}]",
            (int platformTries) => obj.PlatformGenerationStep.Tries = platformTries);

        options.Add("window=", $"Probability to have a window.\n[Default: {obj.windowPercentChance}]", (float windowPercentChance) => obj.windowPercentChance = windowPercentChance);
        options.Add("door=", $"Probability to have a door.\n[Default: {obj.doorPercentChance}]", (float doorPercentChance) => obj.doorPercentChance = doorPercentChance);
        options.Add("outside-stair=", $"Probability to have an outside stair.\n[Default: {obj.outsideStairPercentChance}]", (float outsideStairPercentChance) => obj.outsideStairPercentChance = outsideStairPercentChance);
        options.Add("inside-stair=", $"Probability to have an inside stair.\n[Default: {obj.insideStairPercentChance}]", (float insideStairPercentChance) => obj.insideStairPercentChance = insideStairPercentChance);
        options.Add("rows=", $"Number of rows.\n[Default: {obj.rows}]", (int rows) => obj.rows = rows);
        options.Add("columns=", $"Number of columns.\n[Default: {obj.columns}]", (int columns) => obj.columns = columns);
        options.Add("cell-unit-size=", $"Size of one cell.\n[Default: {obj.cellUnitSize}]", (float cellUnitSize) => obj.cellUnitSize = cellUnitSize);
        options.Add("nb-floors=", $"Number of possible floors to generate.\n[Default: {obj.nbOfFloors}]", (int nbOfFloors) => obj.nbOfFloors = nbOfFloors);
        options.Add("randomize-bounds:", $"Randomize bounds.\n[Default: {obj.randomizeBounds}]", (string randomizeBoundsStr) => returnValue &= ParseFlag("randomize-bounds", randomizeBoundsStr, out obj.randomizeBounds));
        options.Add("randomize-bounds-chance=", $"Probability to roll a new bound size on new floors.\n[Default: {obj.newRowColChance}]", (float newRolColChance) => obj.newRowColChance = newRolColChance);
        argsList = options.Parse(argsList);

        return returnValue;
    }

    public override bool ValidateArgs(List<string> args)
    {
        return true;
    }
}
