#!/usr/bin/env python2.7
import argparse
import os
import lib
env = lib.init()

parser = argparse.ArgumentParser(
  description='Validates Clojure source code formatting')
parser.add_argument("--fix", action = "store_true", help = "Fix formatting")
args = parser.parse_args()

if args.fix:
  env.lein(["cljfmt", "fix"])
else:
  print("Checking Clojure source code formatting...")
  env.lein(["cljfmt", "check"],
           failure_message = "\n Formatting Error: Run '" +
           __file__ + " --fix' to fix")
