#!/usr/bin/env python2.7
import lib
import os

env = lib.init()

print("\nRunning driver tests...\n")

env.lein(["test"])

print("\nRunning effects tests...\n")

cwd = os.getcwd()
os.chdir(env.effects_root)
lib.call(["lein", "test"], failure_message = "Effects tests failed!")
os.chdir(cwd)
