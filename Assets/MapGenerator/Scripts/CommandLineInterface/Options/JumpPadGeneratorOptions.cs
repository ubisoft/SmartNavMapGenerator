using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using UnityEngine;

public class JumpPadGeneratorOptions : GenericOptions<JumppadGenerator>
{
    public JumpPadGeneratorOptions(JumppadGenerator generator) : base(generator)
    {
    }

    public override bool Parse(ref List<string> args)
    {
        bool parsedWithoutError = true;

        options.Add("Jump Pad Generation");
        options.Add("jumppads:", $"Whether or not to generate jump pads throughout the map.\n[Default: {obj.GenerateJumpPads}]",
            (string strJumpPads) => parsedWithoutError &= ParseFlag("jumppads", strJumpPads, out obj.GenerateJumpPads));

        options.Add("min-jumppads=", $"Minimum number of jump pads to generate for the map.\n[Default: {obj.MinNbJumpPads}]", 
            (int minJumpPads) => obj.MinNbJumpPads = minJumpPads);

        options.Add("max-jumppads=", $"Maximum number of jump pads to generate for the map.\n[Default: {obj.MaxNbJumpPads}]",
            (int MaxNbJumpPads) => obj.MaxNbJumpPads = MaxNbJumpPads);

        options.Add("steepness=", $"Scan the height map for positive height variations higher than this threshold and store them as potential jump pad spawn points. The value is in meters and is applied after scaling.\n[Default: {obj.SteepnessThreshold}]", 
            (float steepness) => obj.SteepnessThreshold = steepness);

        options.Add("min-flatness=", $"This is to prevent spawning jump pads on slopes that would be too steep to walk to but still has one side steep enough for a jump pad. The value is in meters and is applied after scaling.\n[Default: {obj.MinFlatness}]", 
            (float MinFlatness) => obj.MinFlatness = MinFlatness);

        options.Add("search-radius=", $"Radius to consider around each point of the height map when scanning. The value is in meters and is applied after scaling.\n[Default: {obj.SearchRadius}]", 
            (float SearchRadius) => obj.SearchRadius = SearchRadius);

        options.Add("cluster-radius=", $"Radius to consider when clustering all the possible spawn points. There can only be one jump pad per cluster. The value is in meters and is applied after scaling.\n[Default: {obj.ClusterRadius}]", 
            (float clusterRadius) => obj.ClusterRadius = clusterRadius);

        args = options.Parse(args);

        return parsedWithoutError;
    }

    public override bool ValidateArgs(List<string> args)
    {
        return true;
    }

    public override string Name()
    {
        return "Jump Pads";
    }
}
