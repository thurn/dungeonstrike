#!/bin/sh

if [ "$#" -ne 1 ]; then
    echo "Usage: extractThirdParty [.tgz path]"
    exit
fi

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

rm -r ./DungeonStrike/Assets/ThirdParty
mkdir -p ./DungeonStrike/Assets/ThirdParty
SIZE=$(du -k $1 | cut -f 1)
pv --progress --size ${SIZE}k $1 | tar xzf -
