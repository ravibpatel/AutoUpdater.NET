:: .NET Framework 4.5
msbuild "ZipExtractor\ZipExtractor.csproj" /p:TargetFramework=net45;Configuration=Release /verbosity:minimal
msbuild "AutoUpdater.NET\AutoUpdater.NET.csproj" /p:OutputPath=build\lib\net45;TargetFramework=net45;Configuration=Release /verbosity:minimal

:: .NET Core 3.1
dotnet publish --configuration Release --framework netcoreapp3.1 "ZipExtractor\ZipExtractor.csproj" --output "AutoUpdater.NET\Resources"
dotnet publish --configuration Release --framework netcoreapp3.1 "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\netcoreapp3.1"

:: .NET 5.0
dotnet publish --configuration Release --framework net5.0-windows "ZipExtractor\ZipExtractor.csproj" --output "AutoUpdater.NET\Resources"
dotnet publish --configuration Release --framework net5.0-windows "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\net5.0-windows7.0"

:: .NET 6.0
dotnet publish --configuration Release --framework net6.0-windows "ZipExtractor\ZipExtractor.csproj" --output "AutoUpdater.NET\Resources"
dotnet publish --configuration Release --framework net6.0-windows "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\net6.0-windows7.0"

:: .NET 7.0
dotnet publish --configuration Release --framework net7.0-windows "ZipExtractor\ZipExtractor.csproj" --output "AutoUpdater.NET\Resources"
dotnet publish --configuration Release --framework net7.0-windows "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\net7.0-windows7.0"

:: .NET 8.0
dotnet publish --configuration Release --framework net8.0-windows "ZipExtractor\ZipExtractor.csproj" --output "AutoUpdater.NET\Resources"
dotnet publish --configuration Release --framework net8.0-windows "AutoUpdater.NET\AutoUpdater.NET.csproj" --output "AutoUpdater.NET\build\lib\net8.0-windows7.0"

pause