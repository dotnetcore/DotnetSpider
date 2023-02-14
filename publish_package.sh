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
dotnet nuget push src/DotnetSpider/bin/Release/*.nupkg -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/DotnetSpider.HBase/bin/Release/*.nupkg -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/DotnetSpider.Mongo/bin/Release/*.nupkg -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/DotnetSpider.MySql/bin/Release/*.nupkg -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/DotnetSpider.PostgreSql/bin/Release/*.nupkg -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate
dotnet nuget push src/DotnetSpider.RabbitMQ/bin/Release/*.nupkg -s $NUGET_SERVER -k $NUGET_KEY --skip-duplicate