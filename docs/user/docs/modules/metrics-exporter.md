# :material-chart-line: Metrics Exporter <span class="scope" data-scope="per-cluster"></span>

Exposes Proxmox VE metrics over a Prometheus-compatible HTTP endpoint, ready to be scraped and visualised in Grafana or any compatible monitoring stack. Each enabled cluster gets its own scrape endpoint; collectors are opt-in, individually cached and parallelised.

## Features

<div class="grid cards" markdown>

-   :material-toggle-switch:{ .lg .middle } **Two-level Enable**

    ---

    Master **Enabled** switch for the module, plus an independent **Prometheus Enabled** toggle inside the Prometheus tab. Either disabled returns `503` to scrapes.

-   :material-key-variant:{ .lg .middle } **Per-cluster Token**

    ---

    Each cluster has its own token, stored encrypted at rest and validated with `CryptographicOperations.FixedTimeEquals` at scrape time. Rotate it any time from the settings page.

-   :material-tune-variant:{ .lg .middle } **Presets**

    ---

    Three one-click profiles: **Fast** (essentials only — low API impact), **Standard** (sensible default), **Full** (everything on). Each preset can still be tweaked collector by collector.

-   :material-bolt:{ .lg .middle } **Parallel Collection**

    ---

    `MaxParallelRequests` (default 5) controls how many per-node/per-guest fetches run concurrently. Lower it on small or slow clusters; raise it for big clusters with fast APIs.

-   :material-cached:{ .lg .middle } **Per-collector Cache**

    ---

    Every collector has its own `CacheSeconds` TTL. Slow collectors like S.M.A.R.T. or Subscription can be cached for minutes while keeping fast ones (Node Status) fresh.

-   :material-monitor-dashboard:{ .lg .middle } **API Instrumentation**

    ---

    Optional — exports counters and histograms about the cv4pve-admin → PVE API calls themselves (latency, errors). Handy to spot a misbehaving cluster.

</div>

!!! tip "Hot reload"
    Saving the settings clears the cached engine for that cluster — the next scrape rebuilds it with the new configuration. No service restart needed.

## Why

Why this exporter when PVE already has metrics?

<div class="why-grid" markdown>

<div markdown>
!!! tip "Cluster-wide in one scrape"
    A single endpoint covers cluster, every node, every guest. Prometheus doesn't need to talk to each PVE node individually.
</div>

<div markdown>
!!! success "Opt-in collectors, no surprises"
    Disk SMART, replication, HA, balloon memory — each is a switch with its own cache TTL. You pay for what you actually scrape.
</div>

<div markdown>
!!! info "Auth without certificates"
    Each cluster has a per-cluster token validated in constant time. No PVE API token in your Prometheus config, no certificate rotation pain.
</div>

<div markdown>
!!! warning "Drop-in for Grafana"
    Standard Prometheus text format with `# HELP` and `# TYPE` lines on every metric — your existing dashboards keep working.
</div>

</div>

## Sections

- **Status** — health check for the current cluster: the scrape URL (clickable), how many requests have been served, and when the last one happened

## Endpoint

```text
GET /module/*/metrics-exporter/prometheus/<clusterName>?token=<TOKEN>
```

- **`<clusterName>`** — name of the configured Proxmox VE cluster
- **`<TOKEN>`** — per-cluster token defined in the module settings

A `503` response means the module or the Prometheus exporter is disabled; `401` means the token is wrong; `400` means the cluster name is unknown or disabled.

!!! tip "Grab the URL from the UI"
    The fully-qualified scrape URL for the current cluster is shown (clickable) in the **Status** tab — copy it from there instead of building it by hand.

## Collectors

Grouped per scope; each collector is independently toggleable and has its own cache TTL.

### Cluster

| Collector | What it exposes |
|-----------|------------------|
| **High Availability** | HA state of cluster manager, groups and resources |
| **Backup Info** | Configured backup jobs and per-VM backup coverage |

### Node

| Collector | What it exposes |
|-----------|------------------|
| **Status** | Memory, swap, load average, uptime |
| **Subscription** | License status and tier per node (Community / Basic / Standard / Premium / None) |
| **Replication** | Per-VM replication jobs, sync status and lag |
| **Disk S.M.A.R.T.** | S.M.A.R.T. attributes per physical disk (one API call per disk per node — keep cached) |

### Guest

| Collector | What it exposes |
|-----------|------------------|
| **QEMU Balloon Memory** | Real used memory inside the guest (only on QEMU VMs with the balloon driver enabled) |

## Connecting Prometheus

Minimal `prometheus.yml` scrape job:

```yaml
scrape_configs:
  - job_name: cv4pve-admin
    metrics_path: "/module/*/metrics-exporter/prometheus/my-cluster"
    params:
      token: ['REPLACE_WITH_YOUR_TOKEN']
    static_configs:
      - targets: ['cv4pve-admin.example.com']
```

!!! note "Quote the metrics_path"
    Quote `metrics_path` because the literal `*` in the URL is a wildcard for the "all clusters" routing context and must be preserved as-is — without quotes some YAML parsers treat it as a special character.

If you have multiple clusters, add one scrape job per cluster (or a single job with `__metrics_path__` relabeling from a static list).

## Metric Naming

All metrics are prefixed with `cv4pve_` and grouped by scope. Examples:

- `cv4pve_cluster_quorate`, `cv4pve_cluster_nodes`
- `cv4pve_node_cpu_assigned_cores`, `cv4pve_node_memory_total_bytes`, `cv4pve_node_disk_health`, `cv4pve_node_disk_wearout`
- `cv4pve_guest_cpu_usage_ratio`, `cv4pve_guest_memory_usage_bytes`, `cv4pve_guest_disk_size_bytes`, `cv4pve_guest_uptime_seconds`, `cv4pve_guest_network_receive_bytes_total`
- `cv4pve_ha_state`, `cv4pve_ha_node_state`, `cv4pve_ha_quorate`
- `cv4pve_guests_not_backed_up`
- `cv4pve_api_request_duration_seconds`, `cv4pve_api_request_errors_total` (when **API instrumentation** is on)

For the full, always up-to-date catalogue scrape the endpoint above — every metric carries its own `# HELP` and `# TYPE` line in the response.

## Settings

The default preset on a new module is **Fast**. Each collector has its own `Enabled` switch and `CacheSeconds` TTL (0 = no cache).

??? note settings "Show all settings"

    **Module**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | off | Master on/off switch for the module on the cluster |
    | **Token** | empty (required) | Per-cluster token (stored encrypted, validated with constant-time compare) |

    **Prometheus (Fast preset)**

    | Setting | Default | Purpose |
    |---------|---------|---------|
    | **Enabled** | on | Enable the Prometheus scrape endpoint |
    | **Max Parallel Requests** | 5 | Max concurrent per-node / per-guest fetches (1 = sequential) |
    | **API Instrumentation** | off in Fast (on in Standard / Full) | Export counters and histograms about the underlying PVE API calls (latency, errors) |

    **Cluster collectors**

    | Collector | Default in Fast | Default in Standard | Default in Full |
    |-----------|-----------------|---------------------|-----------------|
    | **High Availability** (`Ha`) | on, cache 0s | on, cache 0s | on, cache 30s |
    | **Backup Info** | on, cache 0s | on, cache 600s | on, cache 600s |

    **Node collectors**

    | Collector | Default in Fast | Default in Standard | Default in Full |
    |-----------|-----------------|---------------------|-----------------|
    | **Status** (memory, swap, load, uptime) | off | on, cache 0s | on, cache 0s |
    | **Subscription** | off | on, cache 3600s | on, cache 3600s |
    | **Replication** | off | on, cache 0s | on, cache 60s |
    | **Disk S.M.A.R.T.** | off | off | on, cache 600s |

    **Guest collectors**

    | Collector | Default in Fast | Default in Standard | Default in Full |
    |-----------|-----------------|---------------------|-----------------|
    | **QEMU Balloon Memory** | off | off | on, cache 0s |
