# CV4PVE Admin - Docker Installation Script
# Copyright (c) Corsinvest Srl

$ErrorActionPreference = "Stop"

Write-Host "==================================================================="
Write-Host "           CV4PVE Admin - Docker Installation"
Write-Host "==================================================================="
Write-Host ""
Write-Host "Welcome! This script will download and configure CV4PVE Admin."
Write-Host ""

# Create directory
$DirName = "cv4pve-admin-docker"
if (Test-Path $DirName) {
    Write-Host "Directory '$DirName' already exists!" -ForegroundColor Red
    Write-Host "Please remove it or choose a different location."
    exit 1
}

# Ask for edition
Write-Host "Choose edition:"
Write-Host "  1) Community Edition (CE)"
Write-Host "  2) Enterprise Edition (EE)"
Write-Host ""
$EditionChoice = Read-Host "Select [1-2] (default: 1)"
if ([string]::IsNullOrWhiteSpace($EditionChoice)) { $EditionChoice = "1" }

switch ($EditionChoice) {
    "1" {
        $ComposeFile = "docker-compose-ce.yaml"
        Write-Host "Selected: Community Edition"
    }
    "2" {
        $ComposeFile = "docker-compose-ee.yaml"
        Write-Host "Selected: Enterprise Edition"
    }
    default {
        Write-Host "Invalid choice. Using Community Edition."
        $ComposeFile = "docker-compose-ce.yaml"
    }
}

Write-Host ""
# Show available tags from Docker Hub
Write-Host "Fetching available tags from Docker Hub..."
$DockerRepo = "corsinvest/cv4pve-admin"
if ($ComposeFile -eq "docker-compose-ee.yaml") { $DockerRepo = "corsinvest/cv4pve-admin-ee" }
try {
    $TagsJson = Invoke-RestMethod -Uri "https://hub.docker.com/v2/repositories/$DockerRepo/tags/?page_size=5" -UseBasicParsing
    $RecentTags = ($TagsJson.results | Where-Object { $_.name -ne "latest" } | Select-Object -First 3 | ForEach-Object { $_.name }) -join "  "
    if ($RecentTags) {
        Write-Host "  Recent tags: latest  $RecentTags"
    } else {
        Write-Host "  Recent tags: latest"
    }
} catch {
    Write-Host "  Recent tags: latest"
}
Write-Host "  All tags: https://hub.docker.com/r/$DockerRepo/tags"
Write-Host ""
# Ask for version tag
$Tag = Read-Host "Docker image tag (default: latest)"
$Tag = $Tag.Trim()
if ([string]::IsNullOrWhiteSpace($Tag)) { $Tag = "latest" }
Write-Host "Using tag: $Tag"

Write-Host ""
# Ask for PostgreSQL password
do {
    $SecurePassword = Read-Host "PostgreSQL password (press ENTER for default)" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword)
    $PostgresPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    $PostgresPassword = $PostgresPassword.Trim()

    if ([string]::IsNullOrWhiteSpace($PostgresPassword)) {
        $PostgresPassword = "cv4pve-admin"
        Write-Host "Using default password 'cv4pve-admin' (CHANGE IT LATER!)" -ForegroundColor Yellow
        break
    } elseif ($PostgresPassword.Length -lt 8) {
        Write-Host "Password too short! Minimum 8 characters. Try again." -ForegroundColor Red
    } else {
        Write-Host "Password configured ($($PostgresPassword.Length) characters)"
        break
    }
} while ($true)

Write-Host ""
Write-Host "Creating directory: $DirName"
New-Item -ItemType Directory -Path $DirName | Out-Null
Set-Location $DirName

# Base URL for raw files
$BaseUrl = "https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/src/docker"

# Files to download
$Files = @(
    ".env",
    "docker-compose-ce.yaml",
    "docker-compose-ee.yaml",
    "adminctl",
    "README.md"
)

Write-Host ""
Write-Host "Downloading files..."
foreach ($file in $Files) {
    Write-Host "  - $file"
    try {
        Invoke-WebRequest -Uri "$BaseUrl/$file" -OutFile $file -UseBasicParsing
    }
    catch {
        Write-Host "Failed to download: $file" -ForegroundColor Red
        exit 1
    }
}

# Copy selected compose file to docker-compose.yaml
Copy-Item $ComposeFile docker-compose.yaml
Write-Host "Configured: $ComposeFile -> docker-compose.yaml"

# Update .env file
$envContent = Get-Content .env
if ($Tag -ne "latest") {
    $envContent = $envContent -replace '^CV4PVE_ADMIN_TAG=.*', "CV4PVE_ADMIN_TAG=$Tag"
}
$envContent = $envContent -replace '^POSTGRES_PASSWORD=.*', "POSTGRES_PASSWORD=$PostgresPassword"
$envContent | Set-Content .env
Write-Host "Configuration updated in .env"

Write-Host ""
Write-Host "==================================================================="
Write-Host "                  Installation completed!"
Write-Host "==================================================================="
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. cd $DirName"
Write-Host "  2. docker compose up -d"
Write-Host ""
Write-Host "Access: http://localhost:8080"
Write-Host "Default credentials: admin@local / Password123!"
Write-Host ""
Write-Host "Tip: Use './adminctl' for easy Docker management"
Write-Host ""
