/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 *
 * The Proxmox VE privileges each module needs beyond the base (the built-in PVEAuditor role).
 * Written once here: the PvePrivileges component prints them in the page of each module, in the
 * blocks of the AI Server tools and of the Workflow activities, and in the summary table of
 * configuration/pve-permissions.mdx, so the pages cannot disagree.
 *
 * Source of each entry: the Proxmox VE API calls the module makes, checked against the permissions
 * of the Proxmox VE 9 API schema, and the permissions pages of the cv4pve tools the modules embed.
 * Change an entry only after reading the code.
 *
 * In the texts: `code` between backticks, **bold** between double asterisks.
 */

/** Rows: `privileges` (all needed together), `use`, and `without` (what happens when missing). */
const backupFiles = {
  privileges: ['VM.Backup', 'Datastore.AllocateSpace'],
};

/** Actions on guests and nodes: Dashboard, Resources and Portal offer them. */
export const guestActions = [
  { privileges: ['VM.PowerMgmt'], use: 'Start, stop, shutdown, reboot, reset, suspend, resume a guest' },
  { privileges: ['VM.Console'], use: 'Guest console' },
  { privileges: ['VM.Snapshot'], use: 'Create, edit, delete a snapshot' },
  { privileges: ['VM.Snapshot.Rollback'], use: 'Roll back a snapshot' },
  { privileges: ['VM.Replicate'], use: 'Replication: **Schedule Now**' },
  { ...backupFiles, use: '**Backups** tab: list, configuration and deletion of backups' },
  { privileges: ['Sys.Console'], use: 'Node console' },
  { privileges: ['Sys.PowerMgmt'], use: 'Shut down, reboot a node' },
];

/**
 * Key: the slug of the module page.
 * `rows`: the privileges beyond the base. `actions: true`: the module offers the actions on guests
 * and nodes. `by` and `page`: the privileges depend on the tool or the activity used, written in
 * that page. `notes`: sentences under the table. A module with none of these needs the base only.
 */
export const pvePrivileges = {
  'ai-server': {
    name: 'AI Server',
    by: 'tool',
    page: 'ai-server-tools',
    notes: ['The read-only tools need the base only. `ListBackups` and `ListStorageContent` return the backup files only with `VM.Backup` and `Datastore.AllocateSpace`.'],
  },
  autosnap: {
    name: 'AutoSnap',
    rows: [
      { privileges: ['VM.Audit'], base: true, use: 'List the guests, read their configuration and snapshots', without: 'The guest is not selected' },
      { privileges: ['VM.Snapshot'], use: 'Create and remove snapshots, also with **Include RAM**', without: 'The run fails' },
      { privileges: ['Datastore.Audit'], base: true, use: 'Storage usage for **Max percentage storage**', without: 'The storage check is skipped and the snapshots are taken anyway' },
      { privileges: ['Pool.Audit'], base: true, use: 'Select guests by pool', without: 'The pool selects no guests' },
    ],
    notes: [
      'On Proxmox VE 8 and earlier the pool list needs `Pool.Allocate` in place of `Pool.Audit`.',
      '`VM.Snapshot` also allows the rollback of a guest to any of its snapshots. AutoSnap never rolls back, but who holds the account can.',
    ],
  },
  'backup-analytics': {
    name: 'Backup Analytics',
    rows: [{ ...backupFiles, use: 'List the backup files', without: 'The list of backups is empty, with no error' }],
    notes: ['Proxmox VE lists a backup file only to an account that has both privileges: `VM.Backup` on the guest and `Datastore.AllocateSpace` on the storage.'],
  },
  bots: {
    name: 'Bots',
    rows: [
      { privileges: ['VM.PowerMgmt'], use: '`/vmstart`, `/vmstop`, `/vmshutdown`, `/vmreset`' },
      { privileges: ['Sys.PowerMgmt'], use: '`/nodereboot`, `/nodeshutdown`' },
    ],
    notes: ['`/get`, `/set`, `/create`, `/delete` and the aliases need the privilege of the API path they call.'],
  },
  'command-palette': {
    name: 'Command Palette',
    rows: [
      { privileges: ['VM.PowerMgmt'], use: '`start`, `stop`, `restart`' },
      { privileges: ['VM.Console'], use: '`console`' },
      { privileges: ['VM.Snapshot'], use: '`create snapshot`' },
    ],
  },
  dashboard: { name: 'Dashboard', actions: true },
  diagnostics: {
    name: 'Diagnostics',
    rows: [
      { privileges: ['Sys.Modify'], use: 'List of the updates available on each node, in every profile', without: 'The checks on the updates fail' },
      { ...backupFiles, use: 'Backup checks, Standard and Full profiles', without: 'The backup checks are skipped' },
    ],
    notes: ['These are more than read-only privileges: Proxmox VE asks for them also to read these data.'],
  },
  'metrics-exporter': { name: 'Metrics Exporter' },
  'node-protect': { name: 'Node Protect', note: 'It works over SSH.' },
  portal: { name: 'Portal', actions: true },
  'replication-analytics': { name: 'Replication Analytics' },
  resources: { name: 'Resources', actions: true },
  'system-report': {
    name: 'System Report',
    rows: [
      { privileges: ['Sys.Modify'], use: 'Updates available on each node, Standard and Full presets', without: 'A warning for each node' },
      { privileges: ['Sys.Syslog'], use: 'Node firewall log (Standard and Full presets) and syslog (Full preset)', without: 'A warning for each node' },
      { privileges: ['VM.Console'], use: 'Firewall log of the guests, Standard and Full presets', without: 'A warning for each guest' },
      { ...backupFiles, use: 'Backups, Standard and Full presets', without: 'The backups are missing from the report' },
      { privileges: ['VM.Config.Disk'], use: 'Disk images in the storage content, Standard and Full presets', without: 'The disk images are missing from the report' },
    ],
    notes: ['The Fast preset needs the base only. All these privileges except `Sys.Syslog` are more than read-only: Proxmox VE asks for them also to read these data.'],
  },
  'update-manager': { name: 'Update Manager', note: 'It works over SSH.' },
  'ups-monitor': { name: 'UPS Monitor', note: 'It reads the UPS over SNMP.' },
  'vm-performance': { name: 'VM Performance' },
  workflow: { name: 'Workflow', by: 'activity', page: 'workflow-activities', notes: ['The inventory activities need the base only.'] },
};

/** AI Server tools that need more than the base. The others are read-only. */
export const toolPrivileges = {
  ChangeVmState: ['VM.PowerMgmt'],
  CreateVmSnapshot: ['VM.Snapshot'],
  DeleteVmSnapshot: ['VM.Snapshot'],
  RollbackVmSnapshot: ['VM.Snapshot.Rollback'],
  MigrateVm: ['VM.Migrate'],
  BackupVm: ['VM.Backup', 'Datastore.AllocateSpace'],
  ListBackups: ['VM.Backup', 'Datastore.AllocateSpace'],
  DeleteBackup: ['Datastore.Allocate'],
  DeleteIso: ['Datastore.Allocate'],
  DeleteStorageContent: ['Datastore.Allocate'],
  DownloadIso: ['Datastore.AllocateTemplate'],
};

/** Workflow activities that need more than the base, by display name. */
export const activityPrivileges = {
  'PVE VM Power Action': ['VM.PowerMgmt'],
  'PVE Create Snapshot': ['VM.Snapshot'],
  'PVE Delete Snapshot': ['VM.Snapshot'],
  'PVE Update Snapshot': ['VM.Snapshot'],
  'PVE Rollback Snapshot': ['VM.Snapshot.Rollback'],
  'PVE Migrate': ['VM.Migrate'],
  'PVE Backup': ['VM.Backup', 'Datastore.AllocateSpace'],
  'PVE Clone': ['VM.Clone', 'VM.Allocate', 'Datastore.AllocateSpace'],
  'PVE Resize Disk': ['VM.Config.Disk'],
  'PVE Convert to Template': ['VM.Allocate'],
  'PVE Manage HA': ['Sys.Console'],
  'PVE Node Shutdown/Reboot': ['Sys.PowerMgmt'],
};
