###

```
docker stop cnblogs
docker rm cnblogs
docker run --name cnblogs \
    -e 'DOTNET_SPIDER_DEVELOPMENT=Development' \
    -e 'DOTNET_SPIDER_ID=36f51567-7b44-4dc3-936b-1a1e077bb608' \
    -e 'DOTNET_SPIDER_NAME=cnblogs' \
    -e 'DOTNET_SPIDER_TYPE=DotnetSpider.Spiders.CnblogsSpider' \
    registry.zousong.com:5000/dotnetspider/spiders.startup:latest
               
```
