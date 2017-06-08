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
  elif elapsed > 5:
    print("Done. " + str(round(elapsed, 2)) + " seconds elapsed.")

def call_script(name, args=[]):
  """Calls a script and times the run time."""
  def fn(): lib.call([env.script(name)] + args)
  time_duration(fn)

def call_script_on_staging(name, args=[]):
  """Calls a script on the project copy in the staging area."""
  def fn(): lib.call([os.path.join(env.staging_path, "scripts", name)] + args)
  time_duration(fn)

print("Starting pre-commit checks...")

# Note: These should be ordered from shortest to longest runtime.
call_script("uncrustify.py")
call_script("cljfmt.py")
call_script("run_driver_tests.py")
call_script("checksum.py")

# Unity tests need to be run on a separate copy of the project, because you
# can't have the same project open in two different copies of Unity at once.
call_script("copy_to_staging_area.py")
call_script_on_staging("run_editor_tests.py")
call_script_on_staging("run_integration.py", ["--test", "all"])
