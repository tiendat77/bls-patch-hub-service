[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact="High")]
param ()
Set-StrictMode -Version 2.0

if (!([bool]([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")))
{
  throw "You must be running as an administrator, please restart as administrator"
}


New-Service -Name PatchHub -DisplayName "Patch Hub Service" -BinaryPathName "C:\Program Files\PatchHubService\PatchHubService.exe" -Description "A service to install software patches" -StartupType Automatic
