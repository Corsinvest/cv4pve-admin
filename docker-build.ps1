# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

#read project version
$xml = [xml](Get-Content .\src\common.props)
$version = $xml.Project.PropertyGroup.Version
Write-Host "Project version: $version"

#admin
Write-Host "Build Docker cv4pve-admin"
docker rmi corsinvest/cv4pve-admin:$version --force
docker build --rm -f "Dockerfile" -t corsinvest/cv4pve-admin:$version "."

#remove unused images
docker image prune -f