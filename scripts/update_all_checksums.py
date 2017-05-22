#!/usr/bin/env python2.7
import os
import lib
env = lib.init()

lib.call(["find", ".", "-name", ".DS_Store", "-delete"])
third_party = os.path.join(env.client_root, 'Assets/ThirdParty')

for directory in os.listdir(third_party):
  path = os.path.join(third_party, directory)
  if not os.path.isdir(path): continue
  print path
  lib.call(["cfv", "-C", "-rr", "-p", path])
  sfv = os.path.join(path, directory + ".sfv")
  newPath = os.path.join(env.checksums_root,  directory + ".svf")
  os.rename(sfv, newPath)
  lib.call(["perl", "-p", "-i", "-e", "s/Generated.*//g", newPath])

metasum = lib.output([env.script("metasum.py")]).rstrip()
version_path = os.path.join(env.client_root, "assets_version.md5")
with open(version_path, "w") as assets_version:
  assets_version.write(metasum)

print("Checksums updated.")

if lib.yesno("Show git status?"):
  lib.call(["git", "status"])

if lib.yesno("Run compress_third_party.py now?"):
  lib.call([env.script("compress_third_party.py")])
