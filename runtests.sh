#!/usr/bin/env bash
dotnet --info
dotnet test src/DotnetSpider.Tests/DotnetSpider.Tests.csproj -c release -v n