#!/usr/bin/env bash
dotnet publish -c Release
docker build --tag registry.zousong.com:5000/dotnetspider/spiders.startup:latest .