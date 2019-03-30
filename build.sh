rem build
dotnet build DotnetSpider.sln -c Release
nuget push %%i -Source http://zlzforever.6655.la:40001/nuget
