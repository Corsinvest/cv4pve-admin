#!/bin/bash
# CV4PVE Admin - Docker Installation Script
# Copyright (c) Corsinvest Srl

set -e

echo "==================================================================="
echo "           CV4PVE Admin - Docker Installation"
echo "==================================================================="
echo ""
echo "Welcome! This script will download and configure CV4PVE Admin."
echo ""

# Create directory
DIR_NAME="cv4pve-admin-docker"
if [ -d "$DIR_NAME" ]; then
    echo "Directory '$DIR_NAME' already exists!"
    echo "Please remove it or choose a different location."
    exit 1
fi

# Ask for edition
echo "Choose edition:"
echo "  1) Community Edition (CE)"
echo "  2) Enterprise Edition (EE)"
echo ""
read -p "Select [1-2] (default: 1): " EDITION_CHOICE
EDITION_CHOICE=${EDITION_CHOICE:-1}

case $EDITION_CHOICE in
    1)
        COMPOSE_FILE="docker-compose-ce.yaml"
        echo "Selected: Community Edition"
        ;;
    2)
        COMPOSE_FILE="docker-compose-ee.yaml"
        echo "Selected: Enterprise Edition"
        ;;
    *)
        echo "Invalid choice. Using Community Edition."
        COMPOSE_FILE="docker-compose-ce.yaml"
        ;;
esac

echo ""
# Ask for version tag
read -p "Docker image tag (default: latest): " TAG
TAG=$(echo "$TAG" | xargs)  # Trim whitespace
if [ -z "$TAG" ]; then
    TAG="latest"
fi
echo "Using tag: $TAG"

echo ""
# Ask for PostgreSQL password
while true; do
    read -sp "PostgreSQL password (press ENTER for default): " POSTGRES_PASSWORD
    echo ""
    POSTGRES_PASSWORD=$(echo "$POSTGRES_PASSWORD" | xargs)  # Trim whitespace

    if [ -z "$POSTGRES_PASSWORD" ]; then
        POSTGRES_PASSWORD="cv4pve-admin"
        echo "Using default password 'cv4pve-admin' (CHANGE IT LATER!)"
        break
    elif [ ${#POSTGRES_PASSWORD} -lt 8 ]; then
        echo "Password too short! Minimum 8 characters. Try again."
    else
        echo "Password configured (${#POSTGRES_PASSWORD} characters)"
        break
    fi
done

echo ""
echo "Creating directory: $DIR_NAME"
mkdir "$DIR_NAME"
cd "$DIR_NAME"

# Base URL for raw files
BASE_URL="https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/master/src/docker"

# Files to download
FILES=(
    ".env"
    "docker-compose-ce.yaml"
    "docker-compose-ee.yaml"
    "adminctl"
    "README.md"
)

echo ""
echo "Downloading files..."
for file in "${FILES[@]}"; do
    echo "  - $file"
    if ! curl -fsSL -O "$BASE_URL/$file"; then
        echo "Failed to download: $file"
        exit 1
    fi
done

# Copy selected compose file to docker-compose.yaml
cp "$COMPOSE_FILE" docker-compose.yaml
echo "Configured: $COMPOSE_FILE -> docker-compose.yaml"

# Update .env file
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    if [ "$TAG" != "latest" ]; then
        sed -i '' "s/^CV4PVE_ADMIN_TAG=.*/CV4PVE_ADMIN_TAG=$TAG/" .env
    fi
    sed -i '' "s/^POSTGRES_PASSWORD=.*/POSTGRES_PASSWORD=$POSTGRES_PASSWORD/" .env
else
    # Linux
    if [ "$TAG" != "latest" ]; then
        sed -i "s/^CV4PVE_ADMIN_TAG=.*/CV4PVE_ADMIN_TAG=$TAG/" .env
    fi
    sed -i "s/^POSTGRES_PASSWORD=.*/POSTGRES_PASSWORD=$POSTGRES_PASSWORD/" .env
fi
echo "Configuration updated in .env"

# Make adminctl executable
chmod +x adminctl

echo ""
echo "==================================================================="
echo "                  Installation completed!"
echo "==================================================================="
echo ""
echo "Next steps:"
echo "  1. cd $DIR_NAME"
echo "  2. docker compose up -d"
echo ""
echo "Access: http://localhost:8080"
echo "Default credentials: admin@local / Password123!"
echo ""
echo "Tip: Use './adminctl' for easy Docker management"
echo ""
