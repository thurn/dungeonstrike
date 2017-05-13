#!/usr/bin/env python

import os
import subprocess

THIRD_PARTY = 'DungeonStrike/Assets/ThirdParty'

if not os.getcwd().endswith("dungeonstrike"):
  print("This script must be invoked from the root directory.")
  exit()

subprocess.call(["find", ".", "-name" ,".DS_Store", "-delete"])

for directory in os.listdir(THIRD_PARTY):
  path = THIRD_PARTY + "/" + directory
  if not os.path.isdir(path): continue
  print path
  subprocess.call(["cfv", "-C", "-rr", "-p", path])
  sfv = path + "/" + directory + ".sfv"
  newPath = "checksums/" + directory + ".svf"
  os.rename(sfv, newPath)
  subprocess.call(["perl", "-p", "-i", "-e", "s/Generated.*//g", newPath])

metasum = subprocess.check_output(["./scripts/metasum.sh"])
with open("./DungeonStrike/assets_version.md5", "w") as assets_version:
  assets_version.write(metasum)

print("Checksums updated. Remember to run ./scripts/createThirdParty.sh")
