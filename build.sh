dotnet --info
echo 'Run core tests'
dotnet test src/DotnetSpider.Core.Test/DotnetSpider.Core.Test.csproj -f netcoreapp2.0
echo 'Run extension tests'
dotnet test src/DotnetSpider.Extension.Test/DotnetSpider.Extension.Test.csproj -f netcoreapp2.0