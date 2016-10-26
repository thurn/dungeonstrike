#!/bin/sh

if [ "$#" -ne 2 ]; then
    echo "Usage: createThirdParty [password] [outputPath]"
    exit
fi

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

rm -f $2/ThirdParty.tgz.gpg
tar cz ./DungeonStrike/Assets/ThirdParty | pv --progress --size 2g | gpg --symmetric --batch --passphrase $1 --output $2/ThirdParty.tgz.gpg
