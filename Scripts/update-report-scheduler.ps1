param (
    [Parameter(Mandatory=$true)]
    [string]$url
)

$PatchHubPath = Join-Path $env:TEMP "PatchHubDownloads"
$PatchFile = Join-Path $PatchHubPath "BLogicReportScheduler.zip"
$DownloadPath = Join-Path $PatchHubPath "BLogicReportSchedulerPatch"
$BLogicReportSchedulerPath = "C:\Program Files (x86)\BLogic Systems\BLogic Service\BLogicViewTaskScheduleHandler"

########################################################################################
# Functions
########################################################################################

function Prepare-Environment {
    if (-not (Test-Path $BLogicReportSchedulerPath)) {
        $result = "BLogicReportScheduler is not installed"
        Write-Host $result
        return $result
    }

    if (-not (Test-Path $DownloadPath)) {
        New-Item -ItemType Directory -Path $DownloadPath | Out-Null
    }

    return $null
}

function Download-File {
    param (
        [string]$url
    )

    try {
        Write-Host "Download patch file"
        Invoke-WebRequest -Uri $url -OutFile $PatchFile
    }
    catch {
        $result = "Failed to download patch file"
        Write-Host $result
        return $result
    }

    Write-Host "Extracting patch file"
    Expand-Archive -Force -Path $PatchFile -DestinationPath $DownloadPath

    if ($?) {
        Write-Host "Extraction successful"
    }
    else {
        $result = "Failed to extract patch file"
        Write-Host $result
        return $result
    }

    Remove-Item -Path $PatchFile -Force

    return $null
}

function Install-Patch {
    Write-Host "Copying patch files"
    Copy-Item -Path "$DownloadPath\*" -Destination $BLogicReportSchedulerPath -Recurse -Force

    if ($?) {
        Write-Host "Successfully copied patch files"
    }
    else {
        $result = "Failed to copy patch files"
        Write-Host $result
        return $result
    }

    Start-Sleep -Seconds 1

    Remove-Item -Path $DownloadPath -Recurse -Force
    Write-Host "Patch installed successfully"

    return $null
}

function Clean-Up {
    Write-Host "Cleaning up temporary files and folders"
    Remove-Item -Path $PatchFile -Force -ErrorAction SilentlyContinue
    Remove-Item -Path $DownloadPath -Recurse -Force -ErrorAction SilentlyContinue
}

########################################################################################
# MAIN
########################################################################################

if (!([bool]([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")))
{
    Start-Process powershell -ArgumentList "-File $($MyInvocation.MyCommand.Path)" -Verb RunAs
    return "Aborting script execution due to lack of Administrator privileges."
}

$response = ""

$response = Prepare-Environment
if ($response -ne $null) {
    Clean-Up
    return $response
}

$response = (Download-File -url $url)
if ($response -ne $null) {
    Clean-Up
    return $response
}

$response = (Install-Patch $PatchFolder)
if ($response -ne $null) {
    Clean-Up
    return $response
}
