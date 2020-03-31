#!/usr/bin/env bash
dotnet publish -c Release
docker build -t dotnetspider/agent:latest .