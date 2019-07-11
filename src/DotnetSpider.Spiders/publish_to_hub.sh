#!/usr/bin/env bash
dotnet build -c Release
dotnet publish -c Release
docker build --tag spiders .
docker tag spiders dotnetspider/spiders:latest
tag=$(date +%Y%m%d%H%M%S)
docker tag spiders dotnetspider/spiders:$tag
docker push dotnetspider/spiders:latest
docker push dotnetspider/spiders:$tag