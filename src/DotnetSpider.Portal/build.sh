#!/usr/bin/env bash
yarn install
dotnet restore
dotnet publish -c Release
docker build --tag dotnetspider/portal:latest .