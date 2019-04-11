###

```
docker stop xxxxxxxx
docker rm xxxxxxxx
docker run --name xxxxxxxx \
    -e dotnetspider.spider.id=xxxxxxxx \
    -e dotnetspider.spider.class=DotnetSpider.Spiders.CnblogsSpider \
    -e dotnetspider.spider.name=博客园爬虫 \
    -v ~/logs:/logs \
    zlzforever/dotnetspider.spiders   
               
```
