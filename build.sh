#!/bin/sh
rm -rf ~/solutions/DotnetSpider/spider_nuget_packages
dotnet restore
dotnet pack src/HtmlAgilityPack/project.json -o spider_nuget_packages 
dotnet pack src/HtmlAgilityPack.Css/project.json -o spider_nuget_packages
dotnet pack src/DotnetSpider.Core/project.json -o spider_nuget_packages
dotnet pack src/DotnetSpider.Validation/project.json -o spider_nuget_packages
dotnet pack src/DotnetSpider.Redial/project.json -o spider_nuget_packages
dotnet pack src/DotnetSpider.Extension/project.json -o spider_nuget_packages
ftp -n<<!
open redis
user ftpuser 1qazZAQ!
binary
hash
lcd ~/solutions/DotnetSpider/spider_nuget_packages
cd /nuget
prompt
mdelete /nuget/HtmlAgilityPack/0.0.9/*
rmdir /nuget/HtmlAgilityPack/0.0.9
rmdir /nuget/HtmlAgilityPack
mdelete /nuget/HtmlAgilityPack.Css/0.0.9/*
rmdir /nuget/HtmlAgilityPack.Css/0.0.9
rmdir /nuget/HtmlAgilityPack.Css
mdelete /nuget/DotnetSpider.Redial/0.0.9/*
rmdir /nuget/DotnetSpider.Redial/0.0.9
rmdir /nuget/DotnetSpider.Redial
mdelete /nuget/DotnetSpider.Validation/0.0.9/*
rmdir /nuget/DotnetSpider.Validation/0.0.9
rmdir /nuget/DotnetSpider.Validation
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