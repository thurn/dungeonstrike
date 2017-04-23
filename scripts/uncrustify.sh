#!/bin/sh

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

needs_fixing=$(mktemp)

find "./DungeonStrike/Assets/Source" -type f -iname "*.cs" -print0 | while IFS= read -r -d $'\0' line; do
    temp=$(mktemp)
    uncrustify -l CS -c scripts/uncrustify.cfg -q -o $temp -f $line
    diff=$(diff $line $temp)
    if [ "$diff" != "" ]
    then
        if [ "$1" == "--fix" ]
        then
            echo "Reformatting: $line"
            uncrustify -l CS -c scripts/uncrustify.cfg -q --no-backup $line
            rm $temp
        else
            echo "File requires formatting: $line."
            echo "diff $line $temp"
            echo "needs_fixing" >> $needs_fixing
        fi
    else
        rm $temp
    fi
done

if [ -s $needs_fixing ]
then
    echo "Run './scripts/uncrustify.sh --fix' to reformat."
    rm $needs_fixing
    exit 1
fi

if [ "$1" != "--fix" ]
then
  echo "No Uncrustify issues!"
fi

rm $needs_fixing
