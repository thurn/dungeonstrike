#!/usr/bin/env python2.7
import lib
env = lib.init()

print("Running Unity editor tests...")

result = env.unity([
  "-batchmode",
  "-projectPath", env.client_root,
  "-runEditorTests"
])

if result == 0:
  print("Editor tests passed!")
