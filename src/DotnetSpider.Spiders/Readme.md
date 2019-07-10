###

```
docker stop cnblogs
docker rm cnblogs
docker run --name cnblogs \
    -e 'id=36f51567-7b44-4dc3-936b-1a1e077bb608' \
    -e 'name=cnblogs' \
    -e 'type=DotnetSpider.Spiders.CnblogsSpider' \
    registry.intra-pamirs.com/dotnetspider.spiders:latest
               
```
