// SPDX-FileCopyrightText: Copyright Corsinvest Srl
// SPDX-License-Identifier: AGPL-3.0-only

// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import corsinvestTheme from '@corsinvest/cv4pve-docs-theme';

// The same site is built twice: for GitHub Pages (default base) and for the copy embedded in the
// application, served on /help/ (DOCS_BASE=help). Pages link to each other with relative URLs,
// so they work under both. The value has no leading slash: Git Bash on Windows would rewrite
// "/help" as a file path.
const base = '/' + (process.env.DOCS_BASE ?? 'cv4pve-admin').replace(/^\/+/, '');

export default defineConfig({
  site: 'https://corsinvest.github.io',
  base,
  integrations: [
    starlight({
      title: 'cv4pve-admin',
      description: 'Professional administration interface for Proxmox VE with advanced features and support.',
      // Brand, product icon, GitHub link, the Corsinvest sidebar group and
      // external links in a new tab come from the shared cv4pve theme.
      plugins: [
        corsinvestTheme({
          repo: 'cv4pve-admin',
          // End of the <title> of the pages, in place of the site name: what people search for.
          // A page with its own <title> in the frontmatter keeps it.
          titleSuffix: 'cv4pve-admin for Proxmox VE',
          branch: 'main',
          // Visits, without cookies. Only on the public site: the copy embedded in the application
          // (DOCS_BASE set) sends nothing.
          matomo: process.env.DOCS_BASE ? undefined : { url: 'https://matomo.corsinvest.it/', siteId: 16 },
          // Product icon: favicon and header, dark variant for the dark theme.
          icon: { light: '/icon.svg', dark: '/icon-dark.svg' },
          // Steps panel in the home hero: the same steps, in the same order and words, as
          // Getting started. That page can have more steps, never different ones.
          steps: {
            items: [
              'Run the installer',
              'Start the containers',
              'Open `http://<server-ip>:8080`',
              'Add your first Proxmox VE cluster',
            ],
          },
        }),
      ],
      head: [
        // The theme opens in a new tab the external links written in Markdown and those of the header.
        // This covers the ones Starlight writes itself.
        {
          tag: 'script',
          content:
            `document.addEventListener('DOMContentLoaded',()=>{document.querySelectorAll('a[href^="http"]:not([target])')` +
            `.forEach((a)=>{if(a.host!==location.host){a.target='_blank';a.rel='noopener noreferrer';}});});`,
        },
      ],
      customCss: ['./src/styles/custom.css'],
      sidebar: [
        {
          label: 'Start here',
          items: [
            { label: 'Getting Started', slug: 'getting-started' },
            { label: 'Docker Deployment', slug: 'docker' },
            { label: 'Editions', slug: 'editions' },
            { label: 'User Guide', slug: 'user_guide' },
            { label: 'Concepts', slug: 'concepts' },
          ],
        },
        {
          label: 'Configuration',
          items: [
            { label: 'Overview', slug: 'configuration' },
            { label: 'appsettings.extra.json', slug: 'configuration/appsettings-extra' },
            {
              label: 'Admin Area',
              collapsed: true,
              items: [
                { label: 'Overview', slug: 'configuration/admin-area' },
                { label: 'Proxmox VE Clusters', slug: 'configuration/admin-area/clusters' },
                { label: 'Security & Access Control', slug: 'configuration/admin-area/security' },
                { label: 'Monitoring', slug: 'configuration/admin-area/monitoring' },
                { label: 'Maintenance', slug: 'configuration/admin-area/maintenance' },
                { label: 'General Settings', slug: 'configuration/admin-area/general-settings' },
                {
                  label: 'Notification Hub',
                  collapsed: true,
                  items: [
                    { label: 'Overview', slug: 'configuration/admin-area/notifier' },
                    { label: 'WebHook Examples', slug: 'configuration/admin-area/notifier-webhook-examples' },
                  ],
                },
              ],
            },
            { label: 'PVE Permissions', slug: 'configuration/pve-permissions' },
            { label: 'User Profile', slug: 'configuration/profile' },
          ],
        },
        {
          label: 'Modules',
          collapsed: true,
          items: [
            { label: 'Overview', slug: 'modules' },
            {
              label: 'AI Server',
              collapsed: true,
              items: [
                { label: 'Overview', slug: 'modules/ai-server' },
                { label: 'Tools', slug: 'modules/ai-server-tools' },
                { label: 'MCP Bridge', slug: 'modules/ai-server-bridge' },
                { label: 'Query', slug: 'modules/ai-server-query' },
              ],
            },
            {
              // The Web API Hook is the Enterprise part of AutoSnap, not a module of its own.
              label: 'AutoSnap',
              collapsed: true,
              items: [
                { label: 'Overview', slug: 'modules/autosnap' },
                { label: 'Web API Hook', slug: 'modules/autosnap-webhook' },
              ],
            },
            'modules/backup-analytics',
            'modules/bots',
            'modules/command-palette',
            {
              label: 'Dashboard',
              collapsed: true,
              items: [
                { label: 'Overview', slug: 'modules/dashboard' },
                { label: 'Widgets', slug: 'modules/dashboard-widgets' },
              ],
            },
            {
              label: 'Diagnostics',
              collapsed: true,
              items: [
                { label: 'Overview', slug: 'modules/diagnostics' },
                { label: 'Settings', slug: 'modules/diagnostics-settings' },
                { label: 'Compliance Reports', slug: 'modules/diagnostics-compliance' },
              ],
            },
            'modules/metrics-exporter',
            'modules/node-protect',
            'modules/portal',
            'modules/replication-analytics',
            'modules/resources',
            'modules/system-report',
            'modules/update-manager',
            'modules/ups-monitor',
            'modules/vm-performance',
            {
              label: 'Workflow',
              collapsed: true,
              items: [
                { label: 'Overview', slug: 'modules/workflow' },
                { label: 'Activities', slug: 'modules/workflow-activities' },
              ],
            },
          ],
        },
        {
          label: 'Troubleshooting',
          items: [
            'troubleshooting',
            { label: 'CLI Reference', slug: 'cli' },
            { label: 'HTTPS Setup', slug: 'https' },
          ],
        },
      ],
    }),
  ],
});
