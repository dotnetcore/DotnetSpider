#!/usr/bin/env bash
dotnet build -c Release
dotnet publish -c Release
docker build --tag portal .
docker tag portal dotnetspider/portal:latest
tag=$(date +%Y%m%d%H%M%S)
docker tag portal dotnetspider/portal:$tag
docker push dotnetspider/portal:latest
docker push dotnetspider/portal:$tag