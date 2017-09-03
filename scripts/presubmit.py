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

call_script("check_for_unsaved_files.py")

print("Checking for banned regexes...")
source_dir = os.path.join(env.client_root, "Assets", "Source")
driver_dir = os.path.join(env.driver_root, "src")
driver_tests_dir = os.path.join(env.driver_root, "test")

lib.ban_regex(source_dir, ".cs", r"Debug\.Log")
lib.ban_regex(driver_dir, ".clj", r"\(p ")
lib.ban_regex(driver_tests_dir, ".clj", r"\(p ")

# Note: These should be ordered from shortest to longest runtime.
call_script("check_line_lengths.py")

call_script("uncrustify.py")

call_script("cljfmt.py")

call_script("run_driver_tests.py")

call_script("checksum.py")

# Unity tests need to be run on a separate copy of the project, because you
# can't have the same project open in two different copies of Unity at once.
call_script("copy_to_staging_area.py")
call_script_on_staging("run_editor_tests.py")
call_script_on_staging("run_integration.py", ["--test", "all"])
