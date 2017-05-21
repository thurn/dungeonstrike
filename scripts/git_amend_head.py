#!/usr/bin/env python2.7
import sys
import re
import lib
env = lib.init()

branch = lib.output(["git", "branch"]).rstrip()
all_args = sys.argv[1:]

if branch != "* master":
  print("Not on master, aborting.")
  lib.call(["git", "commit", "-a", "--amend"] + all_args)
  exit()

lib.call([env.script("presubmit.py")])

time = lib.output(["date", "+%Y-%m-%d %H:%M"])
key = lib.output(["md5", "-q", "-s", time]).rstrip()
previous_message = lib.output(["git", "log", "-1", "--pretty=%B"]).rstrip()
without_key = re.sub(r"KEY:.*", "", previous_message).rstrip()

print("Creating commit...")
lib.call(
  ["git", "commit"] +
  all_args +
  ["-a", "--amend", "-m", without_key, "-m", "KEY: " + key]
)
