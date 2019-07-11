#!/usr/bin/env bash
docker stop $1
docker rm $1
docker pull registry.zousong.com:5000/dotnetspider/downloader-agent:latest
mkdir -p /Users/lewis/dotnetspider/logs
docker run --name $1 -d --restart always -e "DOTNET_SPIDER_AGENTID=$1" -e "DOTNET_SPIDER_AGENTNAME=$1" -e "DOTNET_SPIDER_KAFKACONSUMERGROUP=Agent" -v /Users/lewis/dotnetspider/agent/logs:/logs registry.zousong.com:5000/dotnetspider/downloader-agent:latest