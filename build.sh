#!/bin/sh
rm -rf ~/solutions/DotnetSpider/spider_nuget_packages
dotnet restore
dotnet build src/DotnetSpider.Extension/project.json -f netcoreapp1.0
dotnet pack src/DotnetSpider.HtmlAgilityPack/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.HtmlAgilityPack.Css/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.Core/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.Redial/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
dotnet pack src/DotnetSpider.Extension/project.json -o ~/solutions/DotnetSpider/spider_nuget_packages --no-build
ftp -n<<!
open redis
user ftpuser 1qazZAQ!
binary
hash
lcd ~/solutions/DotnetSpider/spider_nuget_packages
cd /nuget
prompt
mdelete /nuget/DotnetSpider.HtmlAgilityPack/0.0.9/*
rmdir /nuget/DotnetSpider.HtmlAgilityPack/0.0.9
rmdir /nuget/DotnetSpider.HtmlAgilityPack
mdelete /nuget/DotnetSpider.HtmlAgilityPack.Css/0.0.9/*
rmdir /nuget/DotnetSpider.HtmlAgilityPack.Css/0.0.9
rmdir /nugetDotnetSpider./HtmlAgilityPack.Css
mdelete /nuget/DotnetSpider.Redial/0.0.9/*
rmdir /nuget/DotnetSpider.Redial/0.0.9
rmdir /nuget/DotnetSpider.Redial
mdelete /nuget/DotnetSpider.Core/0.0.9/*
rmdir /nuget/DotnetSpider.Core/0.0.9
rmdir /nuget/DotnetSpider.Core
mdelete /nuget/DotnetSpider.Extension/0.0.9/*
rmdir /nuget/DotnetSpider.Extension/0.0.9
rmdir /nuget/DotnetSpider.Extension
mput *
delete 86research02.cache.bin
close
bye
! 