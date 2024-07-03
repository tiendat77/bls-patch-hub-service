[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact="High")]
param ()
Set-StrictMode -Version 2.0

if (!([bool]([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")))
{
    throw "You must be running as an administrator, please restart as administrator"
}

$scriptpath = $MyInvocation.MyCommand.Path
$scriptdir = Split-Path $scriptpath

$patchHubPath = Join-Path $scriptdir "PatchHubService.exe"

if (-not (Test-Path $patchHubPath)) {
    throw "PatchHubService.exe is not present in script path"
}

# Stop and delete the service if it already exists
if (Get-Service PatchHub -ErrorAction SilentlyContinue)
{
   Stop-Service PatchHub
   sc.exe delete PatchHub 1>$null
}

# Install the service
New-Service -Name PatchHub -DisplayName "Patch Hub Service" -BinaryPathName "`"$patchHubPath`"" -Description "A service to install software patches" -StartupType Automatic | Out-Null
Start-Service PatchHub

# Recovery
sc.exe failure PatchHub reset= 0 actions= restart/60000/restart/60000/restart/60000 1>$null

Write-Host -ForegroundColor Green "Patch Hub Service successfully installed"
