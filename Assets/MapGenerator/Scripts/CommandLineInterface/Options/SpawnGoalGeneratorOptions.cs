using System.Collections.Generic;
using LaForge.MapGenerator;

public class SpawnGoalGeneratorOptions : GenericOptions<SpawnGoalGenerator>
{
    public SpawnGoalGeneratorOptions(SpawnGoalGenerator generator) : base(generator)
    {
    }

    public override bool Parse(ref List<string> args)
    {
        bool parsedWithoutError = true;

        options.Add("Spawn-Goal Generation");
        options.Add("spawn-goals=", $"How many spawns-goals to generate for the map.\n[Default: {obj.TotalGoals}]", (int goals) =>
        {
            obj.TotalGoals = goals;
        });

        options.Add("Uniform distance spawn-goal generation");
        options.Add("uni-goal-dist:", $"Whether to use a more uniform distance distribution for goal generation.\n[Default: {obj.GenerateGoalUniformly}]", (string uniformDDStr) =>
        {
            parsedWithoutError &= ParseFlag("uni-goal-dist", uniformDDStr, out obj.GenerateGoalUniformly);
        });

        options.Add("nb-buckets=", $"Number of buckets to use.\n[Default: {obj.NbBuckets}]", (int nbBuckets) => obj.NbBuckets = nbBuckets);

        options.Add("max-tries=", $"Number of tries to generate a spawn-goal with uniform distance.\n[Default: {obj.MaxTries}]", (int maxTries) => obj.MaxTries = maxTries);

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
