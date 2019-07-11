#!/usr/bin/env bash
docker stop dotnetspider.portal
docker rm  dotnetspider.portal
docker run --name dotnetspider.portal -d -p 7897:7896 -v /Users/lewis/dotnetspider/portal/appsettings.json:/portal/appsettings.json -v /Users/lewis/dotnetspider/portal/logs:/logs registry.zousong.com:5000/dotnetspider/portal:latest