#!/usr/bin/env python2.7
import os
import lib
env = lib.init()

print("Copying all project files to staging directory...")

lib.call([
  "rsync", "--archive", "--delete", "--quiet",
  env.project_root + os.sep, # Need trailing / to make rsync not create a subdir
  env.staging_path
])
