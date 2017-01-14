#!/bin/bash

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

echo "Running editor tests..."
unity_bin="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
$unity_bin -batchmode -projectPath "$(pwd)/DungeonStrike" -runEditorTests
result=$?

if [ $result -eq 0 ]; then
    echo "All Editor Tests Passed"
    exit
else
    echo "FAILURES in Editor Tests!"
    exit 1
fi
