FROM mcr.microsoft.com/dotnet/core/sdk:3.1  AS build-env
WORKDIR /agent
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /agent
COPY --from=build-env /agent/out/ .
ENTRYPOINT ["dotnet", "DotnetSpider.Agent"]