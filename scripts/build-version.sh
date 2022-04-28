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

lasttag=`git describe --abbrev=0 --tags --always || echo unknown`
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
echo "Project Output FilePath: " $projectoutputfile

# Format the replaced JSON and output to version.json file
echo $template | grep -Eo '"[^"]*" *(: *([0-9]*|"[^"]*")[^{}\["]*|,)?|[^"\]\[\}\{]*|\{|\},?|\[|\],?|[0-9 ]*,?' | awk '{if ($0 ~ /^[}\]]/ ) offset-=2; printf "%*c%s\n", offset, " ", $0; if ($0 ~ /^[{\[]/) offset+=2}' > $projectoutputfile

# If command args are passed, call another script
if [ "$1" != "" ]; then
	bash $DIR/copy-version.sh $1 $2
fi
