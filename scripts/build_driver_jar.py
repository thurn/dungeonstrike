#!/usr/bin/env python2.7
import os
import lib
env = lib.init()

print("\nBuilding Driver...\n")

env.lein(["uberjar"])
out = os.path.join(env.driver_root, "out")
lib.mkdirs(out)

os.rename(os.path.join(env.driver_root,
                       "target/dungeonstrike-0.1.0-SNAPSHOT-standalone.jar"),
          os.path.join(env.driver_jar_path, "driver.jar"))

# Uberjar artifacts break tools.namespace reloading, must clean after every
# build:

env.lein(["clean"])
