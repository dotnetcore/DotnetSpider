#!/usr/bin/env bash
dotnet --info
dotnet test tests/DotnetSpider.Tests/DotnetSpider.Tests.csproj -f netcoreapp2.2 -c release -v n