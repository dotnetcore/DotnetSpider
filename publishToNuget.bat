rem build
dotnet build DotnetSpider.sln -c Release
dotnet build DotnetSpider.NET45.sln -c Release
cd %cd%\nuget
rem clear old nuget packages
for %%i in (*.nupkg) do del /q/a/f/s %%i
rem create nuget packages
nuget pack DotnetSpider.Core.nuspec
nuget pack DotnetSpider.Extension.nuspec
rem upload nuget packages
for %%i in (*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package
cd ..
