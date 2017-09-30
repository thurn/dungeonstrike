#!/usr/bin/env python2.7
import os
import shutil
import lib
env = lib.init()

print("Updating checksums...")

lib.call(["find", ".", "-name", ".DS_Store", "-delete"])
third_party = os.path.join(env.client_root, 'Assets/ThirdParty')

for directory in os.listdir(third_party):
  path = os.path.join(third_party, directory)
  if not os.path.isdir(path): continue
  if directory == "Plugins": continue
  print path
  lib.call(["cfv", "-C", "-rr", "-p", path])
  sfv = os.path.join(path, directory + ".sfv")
  newPath = os.path.join(env.checksums_root,  directory + ".svf")
  os.rename(sfv, newPath)
  lib.call(["perl", "-p", "-i", "-e", "s/Generated.*//g", newPath])

print("\nChecksums updated. Updating asset_version.md5...")

metasum = lib.output([env.script("metasum.py")]).rstrip()
version_path = os.path.join(env.client_root, "assets_version.md5")
with open(version_path, "w") as assets_version:
  assets_version.write(metasum)

print("asset_version.md5 updated")

if lib.yesno("Show git status? (y/n)"):
  lib.call(["git", "status"])

keep_going = lib.yesno("Create third party archive now? (y/n)")
if not keep_going:
  exit(0)

print("Creating third party archive...")

third_party = os.path.join(env.client_root, "Assets", "ThirdParty")
checksum = lib.output([env.script("metasum.py")]).rstrip()

size = lib.output(["du", "-sk", third_party]).split("\t")[0]

if os.path.exists(env.third_party_path):
  shutil.rmtree(env.third_party_path)

lib.mkdirs(env.third_party_path)
output_file = os.path.join(env.third_party_path, checksum + ".zip")

lib.call(["7z", "a", "-r",  output_file, third_party])
