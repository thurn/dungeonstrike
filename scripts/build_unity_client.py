#!/usr/bin/env python2.7
import lib
env = lib.init()

lib.call([env.script("build_asset_bundles.py")])

print("\nBuilding Unity client...\n")

env.unity([
  "-quit",
  "-batchmode",
  "-projectPath", env.client_root,
  "-buildOSXUniversalPlayer", env.client_binary_path
])
