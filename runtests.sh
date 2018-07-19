dotnet --info
dotnet test src/DotnetSpider.Common.Test/DotnetSpider.Common.Test.csproj -f netcoreapp2.1 -c release
dotnet test src/DotnetSpider.Core.Test/DotnetSpider.Core.Test.csproj -f netcoreapp2.1 -c release
dotnet test src/DotnetSpider.Downloader.Test/DotnetSpider.Downloader.Test.csproj -f netcoreapp2.1 -c release
dotnet test src/DotnetSpider.Extension.Test/DotnetSpider.Extension.Test.csproj -f netcoreapp2.1 -c release
dotnet test src/DotnetSpider.Extraction.Test/DotnetSpider.Extraction.Test.csproj -f netcoreapp2.1 -c release