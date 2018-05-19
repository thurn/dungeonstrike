#!/usr/bin/env python2.7
import argparse
import os
import subprocess
import lib
env = lib.init()

parser = argparse.ArgumentParser(
  description="Runs recording-based integration tests.")
parser.add_argument("--test",
                    help="Test to run, or 'all', or 'changed'",
                    required=True)
parser.add_argument("--verbose",
                    action="store_true",
                    help="Should tests be run in verbose mode?")
args = parser.parse_args()

print("\nRunning integration test(s) '" + args.test + "'...\n")

client_logs = os.path.join(env.client_binary_path, "Logs")
lib.rm(os.path.join(client_logs, "client_logs.txt"))
lib.mkdirs(client_logs)

driver_logs = os.path.join(env.driver_jar_path, "logs")
lib.rm(os.path.join(driver_logs, "driver_logs.txt"))
lib.mkdirs(driver_logs)

lib.call([env.script("build_unity_client.py")])
lib.call([env.script("build_driver_jar.py")])

client = subprocess.Popen([
  os.path.join(env.client_binary_path, "Contents", "MacOS", "dungeonstrike"),
  "-batchmode",
  "--port", "59009",
])

print("Started client with pid " + str(client.pid) + "\n")
verbose = ["--verbose"] if args.verbose else []

lib.call([
  "java",
  "-jar", os.path.join(env.driver_jar_path, "driver.jar"),
  "--crash-on-exceptions",
  "--port", "59009",
  "--client-path", env.client_binary_path,
  "--driver-path", env.driver_jar_path,
  "--tests-path", env.tests_root,
  "--test", args.test
] + verbose)
