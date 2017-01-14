#!/bin/bash
set -e

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

# Check for ThirdParty modifications
./scripts/checksum.py

# Create a backup
./scripts/backup.sh

# Run Editor tests
./scripts/runEditorTests.sh

# Run Gendarme static analyzer
./scripts/gendarme.sh

# Run Uncrustify formatter
./scripts/uncrustify.sh
