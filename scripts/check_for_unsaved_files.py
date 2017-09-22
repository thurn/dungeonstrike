#!/usr/bin/env python2.7
import os
import lib
env = lib.init()

unsaved = lib.output(["find", env.project_root, "-name", ".#*"])
if len(unsaved.strip()) > 0:
  print("Error: Unsaved files")
  print unsaved
  exit(1)
