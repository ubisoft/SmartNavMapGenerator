using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    [Header("Prefab Options")]
    private GameObject[] _wallPrefabs;

    private GameObject[] _roofPrefabs;

    private GameObject[] _windowPrefabs;

    private GameObject[] _doorPrefabs;

    private GameObject[] _floorPrefabs;

    private GameObject[] _outsideStairPrefabs;

    private GameObject[] _insideStairPrefabs;

    public bool GenerateBuildings = false;

    [HideInInspector]
    public PlatformGenerationStep PlatformGenerationStep = new PlatformGenerationStep();

    [Range(0.0f, 1.0f)]
    public float windowPercentChance = 0.3f;

    [Range(0.0f, 1.0f)]
    public float doorPercentChance = 0.2f;

    [Range(0.0f, 1.0f)]
    public float outsideStairPercentChance = 0.2f;

    [Range(0.0f, 1.0f)]
    public float insideStairPercentChance = 0.2f;

    [Range(0.0f, 1.0f)]
    public float newRowColChance = 0.2f;

    [Header("Grid Options")]
    [Range(1, 20)]
    public int rows = 3;

    [Range(1, 20)]
    public int columns = 3;

    public bool randomizeBounds = true;

    [Range(0.0f, 20.0f)]
    public float cellUnitSize = 1;

    [Range(1, 20)]
    public int nbOfFloors = 1;

    [Range(0, 1000)]
    public int DebugSeed = -1;

    private Floor[] floors;

    private List<GameObject> rooms = new List<GameObject>();
    private List<GameObject> buildingGos = new List<GameObject>();

    public List<GameObject> BuildingsGO
    {
        get
        {
            return buildingGos;
        }
    }

    private int prefabCounter = 0;

    private void OnEnable()
    {
        PlatformGenerationStep?.SetEnabledFunc(() => { return GenerateBuildings; });
        FindObjectOfType<TerrainGenerator>()?.AddGenerationStep(PlatformGenerationStep);

        _wallPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Wall");
        _roofPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Roof");
        _windowPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Window");
        _doorPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Door");
        _floorPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Floor");
        _outsideStairPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/OutsideStair");
        _insideStairPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/InsideStair");
    }

    public void Generate(int seed = 0, bool clear = true, float scale = 1, GameObject parent = null)
    {
        if (clear)
            Clear(); 

        if (GenerateBuildings)
        {
            Random.InitState(DebugSeed < -1 ? seed : DebugSeed);
            foreach (Vector3 platform in PlatformGenerationStep.Platforms)
            {
                Generate(platform * scale, false, parent, scale * 0.5f);
            }
        }
    }

    public void Generate(Vector3 position, bool clear = true, GameObject parent = null, float scale = 1)
    {
        prefabCounter = 0;

        GameObject buildingGo = new GameObject($"Building_{position}");
        buildingGo.layer = LayerMask.NameToLayer("Building");

        BuildDataStructure();
        Render(buildingGo);

        buildingGo.transform.localScale = new Vector3(scale, scale, scale);
        Bounds combinedBounds = new Bounds();

        foreach (Renderer renderer in buildingGo.GetComponentsInChildren<Renderer>())
        {
            combinedBounds.Encapsulate(renderer.bounds);
        }

        buildingGo.transform.position = position - new Vector3(combinedBounds.center.x, 0, combinedBounds.center.z);
        buildingGo.transform.parent = parent?.transform;
        buildingGos.Add(buildingGo);
    }

    void BuildDataStructure()
    {
        floors = new Floor[nbOfFloors];

        int floorCount = 0;
        System.Func<float> randomNumber = () => UnityEngine.Random.Range(0.0f, 1.0f);

        // MinRow, MinCol, MaxRow, MaxCol
        buildingBounds initialBounds = new buildingBounds(0, 0, rows, columns);
        buildingBounds currentBounds = new buildingBounds(0, 0, rows, columns);
        buildingBounds previousBounds = new buildingBounds(0, 0, rows, columns);

        foreach (Floor floor in floors)
        {
            Room[,] rooms = new Room[currentBounds.maxRow, currentBounds.maxCol];

            for (int row = currentBounds.minRow; row < currentBounds.maxRow; row++)
            {
                for (int column = currentBounds.minCol; column < currentBounds.maxCol; column++)
                {
                    var roomPosition = new Vector3(row * cellUnitSize, floorCount, column * cellUnitSize);
                    int[] angles = new int[] { 0, 90, 180, -90 };
                    Wall.WallType[] wallTypes = new Wall.WallType[] { Wall.WallType.Blank, Wall.WallType.Blank, Wall.WallType.Blank, Wall.WallType.Blank };

                    // We only create the outside walls
                    if (row == currentBounds.minRow)
                        wallTypes[2] = CanBeAStair(currentBounds.minRow, previousBounds.minRow, floorCount) ? Wall.WallType.PotentialStair : Wall.WallType.Normal;

                    if (row == currentBounds.maxRow - 1)
                    {
                        wallTypes[0] = CanBeAStair(currentBounds.maxRow, previousBounds.maxRow, floorCount) ? Wall.WallType.PotentialStair : Wall.WallType.Normal;
                    }

                    if (column == currentBounds.minCol)
                        wallTypes[1] = CanBeAStair(currentBounds.minCol, previousBounds.minCol, floorCount) ? Wall.WallType.PotentialStair : Wall.WallType.Normal;

                    if (column == currentBounds.maxCol - 1)
                        wallTypes[3] = CanBeAStair(currentBounds.maxCol, previousBounds.maxCol, floorCount) ? Wall.WallType.PotentialStair : Wall.WallType.Normal;

                    rooms[row, column] = new Room(roomPosition, floorCount, (floorCount == floors.Length - 1));
                    for (int i = 0; i < 4; i++)
                    {
                        rooms[row, column].walls[i] = new Wall(roomPosition, Quaternion.Euler(0, angles[i], 0), wallTypes[i]);
                    }
                }
            }

            floors[floorCount] = new Floor(floorCount++, rooms, currentBounds);

            if (randomizeBounds)
            {
                previousBounds = currentBounds;
                //UnityEngine.Random.
                currentBounds.minRow = randomNumber() < newRowColChance ? currentBounds.minRow : UnityEngine.Random.Range(currentBounds.minRow, currentBounds.maxRow);
                currentBounds.minCol = randomNumber() < newRowColChance ? currentBounds.minCol : UnityEngine.Random.Range(currentBounds.minCol, currentBounds.maxCol);
                currentBounds.maxRow = randomNumber() < newRowColChance ? currentBounds.maxRow : UnityEngine.Random.Range(currentBounds.minRow, currentBounds.maxRow + 1);
                currentBounds.maxCol = randomNumber() < newRowColChance ? currentBounds.maxCol : UnityEngine.Random.Range(currentBounds.minCol, currentBounds.maxCol + 1);
            }
        }

        // We add roofs to the rooms which don't have rooms above them
        // We add inside stairs to the rooms which have rooms above them and that aren't on the "outside" on the current floor
        if (floorCount > 0)
        {
            for (int floor = 1; floor < floorCount; floor++)
            {
                currentBounds = floors[floor].bounds;
                previousBounds = floors[floor - 1].bounds;

                for (int row = previousBounds.minRow; row < previousBounds.maxRow; row++)
                {
                    for (int column = previousBounds.minCol; column < previousBounds.maxCol; column++)
                    {
                        if ((row < currentBounds.minRow) || (row >= currentBounds.maxRow) || (column < currentBounds.minCol) || (column >= currentBounds.maxCol))
                            floors[floor - 1].rooms[row, column].hasRoof = true;

                        if ((row >= currentBounds.minRow) && (row < currentBounds.maxRow) && (column >= currentBounds.minCol) && (column < currentBounds.maxCol))
                        {
                            if ((!floors[floor].hasStairs) && (UnityEngine.Random.Range(0.0f, 1.0f) < insideStairPercentChance))
                            {
                                floors[floor].hasStairs = true;
                                floors[floor - 1].rooms[row, column].hasStair = true;
                                floors[floor - 1].rooms[row, column].hasRoof = false;
                                floors[floor].rooms[row, column].hasFloor = false;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool CanBeAStair(int currentMaxIndex, int previousMaxIndex, int floor)
    {
        return ((floor == 0) || (currentMaxIndex < previousMaxIndex));
    }

    private void Render(GameObject buildingGo)
    {
        for ( int floor_ix = 0; floor_ix < floors.Length; floor_ix++)
        {
            Floor floor = floors[floor_ix];
            for (int row = floor.bounds.minRow; row < floor.bounds.maxRow; row++)
            {
                for (int column = floor.bounds.minCol; column < floor.bounds.maxCol; column++)
                {
                    Room room = floor.rooms[row, column];
                    GameObject roomGo = new GameObject($"Room_{row}_{column}_{floor_ix}");
                    rooms.Add(roomGo);
                    roomGo.transform.parent = buildingGo.transform;
                    RoomPlacement(ref floor, room, roomGo);
                }
            }
        }
    }

    private void RoomPlacement(ref Floor floor, Room room, GameObject roomGo)
    {
        foreach(Wall wall in room.walls)
        {
            if (wall.wallTypeSelected != Wall.WallType.Blank)
            {
                SpawnPrefab(GetWallPrefab(ref floor, wall.wallTypeSelected, room.floorNumber), roomGo.transform, wall.position, wall.rotation);
            }
        }

        if (room.hasRoof)
            SpawnPrefab(GetRandomPrefab(_roofPrefabs), roomGo.transform, room.walls[0].position, room.walls[0].rotation);

        if (room.hasFloor)
            SpawnPrefab(GetRandomPrefab(_floorPrefabs), roomGo.transform, room.walls[0].position, room.walls[0].rotation);

        if (room.hasStair)
            SpawnPrefab(GetRandomPrefab(_insideStairPrefabs), roomGo.transform, room.walls[0].position, room.walls[UnityEngine.Random.Range(0, 4)].rotation);
    }

    private GameObject GetWallPrefab(ref Floor floor, Wall.WallType wallType, int floorNb)
    {
        System.Func<float> randomNumber = () => UnityEngine.Random.Range(0.0f, 1.0f);
        if (floorNb == 0)
        {
            if ((wallType == Wall.WallType.PotentialStair) && (randomNumber() < outsideStairPercentChance))
            {
                return GetRandomPrefab(_outsideStairPrefabs);
            }
            else
            {
                return GetRandomPrefab(randomNumber() < doorPercentChance ? _doorPrefabs : _wallPrefabs);
            }
        }
        else
        {
            if ((wallType == Wall.WallType.PotentialStair) && (randomNumber() < outsideStairPercentChance))
            {
                return GetRandomPrefab(_outsideStairPrefabs);
            }
            else
            {
                return GetRandomPrefab(randomNumber() < windowPercentChance ? _windowPrefabs : _wallPrefabs);
            }
        }
    }

    private GameObject GetRandomPrefab(GameObject[] prefabs)
    {
        Debug.Assert(prefabs.Length > 0);
        int Index = UnityEngine.Random.Range(0, prefabs.Length);
        return prefabs[Index];
    }

    private void SpawnPrefab(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        var gameObject = Instantiate(prefab, transform.position + position, rotation);
        gameObject.transform.parent = parent;
        gameObject.name = $"{gameObject.name}_{prefabCounter}";
        gameObject.tag = "BuildingPrefab";
        prefabCounter++;
    }

    public void Clear()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            DestroyImmediate(rooms[i]);
        }
        rooms.Clear();

        for (int i = 0; i < buildingGos.Count; i++)
        {
            DestroyImmediate(buildingGos[i]);
        }
        buildingGos.Clear();
    }

    private void Reset()
    {
        PlatformGenerationStep?.SetEnabledFunc(() => { return GenerateBuildings; });
        FindObjectOfType<TerrainGenerator>()?.AddGenerationStep(PlatformGenerationStep);

        _wallPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Wall");
        _roofPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Roof");
        _windowPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Window");
        _doorPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Door");
        _floorPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/Floor");
        _outsideStairPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/OutsideStair");
        _insideStairPrefabs = Resources.LoadAll<GameObject>("Building Assets/Prefabs/InsideStair");
    }
}