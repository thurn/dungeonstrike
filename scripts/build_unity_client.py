#!/usr/bin/env python2.7
import lib
env = lib.init()

print("\nBuilding Unity client...\n")

env.unity([
  "-quit",
  "-batchmode",
  "-projectPath", env.client_root,
  "-buildOSXUniversalPlayer", env.client_binary_path
])
