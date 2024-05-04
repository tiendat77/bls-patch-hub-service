[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact="High")]
param ()
Set-StrictMode -Version 2.0

if (!([bool]([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")))
{
    throw "You must be running as an administrator, please restart as administrator"
}

$scriptpath = $MyInvocation.MyCommand.Path
$scriptdir = Split-Path $scriptpath

$patchHubpath = Join-Path $scriptdir "patchhub-service.exe"

if (-not (Test-Path $patchHubpath)) {
    throw "patchhub-service.exe is not present in script path"
}

# Stop and delete the service if it already exists
if (Get-Service patchhub -ErrorAction SilentlyContinue)
{
   Stop-Service patchhub
   sc.exe delete patchhub 1>$null
}

# Install the service
New-Service -Name patchhub -DisplayName "Patch Hub Service" -BinaryPathName "`"$patchHubpath`"" -Description "A service to install software patches" -StartupType Automatic | Out-Null
Start-Service patchhub

# Recovery
sc.exe failure patchhub reset= 0 actions= restart/60000/restart/60000/restart/60000 1>$null

Write-Host -ForegroundColor Green "Patch Hub Service successfully installed"
