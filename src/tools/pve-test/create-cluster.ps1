# SPDX-FileCopyrightText: Copyright Corsinvest Srl
# SPDX-License-Identifier: AGPL-3.0-only

# Makes pve-1, pve-2 and pve-3 into the cluster "cvtest". Run it once after "docker compose up -d".
# The containers have no volumes: after "docker compose down" the cluster is gone and this is run again.

$ErrorActionPreference = 'Stop'
$InformationPreference = 'Continue'
$password = 'proxmox123'

function Wait-Node([string] $name) {
    Write-Information "Waiting for $name..."
    for ($i = 0; $i -lt 60; $i++) {
        docker exec $name sh -c 'systemctl is-active --quiet pveproxy pvedaemon pve-cluster' 2>$null
        if ($LASTEXITCODE -eq 0) { return }
        Start-Sleep -Seconds 2
    }
    throw "$name is not ready"
}

'pve-1', 'pve-2', 'pve-3' | ForEach-Object { Wait-Node $_ }

docker exec pve-1 sh -c 'test -f /etc/pve/corosync.conf'
if ($LASTEXITCODE -eq 0) {
    Write-Information 'The cluster already exists.'
}
else {
    docker exec pve-1 pvecm create cvtest --link0 10.0.99.1
    if ($LASTEXITCODE -ne 0) { throw 'pvecm create failed' }
    Start-Sleep -Seconds 3
}

$fingerprint = docker exec pve-1 sh -c 'openssl x509 -in /etc/pve/local/pve-ssl.pem -noout -fingerprint -sha256 | cut -d= -f2'

foreach ($node in @(@{ Name = 'pve-2'; Address = '10.0.99.2' }, @{ Name = 'pve-3'; Address = '10.0.99.3' })) {
    docker exec $node.Name sh -c 'test -f /etc/pve/corosync.conf'
    if ($LASTEXITCODE -eq 0) {
        Write-Information "$($node.Name) is already in the cluster."
        continue
    }

    docker exec $node.Name pvesh create /cluster/config/join --hostname 10.0.99.1 --password $password --fingerprint $fingerprint --link0 $node.Address
    if ($LASTEXITCODE -ne 0) { throw "$($node.Name) could not join the cluster" }
    Start-Sleep -Seconds 4
}

docker exec pve-1 sh -c 'pvecm status | grep -E "^Name:|^Nodes:|^Quorate:"'
