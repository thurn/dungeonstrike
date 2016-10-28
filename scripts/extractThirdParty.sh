#!/bin/sh

if [ "$#" -ne 2 ]; then
    echo "Usage: extractThirdParty [password] [ThirdParty.tgz.gpg path]"
    exit
fi

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

mkdir -p ./DungeonStrike/Assets/ThirdParty
gpg --batch --decrypt --passphrase dungeonstr1ke ~/Dropbox/ThirdParty/ThirdParty.tgz.gpg | pv --progress --size 2g | tar xz