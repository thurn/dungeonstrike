#!/usr/bin/env python2.7
import os
import shutil
import lib
env = lib.init()

print("Creating third party archive...")

third_party = os.path.join(env.client_root, "Assets", "ThirdParty")
checksum = lib.output([env.script("metasum.py")]).rstrip()

size = lib.output(["du", "-sk", third_party]).split("\t")[0]

if os.path.exists(env.third_party_path):
  shutil.rmtree(env.third_party_path)

lib.mkdirs(env.third_party_path)
output_file = os.path.join(env.third_party_path, checksum + ".zip")

lib.call(["7z", "a", "-r",  output_file, third_party])
