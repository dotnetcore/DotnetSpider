cd src/DotnetSpider.Portal
docker build -t dotnetspider/portal:latest .
rm -rf src/DotnetSpider.Portal/out
docker push dotnetspider/portal:latest