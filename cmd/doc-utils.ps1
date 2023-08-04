# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

param(
	[Parameter(Mandatory = $true)]
	[ValidateSet('build')]
	[System.String]$operation
)

#Read project version
$xml = [xml](Get-Content ../src/common.props)
$version = $xml.Project.PropertyGroup.Version
Write-Output "Project version: $version"

Write-Output "Operation: $operation"

function Build-Doc() {
	Write-Output "Build Documentation"

	Write-Output "Fix version"
	#replace version
	$indexDoc = "..\src\Corsinvest.ProxmoxVE.Admin\wwwroot\doc\index.adoc"
	$content = Get-Content  $indexDoc
	For ($i = 0; $i -le $content.Length; $i++) {
		if ($content[$i].StartsWith(":app-version: v")) {
			$content[$i] = ":app-version: v$version"
			break
		}
	}
	Set-Content -Path $indexDoc -Value $content

	#create html
	Write-Output "Create html"
	docker run --rm -it -v .\..\src\Corsinvest.ProxmoxVE.Admin\wwwroot\doc\:/documents/ asciidoctor/docker-asciidoctor asciidoctor index.adoc
	#docker run --rm -it -v .\src\Corsinvest.ProxmoxVE.Admin\wwwroot\doc\:/documents/ asciidoctor/docker-asciidoctor asciidoctor-pdf index.adoc
}

if ($operation -eq 'build') {
	Build-Doc
}