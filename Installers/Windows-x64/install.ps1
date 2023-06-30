Write-Host ">>> Installing Microsoft SqlPackage..."

dotnet tool install -g microsoft.sqlpackage

Write-Host ">>> Create SqlPkg application folder..."

md -Force C:\SqlPkg

Write-Host ">>> Copy SqlPkg files..."

robocopy publish C:\SqlPkg /MIR /COPY:DT /FFT /MT

Write-Host ">>> Add SqlPkg to system path..."

Get-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name path
$old = (Get-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name path).path

if ($old.IndexOf('C:\SqlPkg') -lt 1)
{
    $new = "$old;C:\SqlPkg"
    Set-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name path -Value $new
}

Write-Host ">>> SUCCESS!"
Write-Host ">>> Restart PowerShell (or launch command prompt) to begin using SqlPkg"
