dotnet --info
echo 'Run core tests'
dotnet test src/netstandard2.0/DotnetSpider.Core.Test/DotnetSpider.Core.Test.csproj -f netcoreapp2.0 -c release
echo 'Run extension tests'
dotnet test src/netstandard2.0/DotnetSpider.Extension.Test/DotnetSpider.Extension.Test.csproj -f netcoreapp2.0 -c release