#!/usr/bin/env bash
dotnet build -c Release
dotnet publish -c Release
docker build --tag spiders .
docker tag spiders dotnetspider/spiders.startup:latest
tag=$(date +%Y%m%d%H%M%S)
docker tag spiders dotnetspider/spiders.startup:$tag
docker push dotnetspider/spiders.startup:latest
docker push dotnetspider/spiders.startup:$tag