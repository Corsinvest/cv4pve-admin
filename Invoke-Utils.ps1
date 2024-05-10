# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

param(
    [Parameter(Mandatory=$true)]
	[ValidateSet('test-docker','build-docker','publish-docker','build-doc','build-exe')]
	[System.String]$operation
)

#Read project version
$xml = [xml](Get-Content ".\src\common.props")
$version = $xml.Project.PropertyGroup.Version
Write-Output "Project version: $version"

Write-Output "Operation: $operation"

function Build-Exe
{
	[System.Console]::Clear();
	Write-LogoCV "Build System"

	Build-Doc
 
	$pathBasePrj = ".\src\Corsinvest.ProxmoxVE.Admin"
	$pathBinRelase = "$pathBasePrj\Bin\Release\net8.0"

	Remove-Item -Path ".\$pathBinRelase"  -Recurse -Force

	$fileName = "cv4pve-admin"
	$rids = @("win-x64", "win-x86", "win-arm64", "linux-x64", "linux-arm", "linux-arm64", "osx-x64", "osx-arm64")

	foreach ($rid in $rids) {
		dotnet publish "$pathBasePrj\Corsinvest.ProxmoxVE.Admin.csproj" `
				-r $rid `
				-c Release `
				--self-contained `
				-p:PublishSingleFile=true `
				-p:IncludeAllContentForSelfExtract=true

		$path = "$pathBinRelase\$rid\publish"

		#fix appsettings.json
		Remove-Item "$path\appsettings.json"
		Remove-Item "$path\appsettings.Development.json"

		Remove-Item "$path\*.pdb"
		Remove-Item "$path\libman.json"

		#compress
		$fileDest = "$pathBinRelase\$fileName-$rid.zip"
		Remove-Item $fileDest -ErrorAction SilentlyContinue
		Get-ChildItem -Path "$path\" | Compress-Archive -DestinationPath $fileDest

		Remove-Item "$pathBinRelase\$rid" -Recurse -Force
	}
}

function Build-Doc
{
	Build-AsciiDoc -Version $version -Path ".\src\Corsinvest.ProxmoxVE.Admin\wwwroot\doc\"
}

function Publish-Docker()
{
	docker push corsinvest/cv4pve-admin:$version
}

function Build-Docker()
{
	Build-Doc

	dotnet clean
		
	Write-Output "Build Docker cv4pve-admin"
	docker rmi corsinvest/cv4pve-admin:$version --force
	docker build --rm -f .\src\docker\Dockerfile -t corsinvest/cv4pve-admin:$version ".\"

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

if($operation -eq 'test-docker')
{
	Test-Docker
}
elseif($operation -eq 'build-docker')
{
	Build-Docker
}
elseif($operation -eq 'publish-docker')
{
	Publish-Docker
}
elseif($operation -eq 'build-doc')
{
	Build-Doc	
}
elseif($operation -eq 'build-exe')
{
	Build-Exe
}


