#!/usr/bin/env bash
dotnet publish -c Release
docker build --tag dotnetspider.spiders:latest .