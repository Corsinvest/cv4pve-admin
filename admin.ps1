#!/usr/bin/env pwsh
# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("build", "run")]
    [string]$Command = "build",

    [Parameter(Mandatory=$false)]
    [ValidateSet("docker", "binary")]
    [string]$Type = "docker",
)

$ErrorActionPreference = "Stop"

$projectPath = "src/Corsinvest.ProxmoxVE.Admin/Corsinvest.ProxmoxVE.Admin.csproj"

# Get properties using MSBuild evaluation (handles Directory.Build.props inheritance automatically)
$currentVersion = (dotnet msbuild $projectPath -getProperty:Version -nologo).Trim()
$containerRepo = (dotnet msbuild $projectPath -getProperty:ContainerRepository -nologo).Trim()
$containerImageTags = (dotnet msbuild $projectPath -getProperty:ContainerImageTags -nologo).Trim()
$editionName = $containerRepo.Split('/')[-1]  # "cv4pve-admin" or "cv4pve-admin-ee"

Write-Host "Edition : $editionName" -ForegroundColor Cyan
Write-Host "Version : $currentVersion" -ForegroundColor Cyan

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

    "run" {
        Write-Host "Starting $editionName with docker compose..." -ForegroundColor Cyan
        Push-Location "src/docker"

        try {
            docker compose up
        }
        finally {
            Write-Host "`nStopping services..." -ForegroundColor Yellow
            docker compose down
            Pop-Location
        }

        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
}

Write-Host "`n✓ Done!" -ForegroundColor Green
