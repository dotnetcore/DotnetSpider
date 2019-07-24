#!/usr/bin/env bash
dotnet build -c Release
dotnet publish -c Release
docker build --tag dotnetspider/downloader-agent:latest .
tag=$(date +%Y%m%d%H%M%S)
docker tag dotnetspider/downloader-agent:latest dotnetspider/downloader-agent:$tag
docker push dotnetspider/downloader-agent:latest
docker push dotnetspider/downloader-agent:$tag