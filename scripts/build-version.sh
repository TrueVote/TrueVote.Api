#!/bin/bash
echo build-version.sh

# Output the OS
OS=`uname -s`
echo "OS: " $OS

GITVER=`git --version`
echo "Git: " $GITVER

# Set the dir this is running from
DIR=`cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd`
echo "Directory: " $DIR

# Run git and pull some useful values
commit=`git rev-parse --verify HEAD || echo unknown`
echo "Commit: " $commit

branchname=`git rev-parse --abbrev-ref HEAD || echo unknown`
echo "Branchname: " $branchname

lasttag=$(git describe --abbrev=0 --tags --always || git rev-parse --short HEAD || echo "unknown")
echo "Lasttag: " $lasttag

buildtime=`date -u +"%A, %b %d, %Y %H:%M:%S" || echo unknown`
echo "Buildtime: " $buildtime "UTC"

# Read the template file and replace the tokens
template=`cat $DIR/version_template.json`

template=${template//\{\{branch\}\}/$branchname}
template=${template//\{\{buildtime\}\}/$buildtime}
template=${template//\{\{lasttag\}\}/$lasttag}
template=${template//\{\{commit\}\}/$commit}
echo $template

# Set the path to the output file
projectoutputfile=$DIR/../TrueVote.Api/version.json
echo "Version Output FilePath (.json): " $projectoutputfile

# Format the replaced JSON and output to version.json file
#echo $template | grep -Eo '"[^"]*" *(: *([0-9]*|"[^"]*")[^{}\["]*|,)?|[^"\]\[\}\{]*|\{|\},?|\[|\],?|[0-9 ]*,?' | awk '{if ($0 ~ /^[}\]]/ ) offset-=2; printf "%*c%s\n", offset, " ", $0; if ($0 ~ /^[{\[]/) offset+=2}' > $projectoutputfile
echo $template | sed '$s/ *}$/}/' > $projectoutputfile

# Do the same for the Version.cs file
# Read the template .cs file into a variable for replacements
template2=`cat $DIR/Version_Template.cs`

template2=${template2//\{\{branch\}\}/$branchname}
template2=${template2//\{\{buildtime\}\}/$buildtime}
template2=${template2//\{\{lasttag\}\}/$lasttag}
template2=${template2//\{\{commit\}\}/$commit}
echo $template2

# Set the path to the C# output file
projectoutputfile2=$DIR/../TrueVote.Api/Version.cs
echo "Version Output FilePath (.cs): " $projectoutputfile2

# Output the replaced Version.cs file
echo $template2 > $projectoutputfile2

# Send a Git command to ignore these changes
ignoreversion=`git update-index --assume-unchanged $projectoutputfile2`
echo $ignoreversion

# If command args are passed, call another script
if [ "$1" != "" ]; then
	bash $DIR/copy-version.sh $1 $2
fi
