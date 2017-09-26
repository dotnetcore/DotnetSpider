rem build
dotnet build DotnetSpider.sln -c Release
cd %cd%\nuget
rem clear old nuget packages
for %%i in (*.nupkg) do del /q/a/f/s %%i
rem create nuget packages
nuget pack DotnetSpider.Core.nuspec
nuget pack DotnetSpider.Extension.nuspec
rem upload nuget packages
for %%i in (*.nupkg) do nuget push %%i -Source http://zlzforever.6655.la:40001/nuget
cd ..
