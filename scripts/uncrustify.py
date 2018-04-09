#!/usr/bin/env python2.7
import os
import tempfile
import lib
env = lib.init()

source = os.path.join(env.client_root, "Assets", "Source")

print("Checking C# source code formatting...")

for (dirpath, dirname, filenames) in os.walk(source):
  for name in filenames:
    if name.endswith(".cs") and not lib.is_generated(name):
      path = os.path.join(dirpath, name)
      tmp = tempfile.NamedTemporaryFile()
      lib.call([
        "uncrustify",
        "-l", "CS",
        "-c", os.path.join(env.scripts_root, "uncrustify.cfg"),
        "-q",
        "-o", tmp.name,
        "-f", path
      ])
      devnull = open(os.devnull, 'wb')
      result = lib.call_unchecked(["diff", path, tmp.name], stdout=devnull)
      if result:
        print("Formatting error in " + name)
        while True:
          response = lib.input_prompt(
            "[s]how/[f]ix/[n]ext/[q]uit:",
            validator=lambda x: x[0] in ["s", "f", "n", "q"],
            invalid_message="Please enter s, f, n, or q.")
          if response.startswith("s"):
            lib.call_unchecked(["diff", "-C", "3", path, tmp.name])
          elif response.startswith("f"):
            print("Reformatting " + name)
            lib.call([
              "uncrustify",
              "-l", "CS",
              "-c", os.path.join(env.scripts_root, "uncrustify.cfg"),
              "-q", "--no-backup",
              path
            ])
            break
          elif response.startswith("n"):
            break
          else:
            exit(1)
