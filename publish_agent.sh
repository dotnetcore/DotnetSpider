cd src/DotnetSpider.Agent
docker build -t dotnetspider/agent:latest .
rm -rf src/DotnetSpider.Agent/out
docker push dotnetspider/downloader-agent:latest