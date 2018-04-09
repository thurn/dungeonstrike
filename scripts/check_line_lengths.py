#!/usr/bin/env python2.7
"""A script to enforce maximum line lengths."""

import os
import lib
env = lib.init()

print("Checking line lengths...")

def check_lengths(directory, extension, max_length):
  failed = False
  for (dirpath, dirname, filenames) in os.walk(directory):
    for name in filenames:
      if name.endswith(extension) and not lib.is_generated(name):
        path = os.path.join(dirpath, name)
        with open(path, 'r') as source_file:
          line_number = 0
          for line in source_file.readlines():
            line_number += 1
            if len(line.rstrip()) > max_length:
              print("Error: " + name + ":" + str(line_number) +
                    " is greater than " + str(max_length) + " characters.")
              failed = True
  return failed

failed = False
failed = failed or check_lengths(
  os.path.join(env.client_root, "Assets", "Source"), ".cs", 120)
failed = failed or check_lengths(
  os.path.join(env.driver_root, "src"), ".clj", 80)
failed = failed or check_lengths(
  os.path.join(env.driver_root, "test"), ".clj", 80)

if failed:
  exit(1)

print("All line lengths OK!")
