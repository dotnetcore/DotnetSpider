FROM registry.cn-shanghai.aliyuncs.com/cndotnet/dotnet:2.2-aspnetcore-runtime
WORKDIR /portal
COPY ./bin/Release/netcoreapp2.2/publish/ .
ENTRYPOINT ["dotnet", "DotnetSpider.Portal.dll"]