#!/usr/bin/env bash
export NUGET_SERVER=https://www.myget.org/F/zlzforever/api/v3/index.json
echo $NUGET_SERVER
sh publish_package.sh