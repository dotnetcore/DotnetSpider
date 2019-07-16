FROM registry.cn-shanghai.aliyuncs.com/cndotnet/dotnet:2.2-runtime
WORKDIR /agent
COPY ./bin/Release/netcoreapp2.2/publish/ .
ENTRYPOINT ["dotnet","DotnetSpider.DownloaderAgent.dll"]