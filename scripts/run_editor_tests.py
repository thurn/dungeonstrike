#!/usr/bin/env python2.7
import lib
env = lib.init()

print("\nRunning Unity editor tests...\n")

result = env.unity([
  "-batchmode",
  "-quit", # Documentation says this isn't required, but it's wrong :)
  "-projectPath", env.client_root,
  "-runEditorTests"
])

if result == 0:
  print("Editor tests passed!")
