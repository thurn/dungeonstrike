#!/bin/sh
set -e

echo "Building Unity client..."
unity_bin="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
project_path="$(pwd)/DungeonStrike"

$unity_bin -quit -batchmode -projectPath $project_path -buildOSX64Player "$project_path/Out/dungeonstrike.app"
