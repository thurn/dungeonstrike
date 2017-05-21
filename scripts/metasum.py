#!/usr/bin/env python2.7
import hashlib
import os
import lib
env = lib.init()

result = hashlib.md5()
checksums = sorted(os.listdir(env.checksums_root))
for checksum in checksums:
  file = os.path.join(env.checksums_root, checksum)
  if os.path.isfile(file):
    with open(file) as hashfile:
      result.update(hashfile.read())
print result.hexdigest()
