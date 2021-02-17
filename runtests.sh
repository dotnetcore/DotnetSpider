#!/usr/bin/env bash
dotnet --info
dotnet test src/DotnetSpider.Tests/DotnetSpider.Tests.csproj -f net5.0 -c release -v n