# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

param(
    [Parameter(Mandatory=$true)]
	[ValidateSet('test','build','publish')]
	[System.String]$operation
)

#Read project version
$xml = [xml](Get-Content ../src/common.props)
$version = $xml.Project.PropertyGroup.Version
Write-Host "Project version: $version"

Write-Host "Operation: $operation"

function Publish-Docker()
{
	.\setKey.ps1

	docker push corsinvest/cv4pve-admin:$version
}

function Build-Docker()
{
	#build documentation
	 .\doc-utils.ps1 build

	Write-Host "Build Docker cv4pve-admin"
	docker rmi corsinvest/cv4pve-admin:$version --force
	docker build --rm -f .\..\src\docker\Dockerfile -t corsinvest/cv4pve-admin:$version "..\"

	#remove unused images
	docker image prune -f
}

function Test-Docker()
{
	#docker data
	$dockerDataBase = "d:\DockerData\cv4pve-admin"

	New-Item -Path "$dockerDataBase\data" -ItemType "directory" -ErrorAction SilentlyContinue

	if (!(Test-Path "$dockerDataBase\appsettings.json"))
	{
		Copy-Item "src\Corsinvest.ProxmoxVE.Admin\appsettings.json" -Destination "$dockerDataBase\appsettings.json"
	}

	docker run --rm -it `
		-p 5000:5000 `
		-e TZ=Europe/Rome `
		-v d:/DockerData/cv4pve-admin/data:/app/data `
		-v d:/DockerData/cv4pve-admin/appsettings.json:/app/appsettings.json `
		--name cv4pve-admin `
		corsinvest/cv4pve-admin:$version
}

if($operation -eq 'test')
{
	Test-Docker
}
elseif($operation -eq 'build')
{
	Build-Docker
}
elseif($operation -eq 'publish')
{
	Publish-Docker
}