#!/bin/sh

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

xml_output=$(mktemp)
filtered=$(mktemp)

echo "Outputting unfiltered results to $xml_output"
gendarme.sh --config "scripts/gendarme.xml" --ignore "scripts/gendarme.ignore" --xml $xml_output --quiet ./DungeonStrike/Library/ScriptAssemblies/Assembly-CSharp.dll
xmllint --xpath "/gendarme-output/results/rule/target[contains(defect/@Source, 'DungeonStrike/Assets/Source')]" $xml_output > $filtered

if [ -s $filtered ]
then
    echo "Gendarme errors!"
    echo "See $filtered"
    exit 1
fi

rm $xml_output
rm $filtered

echo "No Gendarme errors!"
