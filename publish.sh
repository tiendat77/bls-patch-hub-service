#!/bin/bash

rm -rf publish
rm -rf releases

name=$(grep -o '"AppName": *"[^"]*"' appsettings.json | awk -F':' '{print $2}' | tr -d '"' | tr -d ' ')
version=$(grep -o '"AppVersion": *"[^"]*"' appsettings.json | awk -F':' '{print $2}' | tr -d '"' | tr -d ' ')

echo "Publishing $name version $version"

# Create releases folder if it doesn't exist
mkdir -p publish
mkdir -p releases

# Publish the project
dotnet publish --configuration Release --output publish

# Copy asset files to the publish folder
rm publish/appsettings.Development.json
cp README.md publish/
cp LICENSE publish/
cp BuildAssets/* publish/
cp -r Scripts publish/

# Create zip file
cd publish
zip -r "../releases/$name@$version.zip" .

# Clean up
cd ..
rm -rf publish
