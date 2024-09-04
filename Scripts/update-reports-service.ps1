param (
    [Parameter(Mandatory=$true)]
    [string]$url
)

$PatchHubPath = Join-Path $env:TEMP "PatchHubDownloads"
$PatchFile = Join-Path $PatchHubPath "ReportsService.zip"
$DownloadPath = Join-Path $PatchHubPath "ReportsServicePatch"
$ReportsServicePath = "C:\Program Files (x86)\BLogic Systems\BLogic Service\BLogicReportService"

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

    if (-not (Test-Path $DownloadPath)) {
        New-Item -ItemType Directory -Path $DownloadPath | Out-Null
    }

    return $null
}

function Download-File {
    param (
        [string]$url
    )

    try
    {
        Write-Host "Download patch file"
        Invoke-WebRequest -Uri $url -OutFile $PatchFile
    }
    catch {
        $result = "Failed to download patch file"
        Write-Host $result
        return $result
    }

    try {
        Write-Host "Extracting patch file"
        Expand-Archive -LiteralPath $PatchFile -DestinationPath $DownloadPath -Force | Out-File -FilePath "$PatchHubPath\Result.txt"

        Remove-Item -Path $PatchFile -Force
        return $null
    }
    catch {
        $Error[0] | Out-File -FilePath "C:\Program Files (x86)\PatchHubService\Logs\Errorlog.txt" -Append
        $result = "Failed to extract patch files $_.Exception.Message"
        Write-Host $result
        return $result
    }
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
    Copy-Item -Path "$DownloadPath\*" -Destination $ReportsServicePath -Recurse -Force

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

$response = Download-File -url $url
if ($response -ne $null) {
    Clean-Up
    return $response
}

$response = Install-Patch
if ($response -ne $null) {
    Clean-Up
    return $response
}