#!/bin/sh
set -e

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

if [ "$#" -ne 1 ]; then
    echo "Usage: compressThirdParty [outputPath]"
    exit
fi

# TODO: This only works on OSX, update to support GNU find
CHECKSUM=$(./scripts/metasum.sh)
OUT="$1/$CHECKSUM.tgz"
SIZE=$(du -sk ./DungeonStrike/Assets/ThirdParty | cut -f 1)

rm -f "$1/*.tgz"

tar cf - ./DungeonStrike/Assets/ThirdParty | pv --progress --size ${SIZE}k | gzip > $OUT
