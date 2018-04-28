#!/usr/bin/env python2.7
from __future__ import print_function

import os
import lib
import json
import re
import collections
env = lib.init()

if not os.path.isfile(env.asset_config_path):
  print("Error: assets.json not found!")
  exit(1)

asset_config = json.load(open(env.asset_config_path, "r"))

class Printer:
  def __init__(self, file):
    self.file = file
    self.indent_level = 0

  def indent(self):
    self.indent_level += 2

  def dedent(self):
    self.indent_level -= 2

  def print(self, string):
    indent = self.indent_level * " "
    print(indent + string, file = self.file)

  def brace(self):
    self.print("{")
    self.indent()

  def unbrace(self):
    self.dedent()
    self.print("}")

Asset = collections.namedtuple("Asset", ["name", "path", "type"])
all_asset_names = set()

def enum_name_from_file_name(filename, config):
  for replace, replace_with in config.get("replace", {}).items():
    filename = filename.replace(replace, replace_with)
  return (config.get("prefix", "") +
    "".join(x.title() for x in filename.split("_")))

def matched_extension(extension, config):
  extensions = set([x.lower() for x in config["extensions"]])
  return extension.lower() in extensions

def type_for_extension(extension, types):
  for type in types:
    if type["extension"] == extension:
      return type
  print("Error: Extension not found in type map " + extension)
  exit(1)

def add_assets_for_directory(asset_map, config, types):
  path = os.path.join(env.assets_dir_path, config["path"])
  for root, dirs, files in os.walk(path):
    for file_path in files:
      (name, extension) = os.path.splitext(file_path)
      extension = extension[1:] # strip leading dot
      if matched_extension(extension, config):
        type = type_for_extension(extension, types)
        relative_path = os.path.relpath(os.path.join(root, file_path),
                                        env.client_root)
        asset_name = enum_name_from_file_name(name, config)
        if asset_name in all_asset_names:
          print("Error: Duplicate asset name! " + asset_name)
          exit(1)
        else:
          all_asset_names.add(asset_name)
        asset_map[type["type"]].add(
          Asset(asset_name, relative_path, type["type"]))

asset_map = collections.defaultdict(set)
for config in asset_config["dirs"]:
  add_assets_for_directory(asset_map, config, asset_config["types"])

header = ("// WARNING: Do not modify this file! This file is automatically\n" +
          "// generated by running 'scripts/generate_asset_references.py'.\n")

def generate_asset_loader(asset_map, p):
  p.print(header)
  p.print("using UnityEngine;")
  p.print("using DungeonStrike.Source.Messaging;\n")
  p.print("namespace DungeonStrike.Source.Assets")
  p.brace()
  p.print("public class AssetUtil")
  p.brace()
  for asset_type, asset_list in asset_map.items():
    if asset_type == "GameObject":
      p.print("public static GameObject InstantiatePrefab(" +
              "AssetRefs refs, PrefabName name, Vector3 position)")
    else:
      p.print("public static " + asset_type + " Get" + asset_type +
              "(AssetRefs refs, " + asset_type + "Name name)")
    p.brace()
    p.print("switch (name)")
    p.brace()
    for value in asset_list:
     if asset_type == "GameObject":
       p.print("case PrefabName." + value.name + ":")
       p.indent()
       p.print("return Object.Instantiate(refs." + value.name +
               ", position, Quaternion.identity);")
       p.dedent()
     else:
       p.print("case " + asset_type + "Name." + value.name + ":")
       p.indent()
       p.print("return refs." + value.name + ";")
       p.dedent()
    p.unbrace()
    p.print("throw new System.InvalidOperationException" \
            "(\"Unknown asset name: \" + name);")
    p.unbrace()
    p.print("")
  p.unbrace()
  p.unbrace()

def generate_refs(asset_map, p):
  p.print(header)
  p.print("using UnityEngine;\n")
  p.print("namespace DungeonStrike.Source.Assets")
  p.brace()
  p.print("public class AssetRefs : MonoBehaviour")
  p.brace()
  for name, values in asset_map.items():
    for value in values:
      p.print("public " + value.type + " " + value.name + ";")
  p.unbrace()
  p.unbrace()

def generate_linker(asset_map, p):
  p.print(header)
  p.print("using UnityEngine;")
  p.print("using UnityEditor;")
  p.print("using DungeonStrike.Source.Assets;\n")
  p.print("namespace DungeonStrike.Source.Assets.Editor")
  p.brace()
  p.print("public class AssetLinker : MonoBehaviour")
  p.brace()
  p.print("public static void LinkAssets(AssetRefs refs)")
  p.brace()
  for name, values in asset_map.items():
    for value in values:
      p.print("refs." + value.name + " = AssetDatabase.LoadAssetAtPath<" + value.type + ">(\"" \
              + value.path + "\");")
      #p.print("refs." + value.name + " = AssetDatabase.LoadAssetAtPath(\"" \
      #        + value.path + "\", typeof(" + value.type + "));")
  p.unbrace()
  p.unbrace()
  p.unbrace()

def hyphenate(name):
    s1 = re.sub('(.)([A-Z][a-z]+)', r'\1-\2', name)
    return re.sub('([a-z0-9])([A-Z])', r'\1-\2', s1).lower()

def generate_assets_clj(asset_map, p):
  p.print(header.replace("//", ";;"))
  p.print("(ns dungeonstrike.generated.assets)\n")
  for type_name, values in asset_map.items():
    if type_name == "GameObject":
      p.print("(def prefab")
    else:
      p.print("(def " + hyphenate(type_name))
    p.indent()
    p.print("#{")
    for value in values:
      p.print(":" + hyphenate(value.name))
    p.dedent()
    p.print("})\n")

output_path = os.path.join(env.assets_dir_path, "Source", "Assets")
generate_asset_loader(asset_map, Printer(
  open(os.path.join(output_path, "AssetUtil.cs"), "w")))
generate_refs(asset_map, Printer(
  open(os.path.join(output_path, "AssetRefs.cs"), "w")))
generate_linker(asset_map, Printer(
  open(os.path.join(output_path, "Editor", "AssetLinker.cs"), "w")))
generate_assets_clj(asset_map, Printer(
  open(os.path.join(env.driver_root, "src", "dungeonstrike", "generated",
                    "assets.clj"), "w")))
env.lein(["cljfmt", "fix"])
