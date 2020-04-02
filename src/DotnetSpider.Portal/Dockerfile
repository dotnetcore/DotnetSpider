FROM mcr.microsoft.com/dotnet/core/sdk:3.1  AS build-env
WORKDIR /portal
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /portal
COPY --from=build-env /portal/out/ .

ENTRYPOINT ["dotnet", "DotnetSpider.Portal.dll"]