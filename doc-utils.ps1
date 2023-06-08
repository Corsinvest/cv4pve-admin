# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

param(
    [Parameter(Mandatory=$true)]
	[ValidateSet('build')]
	[System.String]$operation
)

#Read project version
$xml = [xml](Get-Content .\src\common.props)
$version = $xml.Project.PropertyGroup.Version
Write-Host "Project version: $version"

Write-Host "Operation: $operation"

function Build-Doc()
{
	Write-Host "Build Documenattion"

	Write-Host "Fix version"
	#replace version
	$pathDoc = ".\src\Corsinvest.ProxmoxVE.Admin\wwwroot\doc\"
	$indexDoc = "$pathDoc\index.adoc"

	$content= [System.IO.File]::ReadAllLines($indexDoc)

	For ($i=0; $i -le $content.Length; $i++) {
		 if ($content[$i].StartsWith(":app-version: v"))
		 { 
			 $content[$i]=":app-version: v$version"
			 break
		 } 
	}
	Set-Content -Path $indexDoc -Value $content

	#create html
	Write-Host "Create html"
	docker run --rm -it -v .\src\Corsinvest.ProxmoxVE.Admin\wwwroot\doc\:/documents/ asciidoctor/docker-asciidoctor asciidoctor index.adoc
	#docker run --rm -it -v .\src\Corsinvest.ProxmoxVE.Admin\wwwroot\doc\:/documents/ asciidoctor/docker-asciidoctor asciidoctor-pdf index.adoc
}

if($operation -eq 'build') 
{ 
	Build-Doc
}