#!/usr/bin/env pwsh
# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("build", "publish", "run")]
    [string]$Command = "build",

    [Parameter(Mandatory=$false)]
    [ValidateSet("docker", "binary")]
    [string]$Type = "docker"
)

$ErrorActionPreference = "Stop"

$projectPath = "src/Corsinvest.ProxmoxVE.Admin/Corsinvest.ProxmoxVE.Admin.csproj"

# Get properties using MSBuild evaluation (handles Directory.Build.props inheritance automatically)
$currentVersion = (dotnet msbuild $projectPath -getProperty:Version -nologo).Trim()
$containerImageTags = (dotnet msbuild $projectPath -getProperty:ContainerImageTags -nologo).Trim()
$editionName = (dotnet msbuild $projectPath -getProperty:Edition -nologo).Trim()
$containerRepo = (dotnet msbuild $projectPath -getProperty:ContainerRepository -nologo).Trim()

Write-Host "Edition : $editionName" -ForegroundColor Cyan
Write-Host "Version : $currentVersion" -ForegroundColor Cyan
Write-Host "Action: $Command" -ForegroundColor Cyan
Write-Host "Container Repo: $containerRepo" -ForegroundColor Cyan

switch ($Command) {
    "build" {
        switch ($Type) {
            "docker" {
                Write-Host "Building Docker image..." -ForegroundColor Cyan
                Write-Host "Tags: $containerImageTags" -ForegroundColor Yellow

                dotnet publish $projectPath /t:PublishContainer
                if ($LASTEXITCODE -ne 0) { Write-Host "Build failed!" -ForegroundColor Red; exit $LASTEXITCODE }

                Write-Host "`n✓ Docker build completed!" -ForegroundColor Green
                Write-Host "Tags: $containerImageTags" -ForegroundColor Yellow
            }

            "binary" {
                Write-Host "Building binary..." -ForegroundColor Cyan

                dotnet publish $projectPath -c Release -o "./publish"
                if ($LASTEXITCODE -ne 0) { Write-Host "Build failed!" -ForegroundColor Red; exit $LASTEXITCODE }

                Write-Host "`n✓ Binary build completed!" -ForegroundColor Green
                Write-Host "Output: ./publish" -ForegroundColor Yellow
            }
        }
    }

    "publish" {
        Write-Host "Publishing Docker image to registry.hub.docker.com..." -ForegroundColor Cyan
        Write-Host "Tags: $containerImageTags" -ForegroundColor Yellow

        dotnet publish $projectPath /t:PublishContainer -c Release /p:ContainerRegistry=registry.hub.docker.com
        if ($LASTEXITCODE -ne 0) { Write-Host "Publish failed!" -ForegroundColor Red; exit $LASTEXITCODE }

        Write-Host "`n✓ Docker publish completed!" -ForegroundColor Green
        Write-Host "Tags: $containerImageTags" -ForegroundColor Yellow
    }

    "run" {
        $composeFile = "docker-compose-$($editionName.ToLower()).yaml"
        $env:CV4PVE_ADMIN_TAG = $currentVersion
        $env:DATA_DIR = "./data/$($editionName.ToLower())"
        Write-Host "Starting docker compose ($composeFile)..." -ForegroundColor Cyan
        Push-Location "$PSScriptRoot/src/docker"

        try {
            docker compose -f $composeFile up
        }
        finally {
            Write-Host "`nStopping services..." -ForegroundColor Yellow
            docker compose -f $composeFile down
            Pop-Location
        }

        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
}

Write-Host "`n✓ Done!" -ForegroundColor Green
