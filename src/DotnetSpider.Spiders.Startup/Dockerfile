FROM registry.cn-shanghai.aliyuncs.com/cndotnet/dotnet:2.2-runtime
WORKDIR /app
COPY ./bin/Release/netcoreapp2.2/publish/ .
ENTRYPOINT ["dotnet","DotnetSpider.Spiders.Startup.dll"]