dotnet --info
dotnet build DotnetSpider.sln
dotnet test src/DotnetSpider.Core.Test/DotnetSpider.Core.Test.csproj -f netcoreapp2.0
dotnet test src/DotnetSpider.Extension.Test/DotnetSpider.Extension.Test.csproj -f netcoreapp2.0