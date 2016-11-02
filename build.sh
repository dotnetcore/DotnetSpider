#!/bin/sh
rm -rf ~/solutions/DotnetSpider/spider_nuget_packages
dotnet restore
dotnet build src/DotnetSpider.Extension/project.json
dotnet pack src/DotnetSpider.HtmlAgilityPack/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.HtmlAgilityPack.Css/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.Core/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.Redial/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.Extension/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
nuget push DotnetSpider.HtmlAgilityPack.0.9.1.nupkg
nuget push DotnetSpider.HtmlAgilityPack.Css.0.9.1.nupkg
nuget push DotnetSpider.Core.0.9.1.nupkg
nuget push DotnetSpider.Redial.0.9.1.nupkg
nuget push DotnetSpider.Extension.0.9.1.nupkg