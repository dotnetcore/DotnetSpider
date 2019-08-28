#!/usr/bin/env bash
rm -rf src/DotnetSpider/bin/Release
rm -rf src/DotnetSpider.ExcelExpression/bin/Release
rm -rf src/DotnetSpider.HBase/bin/Release
rm -rf src/DotnetSpider.Kafka/bin/Release
rm -rf src/DotnetSpider.Mongo/bin/Release
rm -rf src/DotnetSpider.MySql/bin/Release
rm -rf src/DotnetSpider.PostgreSql/bin/Release
dotnet publish DotnetSpider.sln -c Release
nuget push src/DotnetSpider/bin/Release/*.nupkg -source https://www.myget.org/F/zlzforever/api/v3/index.json
nuget push src/DotnetSpider.ExcelExpression/bin/Release/*.nupkg -source https://www.myget.org/F/zlzforever/api/v3/index.json
nuget push src/DotnetSpider.HBase/bin/Release/*.nupkg -source https://www.myget.org/F/zlzforever/api/v3/index.json
nuget push src/DotnetSpider.Kafka/bin/Release/*.nupkg -source https://www.myget.org/F/zlzforever/api/v3/index.json
nuget push src/DotnetSpider.Mongo/bin/Release/*.nupkg -source https://www.myget.org/F/zlzforever/api/v3/index.json
nuget push src/DotnetSpider.MySql/bin/Release/*.nupkg -source https://www.myget.org/F/zlzforever/api/v3/index.json
nuget push src/DotnetSpider.PostgreSql/bin/Release/*.nupkg -source https://www.myget.org/F/zlzforever/api/v3/index.json