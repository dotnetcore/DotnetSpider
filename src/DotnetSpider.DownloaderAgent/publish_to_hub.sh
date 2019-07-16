#!/usr/bin/env bash
dotnet build -c Release
dotnet publish -c Release
docker build --tag downloader-agent .
docker tag downloader-agent dotnetspider/downloader-agent:latest
tag=$(date +%Y%m%d%H%M%S)
docker tag downloader-agent dotnetspider/downloader-agent:$tag
docker push dotnetspider/downloader-agent:latest
docker push dotnetspider/downloader-agent:$tag