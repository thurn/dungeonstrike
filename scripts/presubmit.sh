#!/bin/bash
set -e

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

# Run Uncrustify formatter
./scripts/uncrustify.sh

# Check for ThirdParty modifications
./scripts/checksum.py

# Run Editor tests
./scripts/runEditorTests.sh
