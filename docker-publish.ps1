# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

.\setKey.ps1

#Read project version
$xml = [xml](Get-Content .\src\common.props)
$version = $xml.Project.PropertyGroup.Version
Write-Host "Project version: $version"

docker push corsinvest/cv4pve-admin:$version