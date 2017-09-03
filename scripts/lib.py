"""Contains utility functions for writing the command-line scripts in this
   directory. You must ensure that 'env = lib.init()' is the first line of every
   script."""

import collections
import json
import os
import os.path
import shutil
import subprocess

EXPECTED_PROGRAMS = [
  "rsync", "pv", "find", "wc", "lein", "cfv", "git", "md5deep",
  "java", "touch", "python", "7z", "md5"
]

class Env(object):
  def __init__(self, config, scripts_root, project_root):
      self.project_root = project_root
      self.scripts_root = scripts_root
      self.driver_root = os.path.join(project_root, "driver")
      self.effects_root = os.path.join(project_root, "effects")
      self.driver_jar_path = os.path.join(self.driver_root, "out")
      self.client_root = os.path.join(project_root, "DungeonStrike")
      self.client_binary_path = os.path.join(self.client_root,
                                             "Out",
                                             "dungeonstrike.app")
      self.tests_root = os.path.join(project_root, "tests")
      self.checksums_root = os.path.join(project_root, "checksums")
      self.unity_path = config["unity_path"]
      self.staging_path = config["staging_path"]
      self.third_party_path = config["third_party_path"]
      self.check_assets_version()

  def check_assets_version(self):
    """Checks to make sure the assets version currently being used matches the
    one in ThirdParty."""
    with open(os.path.join(self.client_root, "assets_version.md5")) as assets:
      self.assets_version = assets.readline().strip()
    if not os.path.isfile(os.path.join(self.third_party_path,
                                       self.assets_version + ".zip")):
      print("Error: ThirdParty directory does not match current assets " +
            "version '" + self.assets_version + "'")
      print("ThirdParty assets may need to be updated.")
      exit(1)

  def lein(self, args, allow_failure = False):
    """Runs a lein command by changing to the correct directory and invoking
    lein with the provided 'args'."""
    cwd = os.getcwd()
    os.chdir(self.driver_root)
    if allow_failure:
      result = subprocess.call(["lein"] + args)
      os.chdir(cwd)
      return result
    else:
      call(["lein"] + args)
      os.chdir(cwd)

  def script(self, name):
    return os.path.join(self.scripts_root, name)

  def unity(self, args):
    """Invokes Unity with the provided arguments."""
    result = subprocess.call([self.unity_path] + args)
    if result == 1:
      print("Error invoking Unity. Perhaps the project is already open?\n" +
            "Only one instance of Unity can have the project open at once")
      exit(1)
    elif result != 0:
      print("Error invoking Unity. Test failures? Code: " + str(result))
      exit(result)
    else:
      return result

def call(args, failure_message = None):
  """Wrapper around subprocess.check_call"""
  if failure_message:
    result = subprocess.call(args)
    if result != 0:
      print(failure_message)
      exit(result)
  else:
    return subprocess.check_call(args)

def output(args):
  """Wrapper around subprocess.check_output"""
  return subprocess.check_output(args)

def call_unchecked(args, **kwargs):
  """Wrapper around subprocess.call"""
  return subprocess.call(args, **kwargs)

def which(program):
  """Returns the location of a program on the PATH if it can be found, or None
  if it does not exist."""
  def is_exe(fpath):
    return os.path.isfile(fpath) and os.access(fpath, os.X_OK)
  fpath, fname = os.path.split(program)
  if fpath:
    if is_exe(program):
      return program
  else:
    for path in os.environ["PATH"].split(os.pathsep):
      path = path.strip('"')
      exe_file = os.path.join(path, program)
      if is_exe(exe_file):
        return exe_file
  return None

def mkdirs(path):
  """Creates the directory 'path' if needed."""
  if not os.path.exists(path):
    os.makedirs(path)

def rm(path):
  """Removes the file at 'path if it exists."""
  if os.path.exists(path):
    os.remove(path)

def verify_on_path(programs):
  """Verifies that the provided programs can be found on the system path."""
  for program in programs:
    if not which(program):
      print("Program " + program + " not found on path. Please install.")
      exit(1)

def yesno(prompt):
  """Prompts the user to enter y or n in response to a question. Returns True
  for a yes response and False for a no response."""
  while True:
    print(prompt)
    response = raw_input()
    if response == "y":
      return True
    elif response == "n":
      return False

def input_prompt(prompt, default = None, validator = None,
                 invalid_message = None):
  """Displays a prompt for a string input, using the provided default if the
  user inputs the empty string. A 'validator' function can be supplied to check
  the input, 'invalid_message' will be printed if it returns false."""
  while True:
    print(prompt)
    if default:
      print("[default: " + default + "]")
    response = raw_input()
    if response == "" and default:
      response = default
    if validator:
      is_valid = validator(response)
      if is_valid:
        return response
      elif invalid_message:
        print("ERROR: " + invalid_message)
    elif response != "":
      return response

def is_valid_presubmit_dir(path):
  """Validates a presubmit directory selection."""
  if os.path.isfile(path): return False

def prompt_to_create_environment_file(env_path):
  """Prompts the user to create a new environment.json file with the relevant
  script path information in it."""
  response = yesno("No environment.json file found. Create one now? (y/n)")
  if not response:
    exit(0)
  with open(env_path, "w") as env_file:
    unity_path = input_prompt(
      "What is the path to your Unity binary?",
      default = "/Applications/Unity/Unity.app/Contents/MacOS/Unity",
      validator = lambda x: os.path.isfile(os.path.expanduser(x)),
      invalid_message = "Please enter a valid file path.")
    staging_path = input_prompt(
      "What directory should contain a copy of your project for presubmit " +
      "testing? All data at this path will be deleted.",
      default = "/tmp/presubmit",
      validator = lambda x: not os.path.isfile(os.path.expanduser(x)),
      invalid_message = "A file already exists at that path")
    third_party_path = input_prompt(
      "What directory will contain a copy of the third party assets zip file?",
      validator = lambda x: os.path.isdir(os.path.expanduser(x)),
      invalid_message = "That directory does not exist")

    env_file.write(json.dumps({
      "unity_path": os.path.expanduser(unity_path),
      "staging_path": os.path.expanduser(staging_path),
      "third_party_path": os.path.expanduser(third_party_path)
    }))

def init():
  """Initializes the scripting environment. Looks for a file named
  'environment.json' which contains local configuration values. If one is not
  found, prompts the user to create one. Also runs through some useful setup
  instructions.

  A call to this function must be the first line of every script.

  Returns an object representing the environment state.
  """
  scripts_root = os.path.dirname(os.path.realpath(__file__))
  project_root = os.path.abspath(os.path.join(scripts_root, os.pardir))
  env_path = os.path.join(project_root, "environment.json")

  verify_on_path(EXPECTED_PROGRAMS)

  commit_hook = os.path.join(project_root, ".git", "hooks", "commit-msg")
  if not os.path.isfile(commit_hook):
    shutil.copy(os.path.join(scripts_root, "commit-msg.git"), commit_hook)

  if not os.path.isfile(env_path):
    prompt_to_create_environment_file(env_path)
  with open(env_path) as env_file:
    config = json.load(env_file)
    return Env(config, scripts_root, project_root)
