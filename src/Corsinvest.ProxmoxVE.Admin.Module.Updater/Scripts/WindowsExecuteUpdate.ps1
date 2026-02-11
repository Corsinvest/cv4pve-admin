$json = @{
    success = $false
    messages = @()
}

$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')
if (-not $isAdmin) {
    $json.messages += 'The script must be run as Administrator.'
} else {
    $json.messages += 'Starting update search...'

    $updateSession = New-Object -ComObject Microsoft.Update.Session
    $updateSearcher = $updateSession.CreateUpdateSearcher()
    $searchResult = $updateSearcher.Search('IsInstalled=0 AND IsHidden=0')

    if ($searchResult.Updates.Count -eq 0) {
        $json.messages += 'No updates available.'
        $json.success = $true
    } else {
        $updatesToInstall = New-Object -ComObject Microsoft.Update.UpdateColl

        foreach ($update in $searchResult.Updates) {
            if ($update.EulaAccepted -eq $false) {
                $update.AcceptEula()
            }
            $updatesToInstall.Add($update) | Out-Null
        }

        $json.messages += ('Found {0} updates. Downloading...' -f $updatesToInstall.Count)

        $downloader = $updateSession.CreateUpdateDownloader()
        $downloader.Updates = $updatesToInstall
        $downloadResult = $downloader.Download()

        if ($downloadResult.ResultCode -ne 2) {
            $json.messages += 'Error downloading updates.'
        } else {
            $json.messages += 'Download complete. Installing...'

            $installer = $updateSession.CreateUpdateInstaller()
            $installer.Updates = $updatesToInstall
            $installationResult = $installer.Install()

            if ($installationResult.ResultCode -eq 2) {
                $json.messages += 'Updates installed successfully.'
                if ($installationResult.RebootRequired) {
                    $json.messages += 'A system reboot is required.'
                }
                $json.success = $true
            } else {
                $json.messages += ('Error during update installation. Code: {0}' -f $installationResult.ResultCode)
            }
        }
    }
}

Write-Output $json | ConvertTo-Json
