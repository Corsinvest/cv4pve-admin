#!/usr/bin/env pwsh
# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("build", "publish", "run", "clean-assets", "download-assets")]
    [string]$Command = "build",

    [Parameter(Mandatory=$false)]
    [ValidateSet("docker", "binary")]
    [string]$Type = "docker"
)

$ErrorActionPreference = "Stop"

$projectPath  = "src/Corsinvest.ProxmoxVE.Admin/Corsinvest.ProxmoxVE.Admin.csproj"
$assetsPath   = "src/Corsinvest.ProxmoxVE.Admin/wwwroot"
$markerFile   = "$assetsPath/.radzen-version"
$radzenAssets = @(
    "css/fluent-base.css",
    "css/fluent-dark-base.css",
    "css/fluent-dark-wcag.css",
    "css/fluent-wcag.css",
    "fonts/MaterialSymbolsOutlined.woff2",
    "fonts/MaterialSymbolsRounded.woff2",
    "fonts/RobotoFlex.woff2",
    "fonts/SourceSans3VF-Italic.ttf.woff2",
    "fonts/SourceSans3VF-Upright.ttf.woff2"
)

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

    "download-assets" {
        $packagesProps = [xml](Get-Content "Directory.Packages.props")
        $radzenVersion = ($packagesProps.Project.ItemGroup.PackageVersion | Where-Object { $_.Include -eq "Radzen.Blazor" }).Version
        if (-not $radzenVersion) {
            Write-Error "Could not find Radzen.Blazor version in Directory.Packages.props"
            exit 1
        }

        if (Test-Path $markerFile) {
            $installedVersion = (Get-Content $markerFile).Trim()
            if ($installedVersion -eq $radzenVersion) {
                Write-Host "Assets already at Radzen.Blazor v$radzenVersion, skipping download." -ForegroundColor DarkGray
                break
            }
            Write-Host "Assets version mismatch (installed: $installedVersion, required: $radzenVersion), re-downloading..." -ForegroundColor Yellow
        }

        Write-Host "Downloading assets (Radzen.Blazor v$radzenVersion)..." -ForegroundColor Cyan

        $baseUrl = "https://raw.githubusercontent.com/radzenhq/radzen-blazor/refs/tags/v$radzenVersion/RadzenBlazorDemos/wwwroot"

        $success = $true
        foreach ($asset in $radzenAssets) {
            $dest = "$assetsPath/$asset"
            $fileName = [System.IO.Path]::GetFileName($dest)
            $url = "$baseUrl/$asset"
            Write-Host "  Downloading $fileName..." -ForegroundColor Yellow
            $dir = [System.IO.Path]::GetDirectoryName($dest)
            if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
            try {
                Invoke-WebRequest -Uri $url -OutFile $dest -ErrorAction Stop
            }
            catch {
                Write-Warning "Failed to download $fileName. Error: $_"
                $success = $false
            }
        }

        if ($success) {
            Set-Content -Path $markerFile -Value $radzenVersion -NoNewline
            Write-Host "Assets downloaded successfully (v$radzenVersion)." -ForegroundColor Green
        }
    }

    "clean-assets" {
        Write-Host "Cleaning downloaded assets..." -ForegroundColor Cyan

        @($markerFile) + ($radzenAssets | ForEach-Object { "$assetsPath/$_" }) | ForEach-Object {
            if (Test-Path $_) {
                Remove-Item $_
                Write-Host "  Removed: $_" -ForegroundColor Yellow
            }
        }
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
