if (Test-Path ".\Argentini.SqlPkg\nupkg") { Remove-Item ".\Argentini.SqlPkg\nupkg" -Recurse -Force }
. ./clean.ps1
Set-Location Argentini.SqlPkg
dotnet pack
Set-Location ..
