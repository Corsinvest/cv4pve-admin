# Changelog

All notable changes to cv4pve-admin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Community Edition

#### Added

- **Public address**: the new `PublicUrl` setting is the address the users open. The links of the account emails (password reset, email confirmation) and, in *(EE)*, the callback address shown for the identity providers are built from it, not from the address of the request. Without it they follow the request, as before, and a warning at startup says so.

- **Trusted proxies**: the new `TrustedProxies` setting lists the reverse proxies (addresses or networks) whose forwarded headers are accepted: the client address in the audit log and the per-address limits can no longer be written by the client itself. Without it the headers are accepted from any address, as before, so an installation behind a proxy keeps working unchanged.

- **Limit on sign-in attempts**: one address can try to sign in, or to enter the two-factor code, 10 times a minute (`Security:MaxAttemptsPerMinute`, `0` turns it off). It protects also the domain sign-in, which has no local lockout. The AI Server counts its failed authentications the same way: past the limit it answers *Too many requests* and writes nothing in the audit log.

- **Metrics Exporter, token in a header**: Prometheus can send the token in the `Authorization: Bearer` header, so that it does not end up in the logs of a proxy. The token in the address keeps working.

- **Resend email confirmation**: a user whose confirmation link has expired asks for a new one from the login page, without an administrator. The page answers the same way whether the account exists or not, and limits the requests.

- **adminctl update**: pulls the latest images and recreates the services that changed; `self-update` now suggests it as the next step.

- **Cluster settings, SSH port**: each node of a cluster has its own SSH port (default 22), next to the API port, for nodes where SSH does not listen on the standard port. Existing clusters keep port 22.

- **Check for updates**: a button in the Help menu looks for a new version right away, instead of waiting for the periodic check (up to 12 hours).

- **Diagnostic — profiles**: *Fast*, *Standard* and *Full* buttons in the settings, as in Report. *Fast* skips the slowest reads (backup content, snapshots, LVM-thin metadata) for a quick scan of large clusters; *Full* turns on every optional check (S.M.A.R.T., ZFS details, CVE lookup, OK results) for audits.

- **Diagnostic — IOWait threshold**: a node spending too much CPU time waiting for disks is now reported, with its own threshold (Warning 10%, Critical 25%). Before, the check used the CPU thresholds and never fired.

- **Diagnostic — new network checks**: a guest using a VLAN its bridge does not let out of the node, and a guest using a bridge missing on another node (migration or HA would fail).

- **Diagnostic — PDF cover page**: the PDF report opens with a cover page, as the Excel file does: report information and a table of contents with the page number of each section, clickable to jump there. The sections also appear in the bookmarks panel of the PDF viewer.

- **Excel export of the lists**: an export button in the toolbar of the lists (resources, networks, disks, snapshots, cluster and node views, tasks, AutoSnap, backups, replications and, in *(EE)*, users, roles, tokens, audit log, system logs, UPS readings and VM performance) saves them as an Excel file. Every row matching the current filter and sort is exported, not only the ones on screen, and dates are written in local time.

- **Report, Capacity Planning**: a new section with one row per guest, node and storage: what is allocated, what is used on average and at the peak, how fast each storage fills up and the days left before it is full. Nodes also show the vCPUs and memory assigned to the running guests and their ratio to the node. It is built on the RRD data, so each table needs the RRD data of its section. Off by default, on with the *Full* preset.

- **WebHook — custom headers**: HTTP headers can be added to a webhook notification (and to AutoSnap hooks *(EE)*), with placeholders in their values. The `%severity%` placeholder is now listed in the editor.

- **Enterprise features shown in the Community Edition**: the Enterprise widgets (Daily Status, Git Size) are listed in the **Add widget** menu of the dashboard, marked with a diamond, and appear in the module overviews with the subscription notice; the Notifications page has an Apprise tab with the notice; a new **Enterprise Modules** page in the Admin Area lists the modules of the Enterprise Edition. Before, these features did not appear at all.

- **Command line, `user disable-2fa`**: turns off two-factor authentication for a user who lost both the authenticator device and the recovery codes. It is the way out when that user is the only administrator.

#### Changed

- **Metrics Exporter (review your alerts)**: an unknown or disabled cluster and a cluster without a token now answer *Unauthorized* (401), like a wrong token, instead of *Bad request* (400): the answer no longer tells which clusters exist. *Disabled* (503) is returned only with the right token.

- **Permission editor, Read of the page data removed**: for the data of a page (the jobs, status and results of AutoSnap, the backups of Backup Analytics, the replications of Replication Analytics, the scans and ignored issues of Diagnostics, the scans of Update Manager, the backups of Node Protect, the reports of System Report, the UPS devices) the editor offered a *Read* permission that was never checked: opening the page is what shows them. It is no longer offered; roles that already have it keep it, without effect.

- **Resources, permissions per resource on every page (review your roles)**: the Disks, Partitions, Snapshots and Networks pages listed every VM and node of the cluster, unlike Guests, Nodes and Storages. Now they list only the readable ones, and the network diagram draws only readable nodes, guests and storages. The SDN tab needs Read on every VM, node and storage of the cluster.

- **Node Protect, permissions per node (review your roles)**: the module showed the backups of every node to whoever could open its pages. Now the folder backups list only the nodes the user can read; the backup of all the nodes, the Git repository of the Enterprise Edition and the widgets need Read on every VM, node and storage of the cluster.

- **Update Manager, permissions per VM (review your roles)**: the module showed the update status of every guest of the cluster to whoever could open its pages. Now it lists only the guests the user can read; the scan, the report and the widget need Read on every VM, node and storage of the cluster.

- **Diagnostics, permissions per resource (review your roles)**: the module showed the findings of every node, guest and storage to whoever could open its pages. Now the details of a scan, and the compliance view of the Enterprise Edition, list only the findings of the resources the user can read. The counts of the scans, the report, the scan, deleting a scan, ignored issues and the widgets need Read on every VM, node and storage of the cluster.

- **Replication Analytics, permissions per VM (review your roles)**: the module showed the replications of every VM of the cluster to whoever could open its pages. Now the replications, the trends and the scheduled replication jobs list only the VMs the user can read (the trends also only readable nodes). The scan, deleting all data and the widgets of the module need Read on every VM, node and storage of the cluster.

- **Backup Analytics, permissions per VM (review your roles)**: the module showed the backups of every VM of the cluster to whoever could open its pages. Now the backups, the backups in line, the unprotected disks and the trends list only the VMs (and storages) the user can read, and a scheduled backup job only when all its guests are readable. The scan, deleting all data, the log of a whole task and the widgets of the module need Read on every VM, node and storage of the cluster.

- **AutoSnap, permissions per VM (review your roles)**: the module showed and acted on every VM of the cluster to whoever could open its pages. Now *Status* lists only the snapshots of the VMs the user can read, a job is listed only when all of its VMs are readable, and *Snap*, *Purge Snapshots*, deleting and saving a job, and deleting snapshots from *Status*, need the snapshot permission on every VM involved. A job on @all, a pool, a tag or a node, and the widgets of the module, need Read on every VM, node and storage of the cluster. The picker of the VMs offers only the readable ones.

- **Language of the profile**: the language chosen in the profile is applied to the interface and to the account emails sent to the user; before, both followed the language of the server.

- **Deleting a cluster** also deletes the settings of the modules and the data they collected for it (scans, results, history, UPS devices, Portal tenants, tasks and permissions limited to it). The confirmation says so.

- **Maintenance, Fix All**: one confirmation that lists the steps, instead of one question per step.

- **Settings labels**: "Enabled schedulation" is now "Enable scheduling", "Number which should will keep" is now "Number to keep", and the AutoSnap notify option "Allways" is shown as "Always". Existing settings are kept.

- **Help menu**: the version, edition and update check share a single line at the top.

- **Date ranges**: the trends of Backup Analytics, Replication Analytics and UPS *(EE)*, and the syslog and firewall log filters of Report, pick the period in a single field instead of two separate dates. In the Backup and Replication trends the last day of the period is now included.

- **Cluster settings — nodes**: the node list is edited directly, with no edit/save step per row. The order, which sets the node tried first for the API, is changed with up/down buttons instead of drag and drop, and a note explains that the same addresses are used for SSH.

- **Diagnostic**: resource links in the results (and in the Compliance view *(EE)*) open in a new tab, so the report stays open.

- **Container IP addresses**: running containers show the address they have right now, also when it comes from DHCP (`10.0.0.5/24 (dhcp)` instead of just `dhcp`), in the Resources network list, in the network diagram and in the IP search. Stopped containers and older Proxmox VE releases keep showing the configured value.

- **Resources network list**: new *Trunks* column for guest NICs.

- **Report**:
  - Containers show the IP they actually have, like in Resources.
  - VM trunk ports appear in the network sheet and in the diagram; Cluster Access reports the disabled two-factor entries.
  - Rows always come in the same order, with numbers sorted as numbers (`105` before `1000`), so two reports of an unchanged cluster differ only in live values.

- **WebHook — placeholders**: values are escaped for where they are placed: URL-encoded in the address, JSON- or XML-escaped in the body, line breaks removed in headers. A message with quotes or line breaks no longer breaks an XML payload, and a placeholder in the address is no longer JSON-escaped. A value that itself contains a placeholder name is no longer replaced a second time.

- **WebHook — errors**: when the receiver refuses the call, the error shows its answer (the first 500 characters) along with the HTTP status. The log shows only the receiver's host, since Slack, Discord and Teams put the secret in the address.

- **WebHook — GET requests**: the body fields are hidden, since a GET request is sent without a body.

- **AutoSnap (review your jobs)**:
  - A job that selects no VM/CT, or whose guests are on an offline node, now ends as failed and the guests are listed in the log. Before, it ended OK without taking any snapshot. With *On Failure Only* a notification is sent.
  - *Max percentage storage* is now applied: guests on a storage used above the limit (95% by default) are skipped. Before, the limit was never applied. The lowest value is now 1%.
  - Guest selection: `text%` matches the names starting with the text and `%text` the names ending with it; they were inverted. An excluded range (`-200:299`) no longer adds guests, and pools combine with exclusions (`@all,-@pool-test`).
  - Containers with a bind mount or a device mount point, and VMs with a physical disk, are skipped with a message: Proxmox VE cannot snapshot them, and the job failed on them every time.
  - The timestamp format (settings) and the label (job) are checked when saved: a snapshot name longer than 40 characters, or a format that cannot be sorted in time order, is refused with the reason. Before, the job failed when it ran.
  - Selecting guests by pool on Proxmox VE 9 needs `Pool.Audit` instead of `Pool.Allocate`.

- **Metrics Exporter (review your alerts)**: `cv4pve_replication_failed_total` is replaced by `cv4pve_replication_fail_count`. Proxmox VE reports the failed attempts in a row, back to 0 at the first success, so the value now goes down when replication recovers. Alert on `cv4pve_replication_fail_count > 0`.

- **Diagnostic, codes**: the first letter of a code is now always the severity of the finding. Checks with a warning and a critical threshold report the critical level with its own `C` code, with the same number as the `W` code (`WN0027` and `CN0027`). Some codes changed number, and a problem that was reported under two codes is now reported once. Older scans keep the codes they were saved with.

- **Report**:
  - VMs and Containers have new columns with the total size of the disks, the unused disks and, for VMs with the guest agent, the guest partitions.
  - JSON output: the size and the percentage of the same measure no longer share a key (`memoryUsage` is the percentage, `memoryUsageBytes` the size; the same for disk usage), and four misspelled keys are renamed (`smartData`, `ip`, `position`, `uris`).

- **Node Protect**:
  - On Linux the backup archives are readable only by the user running the application, since they contain password hashes and keys.
  - When the backup of a node fails, the old backups are kept: retention is applied only after a run where every node was backed up, so good backups are never deleted to make room for an incomplete one. A run that saved no archive leaves no empty folder.

- **Permissions (review your custom roles)**: the actions of these pages now have their own permission, which hides the button and is checked again when the action runs. Before, opening the page was enough to run them.
  - Backup Analytics and Replication Analytics: *Scan*, *Delete all data* and the export.
  - Diagnostic: *Scan*, deleting and downloading a scan, creating, editing and deleting the ignore rules.
  - Node Protect: *Backup*, delete and download; in *(EE)* also the Git *Push*, *Reset*, *Sync from remote* and download.
  - System Report: new report, delete and download.
  - Update Manager: *Scan* and download.
  - Bots: sending a message.

  The built-in roles of each module include the new permissions. Custom roles do not: their users no longer see these buttons until an administrator assigns the permissions.

- **Account emails**: the emails for the password reset and the email confirmation have a new layout, and the expiry they state is the real one.

- **Remembered browser at the two-factor login** *(EE)*: it is trusted for the days set in `CookieSettings:ExpireDays` (30 by default), like the sign-in itself. Before it was 14 days, while the profile page said 30.

- **Backup Analytics and Replication Analytics, Check widget**: a new widget starts from 30 days, as many as the data kept by default, instead of 60.

#### Fixed

- **Security of the pages**: the application can no longer be shown inside a frame of another site (clickjacking), the browser no longer guesses the type of a file, and the full address of a page is no longer sent to other sites.

- **Sign-in, return address**: a crafted link could send the user to another site after the sign-in.

- **Web console**: the address of the console files could be bent to reach other Proxmox VE requests with the credentials of the application; only the console files are served now.

- **Closed sessions**: a session closed by the application, for example after a password reset or from the command line, stayed valid for up to two minutes.

- **Behind a reverse proxy**: the redirect to the sign-in page used the internal address of the application instead of the public one.

- **Error details**: the details of an error are no longer sent to the browser in production.

- **Sign-in of a disabled account**: with a wrong password the answer is the same as for an account that does not exist; before, it told that the account exists.

- **Backup, file download**: a folder downloaded as tar arrived as a zip file with a `.tar.zst` name.

- **Diagnostic, scan detail**: the findings are grouped worst first (Critical, Warning, Info); the order of the groups was ignored.

- **Trends pages** (Backup Analytics, Replication Analytics and, in *(EE)*, UPS Monitor): choosing a new period keeps the storage or node and the VMs already selected; before, the first click of the range dropped them and the charts stayed empty.

- **Grids**: a group, once expanded, can be collapsed again.

- **Help menu**: the red dot on the Help icon is back when a new version is available, and the separators between menu sections are drawn as lines again. The Help and notification buttons in the header are as narrow as the other icon buttons, with the counter moved to the corner so it no longer covers the bell.

- **Diagnostic — settings**: the node RRD time frame and PSI pressure thresholds changed from the settings page are now applied; before, the scan kept using the defaults. The Storage section no longer shows RRD and PSI fields, which the storage check never used.

- **Diagnostic — fewer false alarms and more accurate checks**:
  - The scan no longer stops halfway when a storage, node or guest does not answer: the missing part is reported and the rest of the scan is completed. When backups or other lists cannot be read, the checks depending on them are skipped instead of reporting "no backup" or "nothing configured".
  - Removed or corrected checks that fired by mistake: VLAN tag on a non VLAN-aware bridge (removed), unused network ports down, start on boot / locked / protection on containers, protected backups counted as old, container bind mounts, ZFS hot spares, disks behind RAID controllers, services not installed, CVEs already fixed in the installed version, and more.
  - Checks that could never fire now work: API tokens, quorum loss on node failure, failing replication jobs, pending kernel reboot, disabled storages still in use, empty pools.
  - Orphaned disks and backups now include containers and are reported once per deleted guest, and are skipped when the account cannot see every guest.
  - Numbers in the messages no longer depend on the server's regional settings (`80.9%` everywhere).
  - Compliance references checked against the official texts (DORA, NIS2, ENS, BSI C5).

- **Diagnostic — ignore rules to review**: the check for a VLAN tag on a non VLAN-aware bridge has been removed, and the *nesting without keyctl* warning is now an Info finding under a new code: ignore rules written for them no longer match anything. Rules matching numbers with a decimal comma (`80,9%`) must be rewritten with a dot.

- **Network diagram**: a guest on several bridges is linked to all of them, guests are listed by numeric id, Open vSwitch bridges are wired to their physical ports, and storages on older network configurations get their link to the bridge.

- **Guest details**: NVMe disks show their wearout like SSDs; the guest agent is recognised also when enabled with the `enabled=1` form; VM trunk ports are shown; options left at their Proxmox VE default (memory, swap, CPU type, OS type, …) show that default instead of an empty or zero value.

- **WebHook — stored secrets**: passwords, tokens and API keys of webhooks are stored encrypted, like the other credentials. Values saved before are kept and encrypted on the next save.

- **Metrics Exporter**: metrics of objects that no longer exist (a deleted or renamed guest, a removed storage, an offline node) are no longer exported forever; guest disk and network counters restart after the guest restarts or migrates; HA node states are reported; SAS disks with a healthy S.M.A.R.T. status are no longer exported as unhealthy.

- **Bots (Telegram)**: VM, container and node commands answer with the error of Proxmox VE when the action is refused, instead of answering as if it had worked. The built-in migrate and node shutdown aliases now work.

- **AutoSnap, deleting a job**: with *On remove job remove snapshots in Proxmox VE* enabled, deleting a job now removes its snapshots and shows the operation among the tasks; before, the snapshots were left in place. The job stays in the list until the removal ends.

- **Dashboard**: cloning a dashboard creates a copy instead of overwriting the original; an imported dashboard belongs to the user who imports it, also when the file was exported by someone else; creating the default dashboards no longer renames the Home and Status dashboards of the other users.

- **Backup Analytics**: the scan imports every backup task of the retention period; before, only the last 50 of each node were read, so the first scan and busy nodes missed the older ones.

- **Replication Analytics**: the scan no longer fails on a cluster without replication jobs, and the old results expire after the retention period also when the replications have stopped. A replication that fails is now stored in the run history, with its error: before, the history kept showing the last successful run and the failure never appeared. A job that keeps failing gives one record for each try a scan finds.

- **Sign-in sessions**: a sign-in now survives a restart or an update of the application: before, every restart signed everyone out, also with *Remember me*. When a password is changed or reset, the other browsers signed in with that account are signed out at once instead of within 30 minutes. After the update to this version everyone signs in once more.

- **Forgot password**: the reset link is sent at most 3 times to the same address in 15 minutes, and 10 requests are accepted from the same computer. Before, anyone who knew an address could fill its mailbox with reset messages.

- **Audit and privileges**: deleting a backup from the *Backups* tab of a guest and *Schedule Now* on a replication are now written to the audit log, with their outcome. Deleting storage content, which the AI Server tools do, also checks the privilege on Proxmox VE before it runs.

- **Backups of a guest**: *New* in the *Backups* tab of a guest now starts a backup, with storage, mode, compression, protection and notes; before, it only showed "Not available". Changing the notes or the protection of a backup is now written to the audit log. *Restore*, which did nothing, is no longer shown.

- **Backup Analytics, Unprotected Guests**: the page was empty when no backup job was enabled, which is when every guest is uncovered. It also ignored the jobs that select a pool and the guests excluded by a job for all guests. It now evaluates every kind of job: all guests with exclusions, pool, list of IDs, and the node a job is bound to.

- **Check widgets**: the *Check* widget of Backup Analytics, Replication Analytics and Diagnostics showed "Module not configured!" for a cluster with the schedule off, also when scans had been made by hand. It now shows those results too.

- **Diagnostics, notification**: the result of a scan is now notified also when both report attachments are off; before, nothing was sent. The message carries the number of critical, warning and info issues. Who has a notifier configured and both attachments off starts to receive it: to receive nothing, leave the list of the notifiers empty.

- **Diagnostics, Community Edition**: the notice in the PDF and Excel reports that names the compliance standards of the Enterprise Edition listed 14 of them. It now lists all 18.

- **Clusters**: the scheduled jobs of a cluster that was deleted or disabled kept running, and failed at every run. They are now removed when the cluster is deleted or disabled, and at every start of the application. A cluster that is enabled again gets its scheduled jobs back at once, with the schedule and the options they had. The data collected for the cluster stays in the database.

- **Add cluster**: with no cluster configured, every user was shown *Add cluster* and taken to the page that creates one, also without the permission to open it. Button and redirect are now only for who has it.

- **Command Palette**: moving through the results with the arrow keys, the selected entry went out of view past the last visible result. The list now follows the selection.

- **Grids, keyboard**: the edit key (F2) opened the editor of the selected row only when the last click of the mouse had been on the name column. It now always opens it.

- **Copy buttons**: after a copy the button shows a green check mark for two seconds, in place of a notification.

- **Installer**: `install.sh` changed, or refused, a database password that contains quotes or a backslash, and reduced two spaces to one. The password is now kept as typed.

- **AutoSnap, module switch**: the *Enabled* setting of the module now stops the scheduled jobs of the cluster; before, the jobs ran whatever its value. A job is scheduled when the module and the job are both on. What is started by hand stays available with the module off. At the first start with this version the setting is turned on, once, for the clusters that have enabled jobs, so that no scheduled snapshot stops with the update.

- **Texts**: the help of the AutoSnap *Search Mode* said that *All* lists the snapshots made by hand; it lists only the snapshots made by AutoSnap, on every guest of the cluster. The permission shown as "Snapshot Rallback" is now "Snapshot Rollback".

- **AutoSnap, startup**: a job whose additional data is missing or cannot be read stopped the start of the whole application. A job with no additional data is now read as such; one that cannot be read is skipped, with an error in the log that names it, and the other jobs are scheduled.

- **Bots, module switch**: the *Enabled* setting of the module now stops the bot of the cluster; before, it had no effect and the bot followed *Telegram → Enabled* alone. The bot runs when both are on. At the first start with this version the setting is turned on, once, for the clusters where the Telegram bot is on.

- **AI Server, permissions (review your tokens)**: five tools returned more than the token is allowed to see. The list of the clusters now shows only the clusters the token can work on; tasks, replication jobs and backup jobs are shown only for the guests and the nodes the token can see, and a backup job lists only the guests the token can see. A token with limited cluster permissions sees less than before.

- **Diagnostic, ignored issues**: an ignore rule hides only the finding it was made for: same resource, same check, same severity. Before, a rule for VM 10 also hid VM 100, and a rule made from an Info finding also hid the Warning and Critical ones with the same text. The check code is shown in the list of the rules. To make a rule wider, clear its fields: gravity and context can be left empty to match any value. Rules created before keep working; some findings hidden by mistake may become visible again.

- **Sign-in**: after the login you are sent back only to a page of this installation; before, a crafted link could send you to another site.

- **Permissions**: the settings of a module can be opened and saved only with the *Edit Settings* permission, also from the *Activate schedulation* button; AutoSnap *Snap* and *Purge*, stopping a task and starting the application update now check their permission when they run, not only when the button is shown. The AutoSnap *Errors* page shows the errors of the current cluster only.

- **Resources, Memory Cleanup and Unlock**: the two entries of the node and guest menu are shown and run only with the *Power Management* permission on that node or guest, and each run is recorded in the audit log. Before, any user who could see the resource could run them. *Schedule Now* of a replication checks its permission also when it runs.

- **Profile, language**: changing the language reloads the page in the new language; before, the page lost the connection.

- **Tasks**: Maintenance *Fix All* and the command line no longer mark as abandoned the tasks that are still running.

- **Widgets**: in the *Status* widget of Diagnostic and of Update Manager the *Critical* and *In error* slices are red again; before, they had no colour of their own. The thin separators of the resource cards, of the *Info* widgets and of the issue list of the health widgets are now drawn.

- **Small fixes**: in the Cluster Usage grid the memory tooltip shows the memory figure (it showed the storage one); the disk usage tooltip of the resource lists ends with `%`; a disabled Metrics Exporter always answers with status 503; the default sender address of a new installation is `noreply@example.com`.

- **Command Palette**: the `vm:` and `ip:` filters list containers too, *create snapshot* offers every guest and not only the running ones, and the guest picker of the commands shows only the guests you are allowed to see.

- **Cluster selector**: the search ignores upper and lower case, and a search without results shows *No results found!* instead of hiding the selector.

- **Add cluster**: the button in the top bar is back when no cluster is configured, also on the pages that do not redirect to the cluster settings.

- **Maintenance**: in the Community Edition *Cleanup System Logs* is shown as an Enterprise feature; before, it ran and reported zero deleted records.

- **System Report**: the keyboard shortcut for a new item opens the new report dialog.

- **Bots, Chats Id**: the field takes one chat ID per line; before, it was a single-line box and only one ID could be entered. A value already saved keeps working.

- **Metrics Exporter**: scrapes arriving at the same time for the same cluster no longer interfere with each other.

- **Installer**: a PostgreSQL password containing `/` or `&` no longer stops the Linux and macOS installer, and one containing `$` is no longer saved altered by the Windows installer.

- **adminctl**: a value with spaces in `.env` is read correctly; after `self-update` the suggested command is the one that applies a changed compose file; the help shows the documentation address.

- **Node Protect**: the paths to back up are read one per line whatever line ending the setting was saved with.

- **AI Server**: the descriptions the assistant reads for eight tools now match what the tools return and accept.

- **AI Server, optional parameters**: the filters and options that the tools describe as optional, such as the status, the type, the detail level and the number of tasks, had to be sent on every call, and a call without them failed. They can now be left out and take the value their description states.

- **Dashboard, permissions (review your roles)**: the Dashboard showed every widget of every module, on every cluster, to whoever could open it. A widget is now shown for the clusters where the user holds the permission of the widget or can open its module, and reads only those clusters. The *Add widget* menu and the *Clusters* picker list only what the user can see. In a dashboard that already exists, a widget the user cannot see stays in its place with a notice and can be removed. The default dashboards are created with only the widgets the user can see.

- **adminctl, database backup**: `backup db create` and `backup all create` report a failure when the database dump fails, instead of saying the backup was created and leaving an empty file.

- **Forgot password**: the reset link opens a page that asks only for the new password. A link that is incomplete, altered, expired or already used shows a page that offers to request a new one, instead of an error. Setting the password from the link also confirms the email address: before, a user with an unconfirmed address could set the password and still not sign in.

- **Two-factor login** *(EE)*: a recovery code can be used at the login; before, the link led to a page that did not exist. A wrong code can be typed again without starting over from the password, and the language of the user is applied.

- **Remember me**: a user who did not come back for more than 14 days was signed out, whatever the number of days in the settings. The session now lasts as long as configured.

- **Password change**: changing the password, or the two-factor setup *(EE)*, no longer signs you out a few minutes later.

- **Email confirmation**: the link shows its result also when you are not signed in; before, it sent you to the login page. An altered link shows a message instead of an error.

- **Scheduled jobs**: the *system* account is put back in place at startup when its name had been changed by the user editor.

- **Account emails translated**: the emails sent to users (email confirmation, password reset, two-factor authentication turned off) now use the language of the interface instead of always English.

- **Backup and Replication Analytics, times (review your data)**: the times read from the Proxmox VE logs were taken in the time zone of the server running cv4pve-admin instead of the one of the node, so with the nodes in another time zone every backup and replication was stored hours off; the start and end of backup tasks were shifted too. Records already stored stay shifted until the data of the cluster is deleted and scanned again.

- **Replication Analytics**:
  - The *not synced for 7+ days* and *24+ hours* alerts and the age bars counted every stored run instead of the last run of each job: the red alert appeared even when every replication had just synced.
  - *Failed* and *with errors* were two alerts counting the same runs: one alert remains.
  - *Trends* reloads the charts when the period or the node changes, not only when the VM changes.
  - A replication that cannot be imported is written in the scan task, which ends as failed, instead of only in the server log.

- **Delete all data** in Backup Analytics and Replication Analytics is now recorded in the audit log.

- **System Report**: a report that failed stayed *waiting* forever; it now ends as failed, with its error, and the failure is recorded in the audit log.

- **Diagnostic**: the Excel and PDF reports list the findings from the most severe, instead of starting from *Info*.

- **AutoSnap, jobs**: the list showed the default schedule (every 15 minutes) and its next run for every job, and an empty *Last Run*; it now shows the real schedule, parallelism and last run. Editing a job was not affected.

- **Updater**:
  - The results of a scan disappeared after 8 hours, while the default schedule runs every 12; they now stay until the next scan.
  - A Windows guest other than Windows 10/11, or a Solaris/Other guest, failed with no message: it now says that its operating system is not supported.
  - A cancelled scan sent the *Scan completed* email and was recorded as successful; it now sends nothing, and the dashboard no longer counts cancelled guests as in error.
  - The *Download* button stays usable after a report that failed.

- **Metrics Exporter**:
  - The token in the *Open metrics* link is encoded: a token with characters such as `+`, `&` or `#` gave an error.
  - When the cluster cannot be reached the endpoint answers *503* with the reason, instead of *200* with old values, so Prometheus sees the target as down.

- **Notifications with attachments**: when a report (for example Diagnostic) was sent to more than one notifier, the second one received an empty or unreadable attachment and failed with no trace; every notifier now gets the full attachment, and a notifier that fails is written in the log.

- **Module menu**: the current page stays highlighted in the menu of a module also when its address carries extra parameters (for example the workflow list).

- **Dashboard widgets**: the widgets that update during a scan could mix two updates or miss the last one; they now always show the final numbers.

#### Internal

- The sign-in session no longer carries the permissions of the roles: they are always read from the database. The session stored for an administrator went from about 20 KB to 3 KB.

- The profile image is scaled down by the browser, and the image library is no longer used: its known vulnerabilities and its license conditions are gone. Any image the browser can read is accepted (also GIF, BMP and AVIF), up to 10 MB.

- Underlying components updated (Proxmox VE API library, Report and Diagnostic libraries, UI components, cache, service discovery, command line). The PDF library stays on its current version: the newer one changed license.

- Updated a .NET cryptography library with two denial of service fixes, and the ASP.NET Core and Entity Framework Core libraries to 10.0.13, the .NET security release.

### Enterprise Edition

#### Added

- **External Authentication**: users sign in with an identity provider (OpenID Connect, Keycloak, Microsoft Entra ID, Google) or with an Active Directory / LDAP domain. Providers are managed in *Admin Area → Security → External Authentication*, stored with encrypted secrets and turned on without a restart; *Test connection* and *Test login* check them before enabling. Each provider can link existing accounts by email (never administrators, accounts with two-factor authentication or built-in accounts), create the account at the first sign-in with default roles, and map its groups to roles, which follow the user when the groups change; the groups of a domain are picked from the directory. The sign-in page shows one icon per provider type, with a list when there are more of the same type, and a *Domain* list before user name and password. A periodic check of each domain removes the roles and closes the sessions of the users who left it or were disabled. In the users list a column shows the linked providers, and the user dialog has two new tabs: the linked logins, which can be linked and unlinked by hand, and the last external sign-ins. Tried with Keycloak, OpenLDAP and an Active Directory without encryption; Microsoft and Google not yet with real accounts.

- **Portal, Tenant Admin**: each tenant can have its own admin, who sees only its tenant and manages the users of that tenant: adds new accounts, chooses their permissions on the tenant's VMs, makes another user admin. Linking an account that already exists stays with the administrator. The tenant roles are assigned by the Portal when a user joins a tenant or becomes its admin.

- **UPS Monitor, thresholds per device**: the editor of a device can set its own battery low, battery critical and runtime critical alert thresholds; an empty field uses the value of the cluster settings.

- **UPS, automatic shutdown of the nodes**: on a power failure the module can shut the nodes of the cluster down. Each device has a new setting, *On power failure*: *Nothing (alerts only)*, the default; *Dry-run (simulate the shutdown)*; *Shutdown (shut the nodes down)*. With Shutdown, when a shutdown condition is met the nodes the device covers get the shutdown command of Proxmox VE, through the API of the cluster and in the order of the settings: nothing is installed on the nodes. A command that fails is tried three times, a node already off is skipped, and the node that hosts the application is commanded last. Choosing Shutdown asks for a confirmation that names the nodes. The credentials of the cluster need the privilege to shut the nodes down: it is checked once a day and the dashboard warns when it is missing. Guests are not stopped one by one, and high availability and Ceph are not handled: the shutdown test warns when the cluster has them. Tried on test nodes with a simulated UPS, not during a real power failure: start with Dry-run. Existing devices stay on *Nothing*.

- **UPS, shutdown simulation**: with *Dry-run* the module says which nodes it would shut down, in which order and why, without shutting anything down. When a shutdown condition is met the nodes are recorded in Tasks and in the audit log and a notification is sent for the power failure, the condition met, the simulated shutdown and the return of the mains. No command is sent to a node.

- **UPS, shutdown conditions and nodes**: new settings, separate from the alert thresholds. A device on battery meets a condition after a time on battery (600 seconds), below a runtime (10 minutes), below a charge (30%) or when the UPS reports battery low or depleted. For each node of the cluster: the order, and with more than one UPS whether all of them or any one must be in condition. The whole cluster can be treated as one, and the node that hosts the application is kept for last: set *Where this application runs*.

- **UPS, Test shutdown and Simulate power failure**: two buttons in the dashboard. *Test shutdown* shows the nodes that would be shut down now, their order, the estimated time and the warnings, and changes nothing. *Simulate power failure* runs the whole sequence as a test, with its entry in Tasks and a notification marked TEST. During a real power failure the dashboard shows a strip with the nodes already recorded and *Close episode*.

- **UPS, new permissions**: *Change shutdown action*, which also allows arming a device, *Test shutdown*, *Simulate power failure* and *Close episode*. The administrator role of the module has them.

- **UPS, SNMP v3**: a UPS can be read with SNMP v3 instead of v2c: a user of the device, authentication (SHA-256, SHA-384, SHA-512, SHA-1, MD5) and, if wanted, encryption (AES-128, AES-192, AES-256). In the device choose the connection *SNMP v3*. The passwords are stored encrypted. Existing devices stay on SNMP v2c.

- **UPS, older network cards**: a new connection *SNMP v1* for cards that answer only v1, and *3DES* and *DES* among the privacy algorithms of SNMP v3, marked as weak in the editor. A device that refuses a request with more values, or an SNMP v1 agent that fails it for one missing value, is read one value at a time, with nothing to set. Tried against simulated agents (SNMP v1 with a missing value, SNMP v3 with DES, a card that refuses requests with more values with an error or with no answer); 3DES and real cards not tried.

- **UPS, NUT connection**: a UPS without a network card, connected by USB or serial cable to another machine, can be read from the NUT server (Network UPS Tools) of that machine. In the device choose the connection *NUT*, the address and port of the server (3493 by default) and the name of the UPS. The connection only reads: it never sends a command to the UPS. Stale data from the server are treated as no reading. No user is needed on the server.

- **UPS, bypass**: a UPS in bypass, where the load is fed by mains without the protection of the battery, now has the status *Bypass* and raises a warning. Before, it was shown as *Unknown* and no alert was sent. The dashboard shows a *Bypass* counter when a device is in it. A bypass does not start a shutdown.

- **Compliance — four new standards**, for a total of 18: ACN (Italian NIS2 measures), NIS2 Implementing Regulation (EU) 2024/2690, ISO 22301 (business continuity) and BSI IT-Grundschutz (Germany). They appear in the Compliance view and in the PDF and Excel reports.

- **AutoSnap — hook test**: a *Test* button in the hook editor sends the request right away, with sample values for the selected phase, and shows the answer.

- **Users, two-factor authentication**: a *2FA* column shows who uses it, and *Disable 2FA* turns it off for a user who lost both the authenticator device and the recovery codes. The user is informed by email. The action has its own permission, included in the built-in role: custom roles need it assigned.

- **Two-factor authentication turned off**: the user receives an email, whoever turned it off.

#### Changed

- **Portal, tenants kept apart**: the Tenants page shows each user only its tenants; creating, editing and deleting tenants and adding or removing their VMs need the new **All tenants** permission. The **Portal Tenant Admin** role no longer gives permissions on all the VMs of the cluster nor the right to create and delete tenants: review your roles. Who had that role becomes admin of the tenants it was already in. A guest now belongs to one tenant only: guests already in two tenants are listed as a warning in the Vms tab.

- **VM Performance, permissions per VM (review your roles)**: the *Status* page listed every running VM of the cluster to whoever could open it. Now it lists only the VMs the user can read.

- **UPS dashboard, disabled devices**: a disabled device is no longer hidden. Its card is grey, says *Disabled* and shows the values of the last reading with its time; it has its own counter and is not counted as online or offline. Before, a cluster with only disabled devices showed "No UPS devices configured".

- **Users, Reset password**: sends the user a link to set a new password and also shows it, so it can be delivered another way. Before, the administrator had to know the current password of the user.

- **UPS, settings (review your settings)**: the settings are in four tabs: *Monitoring*, *Alerts*, *Shutdown* and *Nodes*. The three alert thresholds have new names, because they only send alerts and never shut anything down: *Default Battery Low Threshold* is now *Battery low alert*, *Default Shutdown Battery Threshold* is *Battery critical alert*, *Default Shutdown Time Threshold* is *Runtime critical alert*. Their values are kept.

- **UPS, dashboard on large screens**: three, four or six cards in a row, according to the width of the screen. Each card shows what the device does on a power failure.

- **UPS, continuous reading (review your settings)**: the devices are read continuously instead of on a schedule: every 30 seconds, every 8 seconds while a UPS runs on battery. The *Cron Schedule* setting is gone, replaced by *Reading interval*, *Reading interval on battery* and *Unreachable after (missed readings)*; *Enabled schedulation* is now *Monitoring enabled* and keeps its value. A change of the settings is applied at once. The reading history keeps every reading during a power failure and at every change of status, otherwise one a minute. Run a single instance of the application: two would both send the alerts.

- **UPS, dashboard**: redesigned. Counters of the devices by status, cards ordered by how much attention they need, battery and load with a bar and a small chart of the last three hours, input and output voltage, and a notice saying for how long a device has been on battery or has not answered.

- **UPS, alerts (review your notifications)**: a notification is sent when the alerts of a device change, not at every scan: when the UPS goes on battery, when the charge then drops below a threshold, and so on. Before, the same alert was repeated at every scan, every 5 minutes by default. A *back to normal* notification is sent when no alert is active any more.

- **UPS, device profile**: with *auto* the profile is chosen by asking the device which values it has, not from the text of its description. The profile of the manufacturer is used when the device has its values, otherwise the standard one (RFC 1628). Seven profiles that could not work are removed (Legrand, Salicru, Mitsubishi, Piller, Zigor, NetVision, Borri): a device set to one of them uses the standard profile.

- **UPS, profiles checked against the manufacturers' definitions (review your devices)**: every profile was compared, value by value, with the published definitions of the standard and of each manufacturer, and rewritten where it read the wrong value. Ten profiles are verified and chosen by *auto*: Generic RFC 1628, APC, CyberPower, Eaton / Powerware, NetAgent cards (Mach Power, Ablerex, Tecnoware), Delta, HPE / HP, MGE, Tripp Lite, Riello. Socomec, Huawei, Chloride, GE and Phoenix Contact could not be verified: they work only when picked by hand and the editor says so. Vertiv / Liebert and Gamatronic are removed. A device set to a profile that no longer exists, or renamed (*Eaton / Dell / ABB*, *CyberPower / FSP / PowerWalker*, *Powerware / Exide*), is read with *auto*. Readings and status of existing devices can change: run *Test Connection* on each device.

- **UPS, Test Connection**: the result is shown in a dialog with the profile used and one row for each value: where it is read from and what the device answered, or *Missing*. A warning says which alert thresholds cannot fire because the device gives no charge or no runtime.

- **Workflow engine** updated (Elsa 3.9). In the list of workflows the designer opens from the row menu (*Edit*), no longer by clicking the first cell.

- **Workflow, permissions (review your roles)**: the *Clone*, *Resize*, *Convert to Template* and *Manage HA* activities now check four new permissions (Clone, Resize disk, Convert to template, High Availability), included only in the *Cluster Admin* role, and are recorded in the audit log. Before they acted on Proxmox VE with no check.

- **Workflow, permissions applied (review your roles)**: the permissions of the module were defined but not applied: whoever could open the *Workflows* page could edit, publish, delete and run every workflow. The designer now shows only the actions the user holds on the cluster, and the workflow engine refuses the others. Two new permissions: *Publish and retract* and, in *History*, *Cancel*; the administrator role of the module has them. The pickers of the activities list only the nodes, guests and storages the user can read.

- **Workflow, page address**: the open workflow or run is part of the page address, so the browser *Back* button and the menu link return to the list and the address can be bookmarked. The *Back* buttons are removed, and the heading of the list, which repeated the name of the page, is no longer shown.

#### Fixed

- **AI Server, queries across two tables**: two fields with the same name no longer fail (they come back with the table name in front), and the result can be ordered by a selected field. The tool description now explains how to combine tables.

- **Portal**: a tenant created without a description could not be saved; the editor closed and the tenant was not there.

- **AutoSnap, hooks**: with parallel snapshots, the lines of a hook stay in the log block of their VM/CT instead of mixing with the other guests.

- **Workflow — guest configuration**: for containers, the activity that reads a guest's configuration now returns the real memory, OS type, tags and protection flag, instead of empty or zero values.

- **NodeProtect — Git credentials**: the username and password changed in the Git settings are now saved; before, the change was lost. The password, like the secrets of AutoSnap hooks, is now stored encrypted; values saved before are kept and encrypted on the next save.

- **AI Server, queries**: the filters of a query are now applied; before, they were ignored and every row was returned. A malformed filter or a misspelled property now returns an error instead of being skipped.

- **AI Server, queries**: the query tool now tells the assistant about three more filters it can use: does not contain, does not start with, does not end with.

- **Workflow, guest actions**: the power action, migrate and snapshot (create, delete, rollback, update) activities now run their action; before, they never reached Proxmox VE.

- **Users, Roles and App Tokens**: clicking a row selects it and opens the editor again; the *Roles* tab of an app token no longer drops the connection.

- **App Tokens**: editing a token no longer clears its owner. A new token opens with *Is Active* on, and turning it off before saving is now applied.

- **Audit log**: a failed creation or change of a user or a role is recorded as failed; before, it was recorded as successful.

- **Small fixes**: the photo picker of a user shows only images; the *Docs* link of an AutoSnap hook opens the right page; Node Protect Git shows *Sync from remote*.

- **Apprise**: the addresses are read one per line whatever line ending the setting was saved with.

- **Permissions**: resetting a user password, regenerating an app token, UPS *Scan now* and creating a Portal tenant now check their permission when they run, not only when the button is shown.

- **Users**: the *system* account, used by the scheduled jobs, can no longer be edited from the user list. *Effective Permissions* is shown only to who can read the users. The same button of the app tokens needs the read permission on tokens.

- **Portal**: removing a guest from a tenant no longer removes it from the other tenants. It also removes the permissions the tenant users were given on that guest: before, they kept their access and the Portal no longer showed it. A user who is also in another tenant that has the same guest keeps them. Adding a guest that is already in another tenant now asks for a confirmation that names that tenant.

- **AI Server**: in the Enterprise Edition every request to the endpoint failed with an error, also with a valid key. Requests are now served and written to the audit log with the name of the token. The query tools see only the nodes, guests and storages the token can see.

- **Audit Logs, deletion (review your roles)**: deleting rows of the audit log was allowed by the permission to delete users. It now has a permission of its own, *Delete audit logs*, which administrators hold. A custom role that could delete audit rows only because it could delete users needs the new permission to keep doing it.

- **Node Protect, Git**: *Reset* and *Sync from remote* now run as tasks, with their steps in the task list and their outcome in the audit log; before, a failure left no trace. The confirmation of *Sync from remote* warns that the local repository is removed. On a Windows host *Reset* could also fail because the repository stayed open while the Git page was shown.

- **System Logs**: the log viewer finds the database sink of the logging configuration by its name. Before, it had to be the third of the list, and with another order the viewer could read the wrong table without a notice.

- **AI Server, ISO tools**: the permissions to download and to delete an ISO image could not be given to a token, because they are in no role and the permission editor did not list them. They are now listed with the other tools in the permissions of a token. They stay out of every role: no token has them until they are given.

- **Portal**: *Is Admin* on a tenant user and *Enabled* on a tenant had no effect and are no longer shown; the permissions on each guest can always be set. Adding to a tenant a user whose address belongs to an existing account now asks for a confirmation that shows the roles of that account, and the display name typed is applied to an existing account too.

- **Overviews**: the *Overview* pages of Portal and Workflow described functions the modules do not have (policies, schedules, webhooks, calls to external services). They now say what the modules do.

- **App Tokens**: a role marked as default could not be given to a token. It can now be picked like any other role.

- **Workflow, Wait For Task**: the *Success* output was true whenever the task ended in time, also when the task had failed. It is now true only when the task ended well, and the new *ExitStatus* output carries the outcome. Review the workflows that read *Success* to know only that a task had ended.

- **VM Performance**: the rates per second are now those between the last two readings; before, they were averages since the page was opened, and the totals stayed at the first reading. Two readings less than a second apart no longer end in an error, and a guest that is no longer running leaves the list.

- **Workflow, commands (review your workflows)**: guest power actions, migrations and snapshot activities were refused in silence, because a workflow had no user, and the workflow went on as if they had succeeded. They now run with the permissions of a user: the one who started the workflow or, for a workflow started by a trigger such as *Cron*, the one who last published it. A command that is refused or fails stops the workflow with the reason in its history. The node shutdown and reboot activity and the backup activity now check permissions and write the audit log like the others; the backup runs on the node where the guest is, whatever the *Node* field says.

- **Active sessions**: the channel used to send messages and to force a logout accepts only the connections opened by the application.

- **Profile, two-factor authentication**:
  - *Reset Authenticator* turns two-factor authentication off and starts the setup again. Before, it left it on with no way to configure the app again, and the next login could not be completed.
  - *Disable 2FA* accepts a recovery code, as the page says.
  - A remembered browser is shown, with the days it is remembered for, and *Forget* works.

- **Audit log**: failed logins are recorded; before, none was. Password changes, password resets and every change to the two-factor setup are recorded too.

- **Users**: a built-in account can no longer be deactivated from the editor.

- **UPS**:
  - *Test Connection* said "Connection successful!" also when the device did not answer. It now reports the failure and its reason.
  - A device that does not answer becomes *Offline* and raises an alert after three readings in a row without an answer (the number is in the settings); before, nothing was sent.
  - With *auto* the standard profile was always chosen, whatever the brand.
  - A host name can be used in *Host*; before, only an IP address worked.
  - A wrong profile is reported as such, instead of a reading with all values at zero.
  - An APC device on battery was shown as *Online* with no alert, and one on mains as *Unknown*: each brand now uses its own meaning of the power states. A periodic self-test of the UPS is no longer taken for a power failure.
  - Wrong values in several profiles: battery voltage ten times too high on the standard profile, APC frequency ten times too low, battery voltage, runtime and temperature swapped on CyberPower and NetAgent, and others. Firmware, serial number and output power of the standard profile read the wrong value.

- **Workflow**:
  - Opening *Workflows* or *History* turned the header and the colours of the whole application to a light blue.
  - The *History* page listed no runs when it was the first workflow page opened.
  - On Proxmox VE 9 the list of activities in the designer was empty.
  - *Manage HA* on Proxmox VE 9, where HA groups are replaced by HA rules: the group list is empty and a group gives a clear error.
  - An optional field left empty (backup notes, descriptions, target node or storage, HA group, notifiers, filter expression) made the activity fail.
  - *Filter Collection* failed with any expression on a field.
  - *Get Cluster Replications* was hidden by *Get Node Replications*.

- **AutoSnap, webhooks**:
  - The *CleanJobStart* and *CleanJobEnd* phases never fired: they now fire on *Purge Snapshots*, and a new webhook starts from *SnapJobStart*.
  - Closing the webhook editor without saving kept the changes.

- **Apprise**: notifications with an Excel attachment were refused by Apprise; attachments are sent correctly also when another notifier sends the same message. The editor opens at once instead of waiting for the server.

- **VM Performance**: a guest that stops or moves no longer breaks the page, and the rates are no longer negative after a guest restart. Guests that stop now leave the list and guests that start appear in it without reopening the page; on Proxmox VE 9 each disk of a guest has its own row again, with its name.

- **Diagnostic, compliance**: the findings are listed from the most severe, and the list of critical findings in the PDF says how many there are instead of calling the first five a top 5.

- **Daily status widgets** (Backup Analytics, AutoSnap, System Report): they show 14 days with a bar for every day, instead of 15 with gaps.

## [2.3.0] - 2026-09-24

This release focuses on **stability and interface polish**: pages no longer crash when data is refreshed in the background, grids keep expanded rows and groups the way you left them, severity colours are consistent and readable across light and dark themes, and notifications are clearer on every channel. Underlying components have been updated.

### Community Edition

#### Added

- **Orphan snapshots in Resources**: the guest *Snapshots* panel now also lists snapshots that still exist on the storage but are no longer known to Proxmox VE. They appear at the top, marked as orphans, with a warning showing how much space they take. They cannot be rolled back, edited or deleted from the UI. Like snapshot size calculation, this requires SSH access to the nodes.

- **Diagnostic — permission visibility check**: a new check reports up front which parts of the cluster the configured account can actually see. With a read-only role such as *PVEAuditor*, Proxmox VE hides data instead of returning an error, so guests could look unprotected; the backup checks now say they cannot see the volumes instead of reporting the opposite.

#### Changed

- **Notifications report what actually happened**:
  - Messages sent to webhooks and chat services no longer contain raw HTML tags; mail is sent as plain text.
  - The **Updater** notification now says how many security and regular updates are pending and how many guests need a reboot.
  - The test message names the notifier configuration that sent it, useful when several are set up.
  - Each module now sends its notifications with an appropriate severity instead of always as plain information.

- **Grids**: rows and groups tinted by severity use a lighter wash of the accent colour, so the text stays readable (also in the dark theme). Chart colours for severities now follow the application theme.

#### Fixed

- **AutoSnap**: the *Always* notification setting only sent failures, behaving like *On failure only*.
- **Webhook**: the default JSON template broke when the message contained quotes or line breaks (for example a job log).
- **Grids**: expanded detail rows collapsed and collapsed groups re-opened after a refresh or while scrolling; grids meant to open with groups collapsed now actually do.
- **Diagnostic**: findings about storages, users, backup jobs or cluster-wide settings no longer show a link that navigated away from the report.
- **Resources — Snapshots**: guests whose oldest snapshot had been removed by retention showed an empty snapshot list.
- **Profile**: the password fields are cleared (and hidden again) after a successful password change.
- **Dashboard**: the *Save* button is disabled until a name is entered; saving without a name raised an error.
- Pages listing AutoSnap jobs, backups, diagnostic scans, replications and system reports no longer crash when a background job updates the data while the page is open.
- Nested pools are now resolved correctly when selecting guests with `@pool-`.
- **Active Tasks panel**: a task finishing while the panel was open could appear twice and disconnect the page (yellow reconnect bar).
- **Tasks**: progress lines of long jobs such as System Report could fail to be saved, or get lost, while the job was running.

#### Documentation

- New section on orphan snapshots in Resources.
- Diagnostics: the page now lists all 14 compliance standards the checks are mapped to (previously only four were named), including GDPR, AgID, ENS and BSI C5.
- Corrected the PostgreSQL logging example in the advanced settings (the documented block did not produce any logging configuration), the Appearance settings, the search prefixes, the Proxmox VE permissions table (added AI Server and Bots) and the SSH verification in cluster settings.

### Enterprise Edition

#### Added

- **Daily Status widgets**: AutoSnap, Backup Analytics, Replication Analytics, Node Protect (folders) and System Report have a new widget showing successful and failed runs per day over the last 14 days.

- **UPS Monitor** and **Workflow** modules are now included in the released Enterprise Edition.

#### Changed

- **UPS Monitor — alerts**: notifications carry the right severity (error when a shutdown threshold is reached, warning for low battery or running on battery), are sent as plain text readable on every channel, and are translated.
- **System Logs**: the chart uses the application theme colours for log levels, matching badges and tinted rows elsewhere.
- **Portal**: orphan snapshots are not shown to tenants.
- The Apprise notifier image is pinned to a fixed version, so rebuilding the containers no longer changes the notifier under a working installation.

#### Fixed

- **Node Protect**: the *Git* entry showed "subscription required" instead of the actual Git configuration, because a placeholder appeared next to the real entry.
- **Workflow — Notify activity**: leaving the notifier list empty ("all") dropped the notification silently instead of sending it to every configuration.
- **Diagnostic — Compliance tab**: groups now stay collapsed or expanded as you left them, and findings with no page to open are shown as plain text instead of a link that left the report.
- **Grids**: expanded rows and groups are kept after a refresh in Portal tenants, Audit Logs, UPS devices and Node Protect Git.
- UPS dashboard, UPS devices and Node Protect Git pages no longer crash when a background job updates the data while the page is open.

## [2.2.0] - 2026-07-20

### Community Edition

#### Changed

- **Dashboard and Module Overview**: smoother widget grid. Drag, resize and rearrange widgets with better precision. Layout is more responsive across different screen sizes. Existing saved dashboards keep working as before — no manual step required.

- **Historical charts with zoom and pan**: on time-series charts (node/storage/VM metrics, Backup Analytics, Replication Analytics, Tasks) you can now zoom with the mouse wheel and drag to pan the view. Useful for inspecting a specific interval without changing filters.

#### Added

- **Dashboard widgets**: newly listed widgets on the Dashboard page (Cluster Usage Gauge Stacked, Cluster Usage Grid, Clusters Maps, VM/CT Locked, Nodes Status, Update Manager Status).

### Enterprise Edition

#### Changed

- **System Logs — Range selector**: below the 30-day log chart there is now a summary mini-chart with two draggable handles. Narrow the window down to days, hours or single spikes and the log grid below updates to match. Opens on the last 7 days by default.

- **Enterprise Edition — no license registration required**: the Subscription page is no longer shown in the menu. All enterprise modules run without activation.

## [2.1.0] - 2026-06-29

### Community Edition

#### Added

- **Built-in `system` user**. A locked-out, no-password user is now created on first boot and shown in the users list with the *Built-in* flag. It is the identity used by scheduled jobs and other non-interactive workflows (cron snapshots, retention cleanups, workflow timers), so the audit trail records *who actually did what* even when no person is logged in. It cannot be deleted, modified or used to sign in.

- **Scheduled job identity**. Any background job started from the web UI now runs as the user who clicked the button — manual *Snapshot now*, *Run scan now*, *Cleanup retention* and so on are recorded against your account, not as *anonymous*. Cron-driven jobs run as the new `system` user.

- **Module Overview pages**. Every module landing page now opens with the existing three-column intro plus a small dashboard built from the module's own widgets — no more *Overview* tabs that only show decorative text. Modules that have nothing meaningful to show as widgets (e.g. **Resources**) keep the textual overview as before.

- **Status badges**. The **Tasks** page status column now shows a coloured pill with an icon (Running / Completed / Failed / Cancelled / Abandoned), matching the level badges already used on the *(EE)* **Logs** page.

- **`adminctl self-update`** command refreshes your local `adminctl` script and the docker-compose YAML to match a chosen release without touching your data or `.env` configuration. It takes the latest files from the main branch. Existing files are backed up under `self-update-backups/<timestamp>/` before being overwritten.

- **Diagnostic**
  - PDF and Excel reports are now available in the free edition (used to be Enterprise-only). The PDF lists every issue colour-coded by severity and a separate section with the issues you have chosen to ignore. The Excel workbook opens on a cover sheet and the data sheet has the autofilter already on.
  - **Compliance mapping**. Every check is tagged with the controls it covers across ISO/IEC 27001, EU NIS2, EU DORA and PCI DSS v4.0. The scan details now have a *Compliance* tab that groups findings by standard and control; the mapping is saved with the scan, so older reports stay consistent.
  - **Audit mode**: a new switch makes each passing check appear as a *Pass* result — handy when you have to prove that a control was actually verified, not only when it failed.
  - **Attach PDF / Excel** toggles in the notifier settings.
  - New checks: ZFS pool detail, LVM-thin metadata, S.M.A.R.T. temperature and SSD wearout thresholds, PSI Pressure thresholds (CPU / I/O / Memory), snapshot age and backup recency (RPO).
  - Each issue links straight to the relevant Proxmox VE documentation page (QEMU guest agent, VirtIO drivers, end-of-life OS).
  - The *Download* button on the Scans page is a split-button: one click for the PDF (default), use the arrow to pick the Excel.

- **Updater**
  - PDF and Excel reports in the free edition. The PDF lists every VM/CT and the status of its update scan; if some hosts failed the scan they get their own *Errors* page.
  - **Parallel scan** with a configurable *Max Parallel Requests* (1–50). A single host failing no longer stops the whole scan: the error is recorded against that host and the rest of the run continues.
  - Clicking *Scan* now starts the job immediately (the previous 5-second delay is gone).

- **System Report**
  - **Three output formats**: **Excel** (the existing one), **HTML** (a single browseable page with a table of contents and cross-section links) and **JSON** (machine-readable, with raw values and clean slugs).
  - **One zip file as result**, whatever the format: the zip contains the report itself plus the SVG network diagram.
  - The chosen format is **saved with the report** — opening an old report from the list reminds you how it was generated.
  - *Read-only* mode for the report dialog.
  - The page is now called *Reports* in the side menu (was *Scans*).

- **AutoSnap**
  - **Max Parallel** option per job (1–10) so snapshots within the same run can execute in parallel.

- **AI Server**
  - New MCP tools: list/download/delete ISO images, list templates, delete backups, delete arbitrary storage content, read cluster defaults (migration, bandwidth, HA, console).

- **Metrics Exporter**
  - **Per-cluster configuration** (used to be global): each cluster has its own on/off, its own collectors and its own cache windows.
  - Master *Enabled* switch, plus per-exporter and per-collector switches with editable cache seconds.
  - *Fast / Standard / Full* preset buttons to apply common configurations in one click.

- **Node Protect**
  - Folder backup runs now appear in *System → Tasks* alongside AutoSnap, System Report and Diagnostic, with a clickable deep link and per-node summary lines.
  - Updated default *Paths to backup*: dropped wildcard entries that didn't expand correctly when quoted, added `/var/spool/cron/crontabs` so per-user cron jobs are picked up.

- **Resources**
  - **Three new pages** in the side menu: **Networks**, **Disks** and **Partitions**, each with its own filters, grouping and card / list view switcher.
  - **Networks** has four tabs: *Nodes* (per-node bridges and bonds), *Guests* (per-VM virtual NICs), *SDN* (virtual networks) and *Diagram* (downloadable SVG topology view).
  - **Disks** lists the physical disks across the cluster with disk **Kind** (HDD / SSD / NVMe), model, serial, health and S.M.A.R.T. summary, with a drill-down per disk.
  - **Partitions** shows the partition / mount table per node.
  - **Snapshots** has a new *Trends* tab next to the snapshot list.
  - **Storages** reorganised into tabs: *Configuration*, *Storages*, *Usage* (with sub-tabs *By Storage* and *By VMs*).
  - **VMs** page renamed to **Guests** (it has always included LXC containers, the name now reflects the content).
  - **Card view** alongside the list view: every Guest / Node / Storage gets a card with status and key metrics. Toolbar with type / status / tag toggles, search and grouping.
  - Unified *Health Score* indicator: shows `N/A` when no data is available, with a tooltip explaining why.
  - Guest cards and lists show the operating system icon with a tooltip.

- **System**
  - The **Tasks** page has a search box in the toolbar that filters across Title, Cluster, Module, Phase, Created by and Last log.
  - The **Active Tasks** menu auto-clears the badge for failed / abandoned tasks once you have opened the panel.
  - **New user activation flow**: when an admin creates a user from the UI the new user receives an activation email with a password-reset link, instead of being created without a password.
  - **User unlock** command added to the CLI.
  - **Maintenance: *Compact (full)*** — new button that reclaims disk space after big cleanups. The action is opt-in only, asks for explicit confirmation, and is not run by *Fix All*.
  - **Help bundled with the app** — the user documentation is now included in the container and served locally.

- **Command palette**
  - The `ip:` filter now shows one IPv4 badge per VM (IPv6 and loopback addresses filtered out).

#### Changed

- **Issue cards on the *AutoSnap*, *Backup Analytics*, *Replication Analytics*, *Diagnostic* and *Resources* widgets**. The old "thumb up / thumb down + bullet list" has been redesigned: when everything is fine you see a green thumb up, when there are issues a red thumb down with a number, and when the module is not configured a neutral icon with an explanatory message. Hovering the number opens a compact list of the failing items; each row is clickable and takes you straight to the relevant detail page. With many rows the list scrolls inside itself instead of growing without bound.

- **Diagnostic issue labels**. The items on the *Diagnostic* widget used to be shown as internal paths that were hard to read. They now read *VM 100 on cc01 (cluster)*, *Node cc01 (cluster)*, *Storage ssd-pool (cluster)* and so on.

- **Diagnostic** settings screen redesigned with one accordion per area (Node, QEMU, LXC, Storage, Snapshot, Backup, CVE) and per-context thresholds clearly grouped.

- **Diagnostic** CVE checks now look up Proxmox VE specific CVEs in NVD using a CPE filter — wider coverage than the previous Debian-only tracker.

- **Resources** list view has been redesigned alongside the new card view (filters, grouping and search work consistently across both).

- **Task Tracking** Active Tasks panel and menu refresh row-by-row instead of reloading the whole grid, so the UI no longer flickers when tasks update.

#### Fixed

- **Built-in role permissions on existing installations**. Older databases were seeded with a flag that prevented built-in admin roles from matching specific resources (single VM, single node). The check appeared to *succeed* in role lists but silently *denied* the actual action — most visibly on scheduled AutoSnap deletes. The flag is now correctly set on fresh installs, and existing installations are realigned automatically on first boot.

- **Postgres log sink**. A typo in the column configuration was preventing the database log sink from writing any row — the file log kept working, so the issue went unnoticed. Database logging is now restored, and any future sink failure is surfaced on standard error instead of being swallowed.

- **PVE command result on permission denied / unauthorized / failure**. A regression in the result object construction caused an internal error to bubble up to the UI instead of the expected *Operation not permitted* / *Authentication required* message.

- **Backup Analytics scan**. The scan aborted in the middle when at least one vzdump task in the run had failed (e.g. a VM locked by a snapshot). The failed task is now imported correctly together with the successful ones.

- **KPI cards in the *Snapshot Statistics and Insights* widget** kept stacking vertically even on large screens. They now stay horizontal as soon as there is room for them.

- **Resources → Snapshots** and other grouped grids (AutoSnap status, BackupAnalytics, ReplicationAnalytics, Node Protect folders, Node storage contents): when grouping by more than one column the outer headers used to show only the group key and count. They now correctly aggregate totals (sizes, CPU and disk usage) from every nested level.

- **Diagnostic** Excel export no longer fails on scans where every issue is ignored. PDF reports wrap long text inside every cell instead of running off the right edge of the page.

- **System**
  - Cluster names with spaces no longer break navigation between module pages.
  - Validation messages inside edit dialogs are shown again.
  - Pages with many permission checks load fast, even on installations with many admin roles.
  - The "New Cluster" form no longer refuses the first submit because of a *PveName is required* error.
  - Release notes dialog shows only the current version, not earlier release candidates.
  - QEMU agent network cache refresh dropped from 30 to 15 minutes, so freshly attached NICs appear faster.
  - **Maintenance: *Reindex* and *Optimize* no longer touch other modules' data**. They used to run on the whole database once per installed module — so on an installation with sixteen modules each action ran sixteen times over everything. Now they run once per module, on that module's data only.

- **Notifier**: SMTP and WebHook editors refresh correctly when you pick a different notifier from the list.

- **Docker**: the container health check no longer marks the running container as unhealthy on every probe.

#### Compatibility note

- The default Apprise endpoint in the notifier settings changed from `http://localhost:8000` to `http://apprise:8000` to match the docker-compose service name. Existing notifiers keep the value you saved; new notifiers will use the new default.

#### Internal

- The background job system now uses a real PostgreSQL queue again (it had quietly fallen back to in-memory storage because of a conflicting registration from the workflow engine). Jobs survive process restarts, retries are durable, and the *Hangfire* dashboard shows real history.

### Enterprise Edition

#### Added

- **System / Logs** new native *Logs* page (replaces the Serilog UI integration) with server-side filtering by level, message and source, a stacked column chart of the last 30 days grouped by level, and a row-expansion for heavy fields (message template, exception, raw log event JSON) so the page stays fluid even on tables with millions of records.

- **Audit Logs** *Success* column now uses a coloured pill badge (*Success* / *Failed*).

- **Diagnostic — Compliance report**
  - The PDF gains one section per standard (ISO 27001, NIS2, DORA, PCI DSS). Every section starts on a new page with a short disclaimer, a summary table and a per-control detail block.
  - The Excel workbook gains one *Compliance - &lt;Standard&gt;* sheet per standard.
  - In the scan details page the *Compliance* tab is a real grid grouped by Standard → Control, with clickable links to the affected resources.
  - Colour coding on Gravity / Status cells is consistent between PDF and Excel.

- **System / Security**
  - When an admin creates a new user, a confirmation email is sent automatically. The new user receives an *activate your account* link instead of being stuck with no password.
  - Audit log detail dialog has a copy-to-clipboard button.

- **Portal**
  - Tenant user dialog has a new *Display Name* field; the previous *Admin* checkbox is now clearly labelled *Tenant Admin*. The *UserName* field only accepts email addresses, and is editable only when creating a new user.
  - Tenants without VMs show a friendly note instead of an empty table.

- **Node Protect / Git**
  - Git push runs appear in the task tracker with a clickable link to the Git page, and the task log lines now show remote URL, branch and outcome.

- **Updater**
  - The Enterprise PDF builds on top of the new free-edition PDF and still includes the Executive Summary at the top.

- **Workflow**
  - New built-in activities to query the cluster (replications, guest config, guests, RRD data) and pickers for HA groups, node names, storage names and VM ids.

#### Changed

- **Notifications** updated. Microsoft Teams is no longer supported; ntfy.sh users may need to update their URL.

#### Fixed

- **Background job permissions**. AutoSnap retention deletions and other scheduled jobs were failing with *Permission denied* because the job was running as *anonymous*. They now run as the `system` user with full admin rights and complete successfully.

- **Audit log writes**. Some audit log entries were silently dropped because the database constraint that links them to the user table was violated when no user could be resolved. The user is now always known (real user from the request, or `system` for jobs), and entries are written without errors.

- **Login activity in audit logs**. Successful and failed login attempts are now recorded with the attempted username (in the *Details* column), not as *anonymous*, making it possible to spot brute-force attempts on a specific account.

- **Edit User dialog** could fail on first render because of an internal type-visibility issue with the roles pick-list. The dialog opens reliably again.

- **Apprise notifier** *Render* page sometimes failed to load the catalog of available services because Apprise can serve either JSON or HTML on the same URL. We now explicitly ask for JSON and show a clear error if the endpoint replies with something else.

- **Diagnostic / Updater** PDF tables wrap long values correctly.

- **Workflow** editor no longer fails to start in tenant mode (the live-update channel authenticates correctly).

## [2.0.0] - 2026-03-09

> 🎉 After over a year of complete rewrite, **cv4pve-admin 2.0.0** is here.
> Rebuilt from scratch on .NET 10, Blazor, and Radzen — modern architecture,
> modern stack, modern UX. Thank you to all testers and contributors. 🙏

#### Platform

- Multi-cluster management from a single interface
- Health Score for nodes, VMs, and storages
- Quick command search filtered by user permissions
- Fine-grained user roles and permissions per module
- Audit logging for security-relevant events
- Localization with per-user language/culture selection
- Bookmarkable URLs for every cluster and module
- Real-time progress tracking for running operations
- Dark/light theme, keyboard shortcuts
- Docker, native, and .NET Aspire deployment support
- Notification system with email and WebHook support *(extended channels available in Enterprise Edition)*

### Community Edition

#### Modules

- **AutoSnap** — automated snapshot scheduling with retention policies, hooks, and timeline view
- **Backup Analytics** — backup trend analysis with size, speed, and duration charts
- **Replication Analytics** — replication job monitoring and failure tracking
- **Diagnostic** — cluster health diagnostics with issue scanning and PDF export
- **Dashboard** — customizable widget-based dashboard (gauges, charts, sparklines, heatmaps)
- **Node Protect** — node configuration backup with multiple storage backends
- **Metrics Exporter** — export cluster metrics to external monitoring systems
- **AI Server** — connect Proxmox VE to AI assistants (Claude, ChatGPT, Cursor…) via Model Context Protocol
- **Bots** — Telegram bot for remote cluster management
- **UPS Monitor** — UPS status monitoring via NUT protocol
- **System Report** — system information and configuration report generation
- **Updater** — application self-update management
- **Resources** — cluster resource overview with Health Score, usage charts, and bookmarkable URLs

### Enterprise Edition

#### Additional Modules

- **Portal** — self-service portal for end-user VM management
- **Workflow** — visual workflow automation powered by Elsa
- **VM Performance** — advanced VM performance analytics and trending
- **DRS** — dynamic resource scheduling with HA groups and migration history
- **AI Server Enterprise** — AI Query Engine: ask questions about your infrastructure in natural language
- **System Enterprise** — advanced user management with roles, AppTokens (machine-to-machine API authentication with granular permissions), full audit log, SSO integration, and appearance customization
- **Diagnostic Enterprise** — extended diagnostics with enterprise-specific checks
- **Node Protect Enterprise** — additional storage providers for node configuration backup
- **Notifier Enterprise** — extended channels: Apprise, custom webhooks
- **Profile Enterprise** — extended user profile and SSO integration

