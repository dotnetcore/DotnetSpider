cd src/DotnetSpider.Portal && yarn install
dotnet publish  -c Release
cd ../.. || exit
cp -r src/DotnetSpider.Portal/bin/Release/netcoreapp3.1/publish/ dockerfile/portal/out
cd dockerfile/portal || exit
docker build -t dotnetspider/portal:latest .