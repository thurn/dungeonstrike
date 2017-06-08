#!/usr/bin/env python2.7
import lib
env = lib.init()

print("\nRunning Clojure tests...\n")

env.lein(["test"])
