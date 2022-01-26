# Map Generator

Unity project to procedurally generate 3D terrains with buildings.

## Requirements

- git-lfs
- Unity >= 2020.3.19f1

## Workflow
1. Open the repo in the unity editor
2. Hit play
3. Tweak the generation parameters in the `MapGenerator` gameobject
4. Hit generate
5. Repeat 3-4 until you are satisfied with the style of the map
6. Hit save map and save it wherever you want
   1. Check `Save with goals` and set the number of it, if you want a set of spawn-goals immediately
7. Generate any numbers of different maps in the same style as the saved one using this command:
```sh
./MapGenerator -batchmode -nographics -logFile - -- generate --load path/to/saved/map --nb-maps <n>
```

## How to use (Command line)

### Windows (Powershell)
```powershell
.\MapGenerator.exe -batchmode -logFile - -- <command> [options] | Write-Host
```

### Linux
```sh
./MapGenerator -batchmode -nographics -logFile - -- <command> [options]
```

## To load the output of the generator in a Unity Project see [this](./Assets/MapGenerator/MapGenerator.Shared/README.md) 

## Generator Output

The generator outputs a folder named after the map. This folder contains the following:
### terrain.bin
This contains the terrain mesh.

### map.json
This contains the gameobject hierarchy and their characteristics (position, scale, rotation).

### config.json
This contains all the settings used for generation.

### spawn_goals.json
This file is generated only if spawn-goals are also generated. It contains the list of spawn-goals and a reference to the above config json.

### grounds/ground#.bin
Multiple files of this type will be generated if special kind of grounds are part of the map generation. One file for one ground.

## Building the project

- Open the Unity project
- Hit ctrl + shift + B
- Select Windows, Linux or macOS
- Make sure everything but Development Build is unchecked
- Hit Build

## Command-Line Interface
### General
```
Usage: <MapGenerator.exe|MapGenerator> COMMAND [OPTIONS]

Use `<MapGenerator.exe|MapGenerator> help COMMAND` for help on a specific command.

Available commands:

        change-goals         Command to generate new goals for an existing map.

        generate             Command to generate a map and goals.

        help                 Show this message and exit
```

### change-goals
```
Usage:
    <MapGenerator.exe|MapGenerator> change-goals --load <path/to/map/json> --spawn-goals <x> [OPTIONS]

Description:
    Command to generate new goals for an existing map.

Options:
General
      --load=VALUE           Path to map folder.
Spawn-Goal Generation
      --spawn-goals=VALUE    How many spawns-goals to generate for the map.
                               [Default: 0]
Uniform distance spawn-goal generation
      --uni-goal-dist[=VALUE]
                             Whether to use a more uniform distance
                               distribution for goal generation.
                               [Default: False]
      --nb-buckets=VALUE     Number of buckets to use.
                               [Default: 20]
      --max-tries=VALUE      Number of tries to generate a spawn-goal with
                               uniform distance.
                               [Default: 100]
```

### generate
```
Usage:
    <MapGenerator.exe|MapGenerator.x86_64> generate [OPTIONS]

Description:
    Command to generate a map which consist of a 3D terrain and possibly buildings. It's also possible to generate goals at the same time.

Options:
General
      --load=VALUE           Load generation parameters from a map json config
                               file or map folder.
      --nb-maps=VALUE        How many maps to generate.
      --output=VALUE         Folder where to output maps.
                               [Default: ./]
      --name=VALUE           Set a map name instead of generating a random one.
                               [Default: '']
Terrain Generation
      --volume=VALUE         Size of the marching cube grid.
                               [Default: 20,10,20]
                               Format: float,float,float
      --points-per-axis=VALUE
                             Number of grid points distributed over each axis.
                               Cannot be combined with --spacing.
                               Format: int|int,int,int
      --spacing=VALUE        Distance between each grid points. Cannot be
                               combined with --points-per-axis.
                               [Default: 0.15]
                               Format: float|float,float,float
      --scale=VALUE          Scale of the map.
                               [Default: 6]
      --seed=VALUE           Seed to initialize the randomization.
                               [Default: 0]
      --surfacelevel=VALUE   Threshold at which the marching cube algorithm
                               consider grid point values over or under the
                               isosurface. Default value of 0 means that
                               negatives noise values are considered empty
                               space and positive values inside the mesh.
                               [Default: 0]
      --prewarp-strength=VALUE
                             How much warping to apply to grid point values.
                               This will add some extra variation to the noise.
                               Can be used to get slight variation of the same
                               map.
                               [Default: 0]
      --height=VALUE         Vertical center of the map.
                               [Default: 2]
      --hard-floor=VALUE     Vertical position of the hard floor.
                               [Default: 1.5]
      --hard-floor-weight=VALUE
                             How flat the hard floor is. The lower the value,
                               the more noisy the floor will be.
                               [Default: 40]
  -o[=VALUE1,VALUE2]         Couple of frequency and amplitude to use for
                               sampling from the 3D Perlin function.
                               [Default: 1.01, 1; 0.5, 2; 0.2, 6]
                               Format: -ofloat,float -ofloat,float...
Overhangs
      --overhangs[=VALUE]    Whether or not to generate overhangs throughout
                               the map.
                               [Default: True]
      --overhang-height=VALUE
                             Maximum height of the overhangs relative to the
                               map height.
                               [Default: 0.5]
      --overhang-strength=VALUE
                             Roughly correlate with the size of the overhangs.
                               [Default: 20]
Terraces
      --terraces[=VALUE]     Whether or not to generate terraces throughout the
                               map. WARNING: this feature does not seem to
                               correctly for now.
                               [Default: False]
      --terraceHeight=VALUE  Maximum height of the terraces relative to the map
                               height.
                               [Default: 0.05]
      --terraceStrength=VALUE
                             Roughly correlate with the size of terraces.
                               [Default: 20]
Building Generation
      --buildings[=VALUE]    Whether or not to generate buildings throughout
                               the map. For each building, a flat platform is
                               also generated under it.
                               [Default: False]
      --min-buildings=VALUE  Minimum number of buildings in the map. If the
                               platforms are non-overlapping, the genrator
                               could have to settle for less. A warning will be
                               logged when it happens.
                               [Default: 0]
      --max-buildings=VALUE  Max number of buildings in the map.
                               [Default: 1]
      --platform-inner-radius=VALUE
                             Size of the inner radius of a platform.
                               [Default: 3]
      --platform-outer-radius=VALUE
                             Size of the outer radius of a platform.
                               [Default: 4]
      --platform-min-height=VALUE
                             Minimum height of a platform.
                               [Default: 0]
      --platform-max-height=VALUE
                             Maximum height of a platform.
                               [Default: 4]
      --platform-warping=VALUE
                             How flat the platforms should be from 0 to 1. 1
                               being completely flat.
                               [Default: 1]
      --non-overlapping-platforms=VALUE
                             Can the platforms overlap or not. This also
                               applies to the buidlings. The inner radius is
                               used to compute the overlap. If non-overlapping,
                               it's possible that the minimum number of
                               buildings is not respected. A warning will be
                               logged when it happens.
                               [Default: True]
      --platform-tries=VALUE Number of tries to fit a platform in the map
                               before settling for less.
                               [Default: 10000]
      --window=VALUE         Probability to have a window.
                               [Default: 0.3]
      --door=VALUE           Probability to have a door.
                               [Default: 0.2]
      --outside-stair=VALUE  Probability to have an outside stair.
                               [Default: 0.2]
      --inside-stair=VALUE   Probability to have an inside stair.
                               [Default: 0.2]
      --rows=VALUE           Number of rows.
                               [Default: 3]
      --columns=VALUE        Number of columns.
                               [Default: 3]
      --cell-unit-size=VALUE Size of one cell.
                               [Default: 1]
      --nb-floors=VALUE      Number of possible floors to generate.
                               [Default: 1]
      --randomize-bounds[=VALUE]
                             Randomize bounds.
                               [Default: True]
      --randomize-bounds-chance=VALUE
                             Probability to roll a new bound size on new floors.

                               [Default: 0.2]
Jump Pad Generation
      --jumppads[=VALUE]     Whether or not to generate jump pads throughout
                               the map.
                               [Default: True]
      --min-jumppads=VALUE   Minimum number of jump pads to generate for the
                               map.
                               [Default: 10]
      --max-jumppads=VALUE   Maximum number of jump pads to generate for the
                               map.
                               [Default: 10]
      --steepness=VALUE      Scan the height map for positive height variations
                               higher than this threshold and store them as
                               potential jump pad spawn points. The value is in
                               meters and is applied after scaling.
                               [Default: 5]
      --min-flatness=VALUE   This is to prevent spawning jump pads on slopes
                               that would be too steep to walk to but still has
                               one side steep enough for a jump pad. The value
                               is in meters and is applied after scaling.
                               [Default: -0.5]
      --search-radius=VALUE  Radius to consider around each point of the height
                               map when scanning. The value is in meters and is
                               applied after scaling.
                               [Default: 5]
      --cluster-radius=VALUE Radius to consider when clustering all the
                               possible spawn points. There can only be one
                               jump pad per cluster. The value is in meters and
                               is applied after scaling.
                               [Default: 5]
Ground Generation
      --grounds[=VALUE]      Generates random types of ground (lava or water).
                               To get interesting results the hard floor weight
                               should be set to a 4 or less.
                               [Default: False]
      --lava=VALUE           Probability for a ground to be lava. The water
                               probability will be adjusted accordingly.
                               [Default: 0.5]
      --water=VALUE          Probability for a ground to be water. The lava
                               probability will be adjusted accordingly.
                               [Default: 0.5]
Spawn-Goal Generation
      --spawn-goals=VALUE    How many spawns-goals to generate for the map.
                               [Default: 0]
Uniform distance spawn-goal generation
      --uni-goal-dist[=VALUE]
                             Whether to use a more uniform distance
                               distribution for goal generation.
                               [Default: False]
      --nb-buckets=VALUE     Number of buckets to use.
                               [Default: 20]
      --max-tries=VALUE      Number of tries to generate a spawn-goal with
                               uniform distance.
                               [Default: 100]
```
