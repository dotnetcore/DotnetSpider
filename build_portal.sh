cd src/DotnetSpider.Portal & yarn install
dotnet publish  -c Release
cp -r src/DotnetSpider.Portal/bin/Release/netcoreapp3.1/publish/ dockerfile/portal/out
cd dockerfile/portal
docker build -t dotnetspider/portal:latest .