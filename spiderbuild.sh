#!/bin/sh
dotnet restore
dotnet pack src/HtmlAgilityPack/project.json -o spider_nuget_packages 
dotnet pack src/HtmlAgilityPack.Css/project.json -o spider_nuget_packages
dotnet pack src/MySql.Data/project.json -o spider_nuget_packages
dotnet pack src/Newtonsoft.Json/project.json -o spider_nuget_packages
dotnet pack src/Java2Dotnet.Spider.Common/project.json -o spider_nuget_packages
dotnet pack src/Java2Dotnet.Spider.JLog/project.json -o spider_nuget_packages
dotnet pack src/Java2Dotnet.Spider.Redial/project.json -o spider_nuget_packages
dotnet pack src/Java2Dotnet.Spider.Validation/project.json -o spider_nuget_packages
dotnet pack src/Java2Dotnet.Spider.Core/project.json -o spider_nuget_packages
dotnet pack src/Java2Dotnet.Spider.Extension/project.json -o spider_nuget_packages
ftp -n<<!
open dc01.86research.cn
user nuget 1qazZAQ!
binary
hash
lcd ~/solutions/DotnetSpider/spider_nuget_packages
prompt
mput *
close
bye
!