###

```
docker stop cnblogs
docker rm cnblogs
docker run --name cnblogs \
    registry.intra-pamirs.com/dotnetspider.spiders:latest -i 36f51567-7b44-4dc3-936b-1a1e077bb608 -t DotnetSpider.Spiders.CnblogsSpider -n 博客园爬虫
               
```
