#!/usr/bin/env python

import os
import subprocess

THIRD_PARTY = 'DungeonStrike/Assets/ThirdParty'
CHECKSUMS = '../../../../checksums/'

if not os.getcwd().endswith("ds"):
  print("This script must be invoked from the root directory.")
  exit(1)

for directory in os.listdir(THIRD_PARTY):
  path = THIRD_PARTY + "/" + directory
  if not os.path.isdir(path): continue
  file = CHECKSUMS + directory + ".svf"
  check = subprocess.call(["cfv", "-rr", "-p", path, '-f', file])
  if check != 0:
    print("INVALID CHECKSUM for directory " + directory)
    exit(1)

print("\nALL OK")
