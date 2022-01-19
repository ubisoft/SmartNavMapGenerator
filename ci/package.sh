#!/usr/bin/env bash

set -e
set -x

echo "Making package"

export PACKAGE_PATH=$UNITY_DIR/Builds/
mkdir -p $PACKAGE_PATH

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor} \
  -projectPath $UNITY_DIR \
  -quit \
  -batchmode \
  -nographics \
  -executeMethod BuildUtils.MakePackage \
  -logFile /dev/stdout \
  --output-directory $PACKAGE_PATH

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

ls -la $PACKAGE_PATH
[ -n "$(ls -A $PACKAGE_PATH)" ] # fail job if build folder is empty
