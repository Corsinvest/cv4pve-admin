# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

[System.Console]::Clear();

Write-Output "
    ______                _                      __
   / ____/___  __________(_)___ _   _____  _____/ /_
  / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
 / /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
 \____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/
                                    (Made in Italy)
 =========================================================
 == Build System
 ========================================================="

$pathBasePrj = ".\src\Corsinvest.ProxmoxVE.Admin"
$pathBinRelase = "$pathBasePrj\Bin\Release\net7.0"

Remove-Item -Path ".\$pathBinRelase"  -Recurse -Force

$fileName = "cv4pve-admin"
$rids = @("linux-x64", "linux-arm", "linux-arm64", "osx-x64", "win-x86", "win-x64", "win-arm", "win-arm64")
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