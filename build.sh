dotnet --info
dotnet restore DotnetSpider.sln
dotnet build src/DotnetSpider.Extension.Test/DotnetSpider.Extension.Test.csproj
dotnet test src/DotnetSpider.Core.Test/DotnetSpider.Core.Test.csproj -f netcoreapp2.0 -c Release
dotnet test src/DotnetSpider.Extension.Test/DotnetSpider.Extension.Test.csproj -f netcoreapp2.0 -c Release