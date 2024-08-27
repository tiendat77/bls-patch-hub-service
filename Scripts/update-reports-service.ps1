param (
    [Parameter(Mandatory=$true)]
    [string]$url
)

$ReportsServicePath = "C:\Program Files (x86)\BLogic Systems\BLogic Service\BLogicReportService"
$PatchHubPath = Join-Path $env:TEMP "PatchHubDownloads"
$ReportPatchPath = Join-Path $PatchHubPath "ReportsServicePatch"

$result = ""

########################################################################################
# Functions
########################################################################################

function Prepare-Environment {
    if (-not (Test-Path $ReportsServicePath)) {
        $result = "Reports Service is not installed"
        Write-Host $result
        return $result
    }

    if (-not (Get-Service "BLogicReportService" -ErrorAction SilentlyContinue)) {
        $result = "Reports Service is not installed"
        Write-Host $result
        return $result
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
        $result = "Failed to download patch file"
        Write-Host $result
        return $result
    }

    Write-Host "Extracting patch file"
    Expand-Archive -Force -Path $PatchFile -DestinationPath $ReportPatchPath

    Remove-Item -Path $PatchFile -Force
}

function Install-Patch {
    $ServiceName = "BLogicReportService"

    Write-Host "Stopping '$ServiceName'"
    Stop-Service -Name $ServiceName -Force

    if ($?) {
        Write-Host "Successfully stopped '$ServiceName'"
    }
    else {
        $result = "Failed to stop '$ServiceName'"
        Write-Host $result
        return $result
    }

    Start-Sleep -Seconds 1

    Write-Host "Copying patch files"
    Copy-Item -Path "$ReportPatchPath\*" -Destination $ReportsServicePath -Recurse -Force

    if ($?) {
        Write-Host "Successfully copied patch files"
    }
    else {
        $result = "Failed to copy patch files"
        Write-Host $result
        return $result
    }

    Start-Sleep -Seconds 1

    Write-Host "Starting '$ServiceName'"
    Start-Service -Name $ServiceName

    if ($?) {
        Write-Host "Successfully started '$ServiceName'"
    }
    else {
        $result = "Failed to start '$ServiceName'"
        Write-Host $result
        return $result
    }

    Remove-Item -Path $ReportPatchPath -Recurse -Force
    Write-Host "Patch installed successfully"
}

########################################################################################
# MAIN
########################################################################################

if (!([bool]([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")))
{
    Start-Process powershell -ArgumentList "-File $($MyInvocation.MyCommand.Path)" -Verb RunAs
    return "Aborting script execution due to lack of Administrator privileges."
}

Prepare-Environment
Download-File -url $url
return Install-Patch $PatchFolder