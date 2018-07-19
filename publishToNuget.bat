rem clear old packages
for %%i in (src\DotnetSpider.Common\bin\Release\*.nupkg) do del /q/a/f/s %%i
for %%i in (src\DotnetSpider.Core\bin\Release\*.nupkg) do del /q/a/f/s %%i
for %%i in (src\DotnetSpider.Proxy\bin\Release\*.nupkg) do del /q/a/f/s %%i
for %%i in (src\DotnetSpider.Downloader\bin\Release\*.nupkg) do del /q/a/f/s %%i
for %%i in (src\DotnetSpider.Extension\bin\Release\*.nupkg) do del /q/a/f/s %%i
for %%i in (src\DotnetSpider.Extraction\bin\Release\*.nupkg) do del /q/a/f/s %%i
for %%i in (src\DotnetSpider.HtmlAgilityPack.Css\bin\Release\*.nupkg) do del /q/a/f/s %%i
rem build
dotnet build DotnetSpider.sln -c Release
rem upload new packages
for %%i in (src\DotnetSpider.Common\bin\Release\*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package
for %%i in (src\DotnetSpider.Core\bin\Release\*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package
for %%i in (src\DotnetSpider.Proxy\bin\Release\*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package
for %%i in (src\DotnetSpider.Downloader\bin\Release\*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package
for %%i in (src\DotnetSpider.Extension\bin\Release\*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package
for %%i in (src\DotnetSpider.Extraction\bin\Release\*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package
for %%i in (src\DotnetSpider.HtmlAgilityPack.Css\bin\Release\*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package