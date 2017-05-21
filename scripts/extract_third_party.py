#!/usr/bin/env python2.7
import os
import shutil
import lib
env = lib.init()

print("Extracing third party archive...")

with open(os.path.join(env.client_root, "assets_version.md5")) as version:
  hash = version.read().rstrip()

third_party = os.path.join(env.client_root, 'Assets/ThirdParty')
if os.path.exists(third_party):
  shutil.rmtree(third_party)
lib.mkdirs(third_party)

lib.call([
  "7z", "x",
  os.path.join(env.third_party_path, hash + ".zip"),
  "-o" + os.path.join(env.client_root, 'Assets')
])
