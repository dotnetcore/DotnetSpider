#!/usr/bin/env bash
yarn install
dotnet restore
dotnet publish -c Release
docker build --tag registry.zousong.com:5000/dotnetspider/portal:latest .
docker push registry.zousong.com:5000/dotnetspider/portal:latest