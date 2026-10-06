/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 *
 * Feature cards shared by several module pages: written once, listed in the `items` of a
 * FeatureGrid next to the cards of the page. The links are relative to a module page.
 */

export const featureCron = {
  icon: 'mdi:calendar-clock',
  title: 'On-demand or scheduled',
  text: 'Trigger a run manually from the UI, or schedule recurring runs with a cron expression, using the built-in editor that shows a human-readable description and the next occurrence.',
};

export const featureExportPdfExcel = {
  icon: 'mdi:download',
  title: 'Export PDF / Excel',
  text: 'Download results as **PDF** (colored table with footer and page numbers) or **Excel** (autofilter-enabled sheet for slicing, pivoting and sharing). Format chosen from a split-button dropdown.',
};

export const featureNotifier = {
  icon: 'mdi:bell-ring',
  title: 'Notifier Integration',
  text: 'Attach the generated report to a notification using any of the configured [Notifier](../../configuration/admin-area/notifier/) channels: handy for nightly cron-based runs.',
};

export const featureParallelScan = {
  icon: 'mdi:bolt',
  title: 'Parallel Scanning',
  text: 'The **Max parallel requests** setting (default 5) speeds up scans on large clusters; set to 1 to fall back to sequential mode. A failure on one resource is isolated to that item: the rest of the scan keeps running.',
};

export const featureTaskTracker = {
  icon: 'mdi:format-list-checks',
  title: 'Task Tracker integration',
  text: 'Every run shows up in the global Task Tracker with rich logs (per-resource progress, per-step errors): find what broke without grepping a separate log.',
};
