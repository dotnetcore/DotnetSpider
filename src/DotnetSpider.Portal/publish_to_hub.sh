#!/usr/bin/env bash
yarn install
dotnet restore
dotnet build -c Release
dotnet publish -c Release
docker build --tag dotnetspider/portal:latest .
tag=$(date +%Y%m%d%H%M%S)
docker tag dotnetspider/portal:latest dotnetspider/portal:$tag
docker push dotnetspider/portal:latest
docker push dotnetspider/portal:$tag