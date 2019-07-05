#!/usr/bin/env bash
dotnet publish -c Release
docker build --tag registry.intra-pamirs.com/dotnetspider.spiders:latest .
docker push registry.intra-pamirs.com/dotnetspider.spiders:latest