#!/usr/bin/env bash
echo $NUGET_SERVER
rm -rf src/DotnetSpider/bin/Release
rm -rf src/DotnetSpider.HBase/bin/Release
rm -rf src/DotnetSpider.Mongo/bin/Release
rm -rf src/DotnetSpider.MySql/bin/Release
rm -rf src/DotnetSpider.PostgreSql/bin/Release
rm -rf src/DotnetSpider.RabbitMQ/bin/Release
dotnet build -c Release
dotnet pack -c Release
nuget push src/DotnetSpider/bin/Release/*.nupkg -Source $NUGET_SERVER
nuget push src/DotnetSpider.HBase/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/DotnetSpider.Mongo/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/DotnetSpider.MySql/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/DotnetSpider.PostgreSql/bin/Release/*.nupkg  -Source $NUGET_SERVER
nuget push src/DotnetSpider.RabbitMQ/bin/Release/*.nupkg  -Source $NUGET_SERVER