#!/usr/bin/env bash
dotnet build -c Release
dotnet publish -c Release
docker build --tag registry.zousong.com:5000/dotnetspider/downloader-agent:latest .
docker push registry.zousong.com:5000/dotnetspider/downloader-agent:latest