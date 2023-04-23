Write-Host ">>> Create SqlPkg application folder..."

md -Force c:\SqlPkg

Write-Host ">>> Copy SqlPkg files..."

robocopy publish c:\SqlPkg /MIR /COPY:DT /FFT /MT

Write-Host ">>> Add SqlPkg to system path..."

Get-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name path
$old = (Get-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name path).path

if ($old.IndexOf('c:\SqlPkg') -lt 1)
{
    $new = "$old;c:\SqlPkg"
    Set-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name path -Value $new
}

Write-Host ">>> SUCCESS!"
Write-Host ">>> Restart PowerShell (or launch command prompt) to begin using SqlPkg"
