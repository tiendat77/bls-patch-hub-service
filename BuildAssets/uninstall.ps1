[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact="High")]
param ()
Set-StrictMode -Version 2.0

if (!([bool]([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")))
{
  throw "You must be running as an administrator, please restart as administrator"
}

Remove-Service -Name PatchHub

if ($?) {
  Write-Host "Successfully remove PatchHubService"
}
else {
  sc.exe delete PatchHub
}
