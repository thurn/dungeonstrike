#!/usr/bin/env python2.7
import os
import lib
env = lib.init()

def call_script_on_staging(name, args=[]):
  """Calls a script on the project copy in the staging area."""
  lib.call([os.path.join(env.staging_path, "scripts", name)] + args)

print("Starting pre-commit checks...")

lib.call([env.script("copy_to_staging_area.py")])

# Note: These should be ordered from shortest to longest runtime.
call_script_on_staging("uncrustify.py")
call_script_on_staging("cljfmt.py")
call_script_on_staging("checksum.py")
call_script_on_staging("run_editor_tests.py")
call_script_on_staging("run_integration.py", ["--test", "all"])
