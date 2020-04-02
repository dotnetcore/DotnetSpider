#!/usr/bin/env bash
dotnet --info
dotnet test src/DotnetSpider.Tests/DotnetSpider.Tests.csproj -f netcoreapp3.1 -c release -v n