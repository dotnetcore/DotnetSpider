#!/usr/bin/env bash
dotnet build -c Release
dotnet publish -c Release
docker build --tag dotnetspider/spiders.startup:latest .
docker push dotnetspider/spiders.startup:latest