# Base Map
while [[ $# -gt 0 ]]; do
  key="$1"

  case $key in
    --windows)
      map_generator="./Builds/Windows/MapGenerator.exe"
      shift # past argument
      ;;
    --linux)
      map_generator="./Builds/Linux/map_generator.x86_64"
      shift # past argument
      ;;
  esac
done
mkdir -p TestMapLog

# Base map
mkdir -p Assets/Tests/Maps/BaseMap
$map_generator -batchmode -logFile TestMapLog/BaseMap_log.txt -- generate --seed 0 --nb-maps=1 --scale 1 --volume 5,5,5 --jumppads=False --name="BaseMap" --output="Assets/Tests/Maps/BaseMap"

# Building map
mkdir -p Assets/Tests/Maps/BuildingMap
$map_generator -batchmode -logFile TestMapLog/BuildingMap_log.txt -- generate --seed 0 --nb-maps=1 --scale 1 --volume 5,5,5 --jumppads=False \
                                                                                     --buildings --min-buildings 1 --max-buildings 1 --nb-floors 2 \
                                                                                     --platform-inner-radius 1.5 --platform-outer-radius 1.6 \
                                                                                     --platform-min-height 0 --platform-max-height 0 \
                                                                                     --name="BuildingMap" --output="Assets/Tests/Maps/BuildingMap"

# Jumppad map
mkdir -p Assets/Tests/Maps/JumpPadMap
$map_generator -batchmode -logFile TestMapLog/JumpPadMap_log.txt -- generate --seed 0 --nb-maps=1 --scale 6 --volume 5,5,5 \
                                                                                    --min-jumppads 7 --max-jumppads 7 \
                                                                                    --steepness 4 \
                                                                                    --name="JumpPadMap" --output="Assets/Tests/Maps/JumpPadMap"

# Ground map
mkdir -p Assets/Tests/Maps/GroundMap
$map_generator -batchmode -logFile TestMapLog/GroundMap_log.txt -- generate --seed 0 --nb-maps=1 --scale 1 --volume 5,5,5 --jumppads=False \
                                                                                     --hard-floor-weight 2 --grounds \
                                                                                     --name="GroundMap" --output="Assets/Tests/Maps/GroundMap"

# Ground map with goal
mkdir -p Assets/Tests/Maps/GroundMapWithGoals
$map_generator -batchmode -logFile TestMapLog/GroundMapWithGoals_log.txt -- generate --seed 0 --nb-maps=1 --scale 6 --volume 5,5,5 --jumppads=False \
                                                                                     --hard-floor-weight 2 --grounds \
                                                                                     --spawns-goals 1 \
                                                                                     --name="GroundMapWithGoals" --output="Assets/Tests/Maps/GroundMapWithGoals"