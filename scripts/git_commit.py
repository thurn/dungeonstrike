#!/usr/bin/env python2.7
import sys
import lib
env = lib.init()

branch = lib.output(["git", "branch"]).rstrip()
all_args = sys.argv[1:]

if branch != "* master":
  print("Not on master, aborting.")
  lib.call(["git", "commit"] + all_args)
  exit()

lib.call([env.script("presubmit.py")])

time = lib.output(["date", "+%Y-%m-%d %H:%M"])
key = libf.output(["md5", "-q", "-s", time])

print("Creating commit...")
lib.call(["git", "commit"] + all_args + ["-m", "KEY: " + key])
