# Remove publish and releases folders if they exist
Remove-Item -Recurse -Force -ErrorAction Ignore publish
Remove-Item -Recurse -Force -ErrorAction Ignore releases

# Extract AppName and AppVersion from appsettings.json
$name = (Get-Content appsettings.json) -join "`n" | ConvertFrom-Json | Select -ExpandProperty "AppName"
$version = (Get-Content appsettings.json) -join "`n" | ConvertFrom-Json | Select -ExpandProperty "AppVersion"

Write-Host "Publishing $name version $version"

# Create releases folder if it doesn't exist
New-Item -ItemType Directory -Force -Path publish
New-Item -ItemType Directory -Force -Path releases

# Publish the project
dotnet publish --configuration Release --output publish

# Copy asset files to the publish folder
Remove-Item -Force -ErrorAction Ignore publish/appsettings.Development.json
Copy-Item README.md publish/
Copy-Item LICENSE publish/
Copy-Item BuildAssets\* publish\ -Recurse

# Create zip file
Set-Location publish
Compress-Archive -Path * -DestinationPath "../releases/$name@$version.zip" -Force

# Clean up
Set-Location ..
# Remove-Item -Recurse -Force publish