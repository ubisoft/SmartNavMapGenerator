# MapGenerator.Shared package

## Install
In Unity Package Manager, add this git URL:
```
git://https://github.com/Ubisoft-LaForge/SmartNavMapGenerator.git?path=Assets/MapGenerator/MapGenerator.Shared#0.1.2
```

## Example
This [repo](https://github.com/Ubisoft-LaForge/SmartNavEnvironment) can be used as example of an integration of the package.

## Map Loading

```c#
LaForge.MapGenerator.MapSerializer.LoadSpawnGoalsAndMap("path/to/map/folder");
```

This package contains all the files needed to properly load a map.

### [MapSerializer.cs](./Scripts/MapSerializer.cs)
In most cases this is the only script you will need to interact with.

```c#
public static class MapSerializer
{
    /// <summary>
    /// Load spawn-goal pairs and the map.
    /// </summary>
    /// <param name="mapFolder">Path to the map folder.</param>
    /// <returns>SpawnGoalAndMap is a struct containing the list of Vector3 for the spawn-goal pairs and the gameobject in which the map has been loaded.</returns>
    public static SpawnGoalAndMap? LoadSpawnGoalsAndMap(string mapFolder);

    /// <summary>
    /// Load spawn-goal pairs into a list of Vector3 from the map folder. Normally located in /path/to/<map_name>/spawn_goals.json
    /// </summary>
    /// <param name="mapFolder">Path to the map folder.</param>
    /// <returns>List of pairs of Vector3. The first one corresponds to the spawn point and the second one is the goal.</returns>
    public static List<Vector3[]> LoadSpawnGoals(string mapFolder);

    /// <summary>
    /// Load a map into a gameobject from the map folder.
    /// </summary>
    /// <param name="mapFolder">Path to map folder</param>
    /// <returns>Gameobject containing the map.</returns>
    public static GameObject LoadMap(string mapFolder);
}
```

### [MeshSerializer.cs](./Scripts/MeshSerializer.cs)
Functions to serialize and load the terrain mesh normally stored in `<map_name>/<map_name>.bin`.

### [SimpleJSON.cs](./Scripts/SimpleJSON.cs)
Functions for creating and parsing the json files.

### [TerrainLoader.cs](./Scripts/TerrainLoader.cs)
Monobehaviour class that manages loading the mesh, its dependencies and the vertex color shader. This is automatically used when loading a map with MapSerializer.

## Events

To be notified when something interacted with a jump pad or with a special ground (lava or water):
- create a listener inheriting from the specific event listener that you want. [Example for jump pads](./Runtime/Scripts/JumpPad/DefaultJumpPadBehavior.cs)
- place that script on a game object in your scene.
- make sure that your agent has a rigidbody for collision detection (can be kinematic) and a collider.
- if rigidbody is kinematic, go in Editor > Project Settings > Physics > Contact Pairs Mode, set to Enable Kinematic Static Pairs or Enable All Contact Pairs.

