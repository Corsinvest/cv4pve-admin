$totalStartTime = Get-Date

$baseUrl = "https://raw.githubusercontent.com/radzenhq/radzen-blazor/refs/heads/master/RadzenBlazorDemos/wwwroot"

$cssFolder = "wwwroot/css"
$fontsFolder = "wwwroot/fonts"

$cssFiles = @(
    "css/fluent-base.css",
    "css/fluent-dark-base.css",
    "css/fluent-dark-wcag.css",
    "css/fluent-wcag.css"
)

$fontFiles = @(
    "fonts/MaterialSymbolsOutlined.woff2",
    "fonts/MaterialSymbolsRounded.woff2"
    "fonts/RobotoFlex.woff2",
    "fonts/SourceSans3VF-Italic.ttf.woff2",
    "fonts/SourceSans3VF-Upright.ttf.woff2"
)

function Download-Files ($files, $destinationFolder, $baseUrl) {
    if (-not (Test-Path $destinationFolder)) {
        New-Item -ItemType Directory -Path $destinationFolder | Out-Null
    }

    foreach ($relativePath in $files) {
        $fileName = [System.IO.Path]::GetFileName($relativePath)
        $outputFile = Join-Path $destinationFolder $fileName
        $url = "$baseUrl/$relativePath"

        if (Test-Path $outputFile) {
            Write-Host "$fileName already exists, skipping download."
            continue
        }

        Write-Host "Downloading $fileName from $url ..."

        try {
            $startTime = Get-Date
            Invoke-WebRequest -Uri $url -OutFile $outputFile -ErrorAction Stop
            Write-Host "Saved to $outputFile (Duration: $(((Get-Date) - $startTime).TotalSeconds) seconds)"
        }
        catch {
            Write-Warning "Failed to download $fileName from $url. Error: $_"
        }
    }
}

Download-Files $cssFiles $cssFolder $baseUrl
Download-Files $fontFiles $fontsFolder $baseUrl

Write-Host "All downloads completed in $((Get-Date) - $totalStartTime).TotalSeconds seconds."
