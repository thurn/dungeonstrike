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
  result = env.lein(["cljfmt", "check"], allow_failure = True)
  if result != 0 and lib.yesno("Clojure formatting errors. Fix now? (y/n)"):
    env.lein(["cljfmt", "fix"])
