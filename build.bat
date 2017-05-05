rem build NET45
dotnet build DotnetSpider.net45.sln
rem build NETSTANDARD1.6
dotnet restore DotnetSpider.sln
dotnet build DotnetSpider.sln
cd %cd%\nuget
rem clear old nuget packages
for %%i in (*.nupkg) do del /q/a/f/s %%i
rem create nuget packages
nuget pack DotnetSpider.Core.nuspec
nuget pack DotnetSpider.Extension.nuspec
rem upload nuget packages
for %%i in (*.nupkg) do nuget push %%i -Source https://www.nuget.org/api/v2/package
