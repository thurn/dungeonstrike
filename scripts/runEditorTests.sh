#!/bin/bash

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

# Create a backup to run tests on
./scripts/backup.sh

echo "Running editor tests..."
unity_bin="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
project_path="$HOME/backup/DungeonStrike"
$unity_bin -batchmode -projectPath $project_path -runEditorTests
result=$?

if [ $result -eq 0 ]; then
    echo "All Editor Tests Passed"
    exit
elif [ $result -eq 1 ]; then
    echo "Unable to run editor tests (Project is already open/not found?)"
    exit 1
else
    echo "FAILURES in Editor Tests! Code $result."
    exit 1
fi
