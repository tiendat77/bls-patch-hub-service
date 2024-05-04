param (
    [Parameter(Mandatory=$true)]
    [string]$url
)

$ReportsServicePath = "C:\Program Files\BLogicsystems\Reports Service"

########################################################################################
# Functions
########################################################################################

function Download-File {
    param (
        [string]$url
    )

    Write-Host "Download patch file"
    $TempFolder = Join-Path $env:TEMP "PatchHubDownloads"
    if (-not (Test-Path $TempFolder)) {
        New-Item -ItemType Directory -Path $TempFolder | Out-Null
    }

    $PatchFile = Join-Path $TempFolder "ReportsService.zip"
    Invoke-WebRequest -Uri $url -OutFile $PatchFile

    Write-Host "Extracting patch file"
    $PatchFolder = Join-Path $TempFolder "ReportsServicePatch"
    if (-not (Test-Path $PatchFolder)) {
        New-Item -ItemType Directory -Path $PatchFolder | Out-Null
    }
    Expand-Archive -Force -Path $PatchFile -DestinationPath $PatchFolder

    Remove-Item -Path $PatchFile -Force

    return $PatchFolder
}

function Install-Patch {
    param (
        [string]$path
    )

    $ServiceName = "BLogicReportsService"

    Write-Host "Stopping '$ServiceName' service"
    Stop-Service -Name -Force $ServiceName

    Write-Host "Copying patch files"
    Copy-Item -Path "$path\*" -Destination $ReportsServicePath -Recurse -Force

    Write-Host "Starting '$ServiceName' service"
    Start-Service -Name $ServiceName

    Remove-Item -Path $path -Recurse -Force
    Write-Host "Patch installed successfully"
}

########################################################################################
# MAIN
########################################################################################

$PatchFolder = Download-File -url $url
Install-Patch $PatchFolder