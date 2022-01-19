using Mono.Options;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;

public class TerrainGeneratorOptions : GenericOptions<TerrainGenerator>
{
    public TerrainGeneratorOptions(TerrainGenerator generator) : base(generator)
    {
    }

    public override bool Parse(ref List<string> argsList)
    {
        bool returnValue = true;
        options.Add("Terrain Generation");
        options.Add("volume=", $"Size of the marching cube grid.\n[Default: {obj.Volume.x},{obj.Volume.y},{obj.Volume.z}]\nFormat: float,float,float", 
            (string strVolume) =>
        {
            Vector3 volume;
            Regex rgx = new Regex(@"^([0-9]*\.?[0-9]+,){2}[0-9]*\.?[0-9]+$");
            if (rgx.IsMatch(strVolume))
            {
                string[] volumeComponents = strVolume.Split(',');
                volume = new Vector3(float.Parse(volumeComponents[0]), float.Parse(volumeComponents[1]), float.Parse(volumeComponents[2]));
            }
            else
            {
                Debug.LogError("volume format does not match float,float,float.");
                returnValue = false;
                return;
            }

            obj.Volume = volume;
        });

        argsList = options.Parse(argsList);
        
        options.Add("points-per-axis=", "Number of grid points distributed over each axis. Cannot be combined with --spacing.\nFormat: int|int,int,int", 
            (string strPPA) =>
        {
            
            Vector3 invertedPPAs;
            int singlePPA;
            if (int.TryParse(strPPA, out singlePPA))
            {
                float invertedPPA = 1f / singlePPA;
                invertedPPAs = new Vector3(invertedPPA, invertedPPA, invertedPPA);
            }
            else
            {
                Regex rgx = new Regex(@"^([0-9],){2}[0-9]$");
                if (rgx.IsMatch(strPPA))
                {
                    string[] ppaComponents = strPPA.Split(',');
                    invertedPPAs = new Vector3(1f / float.Parse(ppaComponents[0]), 1f / float.Parse(ppaComponents[1]), 1f / float.Parse(ppaComponents[2]));
                }
                else
                {
                    Debug.LogError("points-per-axis format does not match int|int,int,int.");
                    returnValue = false;
                    return;
                }
            }

            obj.Spacing = Vector3.Scale(obj.Volume, invertedPPAs);

        });

        options.Add("spacing=", $"Distance between each grid points. Cannot be combined with --points-per-axis.\n[Default: {obj.Spacing.x:F2}]\nFormat: float|float,float,float", 
            (string strSpacing) =>
        {
            Vector3 spacing;
            float singleSpacing;
            if (float.TryParse(strSpacing, out singleSpacing))
            {
                spacing = new Vector3(singleSpacing, singleSpacing, singleSpacing);
            }
            else
            {
                Regex rgx = new Regex(@"^([0-9]*\.?[0-9]+,){2}[0-9]*\.?[0-9]+$");
                if (rgx.IsMatch(strSpacing))
                {
                    string[]spacingComponents = strSpacing.Split(',');
                    spacing = new Vector3(float.Parse(spacingComponents[0]), float.Parse(spacingComponents[1]), float.Parse(spacingComponents[2]));
                }
                else
                {
                    Debug.LogError("points-per-axis format does not match int|int,int,int.");
                    returnValue = false;
                    return;
                }
            }

            obj.Spacing = spacing;

        });

        options.Add("scale=", $"Scale of the map.\n[Default: {obj.Scale}]", 
            (float scale) => obj.Scale = scale);

        options.Add("seed=", $"Seed to initialize the randomization.\n[Default: {obj.seed}]", 
            (int seed) => obj.seed = seed);

        options.Add("surfacelevel=", $"Threshold at which the marching cube algorithm consider grid point values over or under the isosurface. Default value of 0 means that negatives noise values are considered empty space and positive values inside the mesh.\n[Default: {obj.surfacelevel}]",
            (float surfacelevel) => obj.surfacelevel = surfacelevel);

        options.Add("prewarp-strength=", $"How much warping to apply to grid point values. This will add some extra variation to the noise. Can be used to get slight variation of the same map.\n[Default: {obj.PrewarpStrength}]",
            (float prewarpStrength) => obj.PrewarpStrength = prewarpStrength);

        options.Add("height=", $"Vertical center of the map.\n[Default: {obj.Height}]",
            (float height) => obj.Height = height);

        options.Add("hard-floor=", $"Vertical position of the hard floor.\n[Default: {obj.HardFloor}]", 
            (float hardfloor) => obj.HardFloor = hardfloor);

        options.Add("hard-floor-weight=", $"How flat the hard floor is. The lower the value, the more noisy the floor will be.\n[Default: {obj.HardFloorWeight}]", 
            (float hardFloorWeight) => obj.HardFloorWeight = hardFloorWeight);

        bool resetOctaves = true;
        options.Add("o:,", "Couple of frequency and amplitude to use for sampling from the 3D Perlin function.\n[Default: 1.01, 1; 0.5, 2; 0.2, 6]\nFormat: -ofloat,float -ofloat,float...", 
            (float frequency, float amplitude) => 
        {
            if (resetOctaves)
            {
                obj.Octaves.Clear();
                resetOctaves = false;
            }
            obj.Octaves.Add(new Octave(frequency, amplitude));
        });

        options.Add("Overhangs");
        options.Add("overhangs:", $"Whether or not to generate overhangs throughout the map.\n[Default: {obj.GenerateOverhangs}]",
            (string strOverhangs) => returnValue &= ParseFlag("overhangs", strOverhangs, out obj.GenerateOverhangs));

        options.Add("overhang-height=", $"Maximum height of the overhangs relative to the map height.\n[Default: {obj.OverhangHeight}]", 
            (float overhangHeight) => obj.OverhangHeight = overhangHeight);

        options.Add("overhang-strength=", $"Roughly correlate with the size of the overhangs.\n[Default: {obj.OverhangStrength}]",
            (float overhangStrength) => obj.OverhangStrength = overhangStrength);

        options.Add("Terraces");
        options.Add("terraces:", $"Whether or not to generate terraces throughout the map. WARNING: this feature does not seem to correctly for now.\n[Default: {obj.GenerateTerraces}]",
            (string strTerraces) => returnValue &= ParseFlag("terraces", strTerraces, out obj.GenerateTerraces));

        options.Add("terraceHeight=", $"Maximum height of the terraces relative to the map height.\n[Default: {obj.TerraceHeight}]",
            (float terraceHeight) => obj.TerraceHeight = terraceHeight);

        options.Add("terraceStrength=", $"Roughly correlate with the size of terraces.\n[Default: {obj.TerraceStrength}]",
            (float terraceStrength) => obj.TerraceStrength = terraceStrength);

        argsList = options.Parse(argsList);

        return returnValue;
    }

    public override bool ValidateArgs(List<string> args)
    {
        bool returnValue = true;
        int exclusiveArgs = 0;
        foreach (string arg in args)
        {
            exclusiveArgs += (arg == "--spacing" | arg == "--points-per-axis") ? 1 : 0;
        }

        if (exclusiveArgs > 1)
        {
            Debug.LogError("Either use --spacing or --points-per-axis, not both.");
            returnValue = false;
        }

        return returnValue;
    }

    public override string Name()
    {
        return "Terrain Generation";
    }
}
