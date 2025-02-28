:: Build ZipExtractor
msbuild "ZipExtractor\ZipExtractor.csproj" /p:Configuration=Release /verbosity:minimal

:: .NET Framework 4.6.2
msbuild "AutoUpdater.NET\AutoUpdater.NET.csproj" /p:OutputPath=build\lib\net462;TargetFramework=net462;Configuration=Release /verbosity:minimal
msbuild "AutoUpdater.NET.WebView2\AutoUpdater.NET.WebView2.csproj" /p:OutputPath=build\lib\net462;TargetFramework=net462;Configuration=Release /verbosity:minimal
msbuild "AutoUpdater.NET.Markdown\AutoUpdater.NET.Markdown.csproj" /p:OutputPath=build\lib\net462;TargetFramework=net462;Configuration=Release /verbosity:minimal

:: .NET Core 3.1
dotnet publish --configuration Release --framework netcoreapp3.1 "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\netcoreapp3.1"
dotnet publish --configuration Release --framework netcoreapp3.1 "AutoUpdater.NET.WebView2\AutoUpdater.NET.WebView2.csproj" --output "AutoUpdater.NET.WebView2\build\lib\netcoreapp3.1"
dotnet publish --configuration Release --framework netcoreapp3.1 "AutoUpdater.NET.Markdown\AutoUpdater.NET.Markdown.csproj" --output "AutoUpdater.NET.Markdown\build\lib\netcoreapp3.1"

:: .NET 5.0
dotnet publish --configuration Release --framework net5.0-windows "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\net5.0-windows7.0"
dotnet publish --configuration Release --framework net5.0-windows "AutoUpdater.NET.WebView2\AutoUpdater.NET.WebView2.csproj" --output "AutoUpdater.NET.WebView2\build\lib\net5.0-windows7.0"
dotnet publish --configuration Release --framework net5.0-windows "AutoUpdater.NET.Markdown\AutoUpdater.NET.Markdown.csproj" --output "AutoUpdater.NET.Markdown\build\lib\net5.0-windows7.0"

:: .NET 6.0
dotnet publish --configuration Release --framework net6.0-windows "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\net6.0-windows7.0"
dotnet publish --configuration Release --framework net6.0-windows "AutoUpdater.NET.WebView2\AutoUpdater.NET.WebView2.csproj" --output "AutoUpdater.NET.WebView2\build\lib\net6.0-windows7.0"
dotnet publish --configuration Release --framework net6.0-windows "AutoUpdater.NET.Markdown\AutoUpdater.NET.Markdown.csproj" --output "AutoUpdater.NET.Markdown\build\lib\net6.0-windows7.0"

:: .NET 7.0
dotnet publish --configuration Release --framework net7.0-windows "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\net7.0-windows7.0"
dotnet publish --configuration Release --framework net7.0-windows "AutoUpdater.NET.WebView2\AutoUpdater.NET.WebView2.csproj" --output "AutoUpdater.NET.WebView2\build\lib\net7.0-windows7.0"
dotnet publish --configuration Release --framework net7.0-windows "AutoUpdater.NET.Markdown\AutoUpdater.NET.Markdown.csproj" --output "AutoUpdater.NET.Markdown\build\lib\net7.0-windows7.0"

:: .NET 8.0
dotnet publish --configuration Release --framework net8.0-windows "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\net8.0-windows7.0"
dotnet publish --configuration Release --framework net8.0-windows "AutoUpdater.NET.WebView2\AutoUpdater.NET.WebView2.csproj" --output "AutoUpdater.NET.WebView2\build\lib\net8.0-windows7.0"
dotnet publish --configuration Release --framework net8.0-windows "AutoUpdater.NET.Markdown\AutoUpdater.NET.Markdown.csproj" --output "AutoUpdater.NET.Markdown\build\lib\net8.0-windows7.0"

:: Remove unnecessary files
Powershell.exe -ExecutionPolicy Bypass -NoLogo -NoProfile -Command "Remove-Item -path AutoUpdater.NET\build\lib\* -include *.deps.json -Recurse"
Powershell.exe -ExecutionPolicy Bypass -NoLogo -NoProfile -Command "Remove-Item -path AutoUpdater.NET.Markdown\build\lib\* -include *.deps.json,*.resources.dll,AutoUpdater.NET.dll,AutoUpdater.NET.xml,AutoUpdater.NET.pdb,Markdig.dll -Recurse"
Powershell.exe -ExecutionPolicy Bypass -NoLogo -NoProfile -Command "Remove-Item -path AutoUpdater.NET.WebView2\build\lib\* -include runtimes,*.deps.json,*.resources.dll,Microsoft.Web.WebView2*,AutoUpdater.NET.dll,AutoUpdater.NET.xml,AutoUpdater.NET.pdb -Recurse"

:: Create NuGet packages
nuget pack AutoUpdater.NET\build\Autoupdater.NET.Official.nuspec -Verbosity detailed -OutputDirectory AutoUpdater.NET\build
nuget pack AutoUpdater.NET.WebView2\build\Autoupdater.NET.WebView2.nuspec -Verbosity detailed -OutputDirectory AutoUpdater.NET.WebView2\build
nuget pack AutoUpdater.NET.Markdown\build\Autoupdater.NET.Markdown.nuspec -Verbosity detailed -OutputDirectory AutoUpdater.NET.Markdown\build

pause