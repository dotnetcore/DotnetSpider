dotnet publish  -c Release
cp -r src/DotnetSpider.Agent/bin/Release/netcoreapp3.1/publish/ dockerfile/agent/out
cd dockerfile/agent || exit
docker build -t dotnetspider/agent:latest .
