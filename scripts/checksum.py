#!/usr/bin/env python
import os
import lib
env = lib.init()

print("Validating checksums...")
third_party = os.path.join(env.client_root, 'Assets/ThirdParty')

for directory in os.listdir(third_party):
  path = os.path.join(third_party, directory)
  if not os.path.isdir(path): continue
  checksum_file = os.path.join(env.checksums_root, directory + ".svf")
  lib.call(["cfv", "-rr", "-q", "-p", path, '-f', checksum_file])
  print(directory + " OK")

print("ALL OK")
