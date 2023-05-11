# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

#Read project version
$xml = [xml](Get-Content .\src\common.props)
$version = $xml.Project.PropertyGroup.Version
Write-Host "Project version: $version"

New-Item -Path "d:\DockerData\cv4pve-admin\data" -ItemType "directory" 

if (!(Test-Path "d:\DockerData\cv4pve-admin\appsettings.json"))
{
	Copy-Item "src\Corsinvest.ProxmoxVE.Admin\appsettings.json" -Destination "d:\DockerData\cv4pve-admin\appsettings.json"
}

docker run --rm -it `
	-p 5000:5000 `
	-e TZ=Europe/Rome `
	-v d:/DockerData/cv4pve-admin/data:/app/data `
	-v d:/DockerData/cv4pve-admin/appsettings.json:/app/appsettings.json `
	--name cv4pve-admin `
	corsinvest/cv4pve-admin:$version