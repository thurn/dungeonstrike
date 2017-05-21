#!/usr/bin/env python2.7
import os
import lib
env = lib.init()

print("Building Unity client...")

env.unity([
  "-quit",
  "-batchmode",
  "-projectPath", env.client_root,
  "-buildOSX64Player", env.client_binary_path
])
