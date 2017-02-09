set version_suffix=""
del /q/a/f/s c:\solutions\DotnetSpider\spider_nuget_packages\*.*
dotnet restore
dotnet build c:\solutions\DotnetSpider\src\DotnetSpider2.Extension\project.json
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider2.HtmlAgilityPack\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider2.HtmlAgilityPack.Css\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider2.Core\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider2.Redial\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider2.Extension\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%
for %%i in (C:\solutions\DotnetSpider\spider_nuget_packages\*.symbols.nupkg) do del /q/a/f/s %%i
for %%i in (C:\solutions\DotnetSpider\spider_nuget_packages\*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package