using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using UnityEngine;

public class GroundGeneratorOptions : GenericOptions<GroundGenerator>
{
    public GroundGeneratorOptions(GroundGenerator groundGenerator) : base(groundGenerator)
    {
    }

    public override bool Parse(ref List<string> args)
    {
        bool parsedWithoutError = true;
        bool hasLavaProb = false;
        bool hasWaterProb = false;

        options.Add("Ground Generation");
        options.Add("grounds:", $"Generates random types of ground (lava or water). To get interesting results the hard floor weight should be set to a 4 or less.\n[Default: {obj.GenerateGrounds}]", 
            (string strGrounds) =>
            {
                parsedWithoutError &= ParseFlag("grounds", strGrounds, out obj.GenerateGrounds);
                float hardFloorWeight = GameObject.FindObjectOfType<TerrainGenerator>().HardFloorWeight;
                if (obj.GenerateGrounds && hardFloorWeight > 4)
                    Debug.LogWarning("Hard floor weight of more than 4 is generally too high to generates holes in the floor to fill.");
            });
        options.Add("lava=", $"Probability for a ground to be lava. The water probability will be adjusted accordingly.\n[Default: {obj.LavaProbability}]",
            (float lavaProbability) =>
            {
                hasLavaProb = true;
                obj.LavaProbability = lavaProbability;
            });
        options.Add("water=", $"Probability for a ground to be water. The lava probability will be adjusted accordingly.\n[Default: {obj.WaterProbability}]",
            (float waterProbability) =>
            {
                hasWaterProb = true;
                obj.WaterProbability = waterProbability;
            });

        args = options.Parse(args);

        if (obj.LavaProbability + obj.WaterProbability > 1)
        {
            Debug.LogError("Probability distribution over the types of ground has to sum to 1.");
            return false;
        }

        if (hasLavaProb && !hasWaterProb)
            obj.WaterProbability = 1 - obj.LavaProbability;

        if (!hasLavaProb && hasWaterProb)
            obj.LavaProbability = 1 - obj.WaterProbability;

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
