FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /agent
COPY ./out .
ENTRYPOINT ["dotnet", "DotnetSpider.Agent.dll"]