param (
    [Parameter(Mandatory=$true)]
    [string]$url
)

$ReportsServicePath = "C:\Program Files (x86)\BLogic Systems\BLogic Service\BLogicReportService"
$PatchHubPath = Join-Path $env:TEMP "PatchHubDownloads"
$ReportPatchPath = Join-Path $PatchHubPath "ReportsServicePatch"

########################################################################################
# Functions
########################################################################################

function Prepare-Environment {
    if (-not (Test-Path $ReportsServicePath)) {
        Write-Host "Reports Service is not installed"
        Exit
    }

    if (-not (Get-Service "BLogicReportService" -ErrorAction SilentlyContinue)) {
        Write-Host "Reports Service is not installed"
        Exit
    }

    if (-not (Test-Path $ReportPatchPath)) {
        New-Item -ItemType Directory -Path $ReportPatchPath | Out-Null
    }
}

function Download-File {
    param (
        [string]$url
    )

    try
    {
        Write-Host "Download patch file"
        $PatchFile = Join-Path $PatchHubPath "ReportsService.zip"
        Invoke-WebRequest -Uri $url -OutFile $PatchFile
    }
    catch {
        Write-Host "Failed to download patch file"
        Exit
    }

    Write-Host "Extracting patch file"
    Expand-Archive -Force -Path $PatchFile -DestinationPath $ReportPatchPath

    Remove-Item -Path $PatchFile -Force
}

function Install-Patch {
    $ServiceName = "BLogicReportService"

    Write-Host "Stopping '$ServiceName'"
    Stop-Service -Name $ServiceName -Force

    Start-Sleep -Seconds 1

    Write-Host "Copying patch files"
    Copy-Item -Path "$ReportPatchPath\*" -Destination $ReportsServicePath -Recurse -Force

    Start-Sleep -Seconds 1

    Write-Host "Starting '$ServiceName'"
    Start-Service -Name $ServiceName

    Remove-Item -Path $ReportPatchPath -Recurse -Force
    Write-Host "Patch installed successfully"
}

########################################################################################
# MAIN
########################################################################################

if (!([bool]([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")))
{
    Start-Process powershell -ArgumentList "-File $($MyInvocation.MyCommand.Path)" -Verb RunAs
    Exit
}

Prepare-Environment
Download-File -url $url
Install-Patch $PatchFolder