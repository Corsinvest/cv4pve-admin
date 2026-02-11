powershell.exe - <<'EOF'
$Searcher = (New-Object -ComObject Microsoft.Update.Session).CreateUpdateSearcher()
$Results = $Searcher.Search('IsInstalled=0 AND Type=''Software'' AND IsHidden=0')
$HasUpdatesNormal = ($Results.Updates | Where-Object { $_.Categories -notcontains 'Security Updates' }).Count -gt 0
$HasUpdatesSecurity = ($Results.Updates | Where-Object { $_.Categories -contains 'Security Updates' }).Count -gt 0
$RequireReboot = (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired') -or (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending')
$json = '{"update_check": true, "update_normal_available": ' + ($HasUpdatesNormal -eq $true).ToString().ToLower() + ', "update_security_available": ' + ($HasUpdatesSecurity -eq $true).ToString().ToLower() + ', "require_reboot": ' + ($RequireReboot -eq $true).ToString().ToLower() + '}'
Write-Output $json
EOF
