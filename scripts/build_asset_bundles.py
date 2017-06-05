#!/usr/bin/env python2.7
import os
import lib
env = lib.init()

print("\nBuilding Unity asset bundles...\n")

env.unity([
  "-quit",
  "-batchmode",
  "-projectPath", env.client_root,
  "-executeMethod",
  "DungeonStrike.Source.Editor.BuildCommands.BuildAssetBundles"
])
