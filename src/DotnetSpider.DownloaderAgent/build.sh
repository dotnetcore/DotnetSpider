#!/usr/bin/env bash
dotnet publish -c Release
docker build --tag dotnetspider/downloader-agent:latest .