#!/usr/bin/env python2.7
import os
import lib
import time
env = lib.init()

def time_duration(func):
  """Prints the duration a function took to run."""
  """Calls a script on the project copy in the staging area."""
  start = time.time()
  func()
  elapsed = time.time() - start
  if elapsed > 60:
    print("Done. " + str(round(elapsed / 60, 2)) + " minutes elapsed.")
  elif elapsed > 10:
    print("Done. " + str(round(elapsed, 2)) + " seconds elapsed.")

def call_script_on_staging(name, args=[]):
  """Calls a script on the project copy in the staging area."""
  def fn(): lib.call([os.path.join(env.staging_path, "scripts", name)] + args)
  time_duration(fn)

print("Starting pre-commit checks...")

def copy(): lib.call([env.script("copy_to_staging_area.py")])
time_duration(copy)

# Note: These should be ordered from shortest to longest runtime.
call_script_on_staging("uncrustify.py")
call_script_on_staging("cljfmt.py")
call_script_on_staging("checksum.py")
call_script_on_staging("run_editor_tests.py")
call_script_on_staging("run_integration.py", ["--test", "all"])
