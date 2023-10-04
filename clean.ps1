Set-Location Argentini.SqlPkg
if (Test-Path ".\bin") { Remove-Item bin -Recurse -Force }
if (Test-Path ".\obj") { Remove-Item obj -Recurse -Force }
dotnet restore
Set-Location ..
