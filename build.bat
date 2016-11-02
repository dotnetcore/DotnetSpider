set version_suffix="rc1"
del /q/a/f/s c:\solutions\DotnetSpider\spider_nuget_packages\*.*
dotnet restore
dotnet build c:\solutions\DotnetSpider\src\DotnetSpider.Extension\project.json
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider.HtmlAgilityPack\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider.HtmlAgilityPack.Css\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider.Core\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider.Redial\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%
dotnet pack c:\solutions\DotnetSpider\src\DotnetSpider.Extension\project.json -o c:\solutions\DotnetSpider\spider_nuget_packages --no-build --version-suffix %version_suffix%