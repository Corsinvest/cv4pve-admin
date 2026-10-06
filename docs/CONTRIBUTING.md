# Writing the cv4pve-admin documentation

The documentation is an [Astro Starlight](https://starlight.astro.build/) site with the shared Corsinvest theme (`@corsinvest/cv4pve-docs-theme`). Pages are MDX files in `src/content/docs/`.

## Commands

Run them from this folder (`docs/`).

| Command | What it does |
|---------|--------------|
| `npm install` | Installs the dependencies |
| `npm run dev` | Local preview with live reload |
| `npm run check` | Builds the site and checks the rules below, links, anchors and sidebar. Run it before every commit |
| `npm run build` | Builds the site for GitHub Pages into `dist/` |
| `DOCS_BASE=help npm run build:help` | Builds the copy embedded in the application into `dist-help/` |

After adding a page, or changing the frontmatter of one, restart `npm run dev`.

## What a page says

- Only what the code does. Read the code before writing: what a button does, which fields a tool returns, the default of a setting. A name or an attribute in the code is not enough, read what the method does.
- When the code is wrong, the page is not changed to describe the bug: the bug goes to the maintainers, and the page keeps saying what the feature is meant to do, or stays silent on the broken part.
- Labels are written as the application shows them (`Cron Schedule`, not `Cron Expression`), in bold.
- A number stated in more than one page (tools, activities, widgets, standards) is the same everywhere.
- The same explanation is written once: the other pages link to it.
- English, plain sentences. No em dash and no en dash: a colon, a comma, parentheses or a new sentence.

## A page

- `title` and `description` in the frontmatter. No `# H1` in the body.
- Headings go down one level at a time, and a heading is not repeated in the same page.
- Links between pages are relative and end with a slash: `../autosnap/`, `../autosnap/#jobs`.
- Literal `{` and `}` in text are written in backticks, and so is `<word>`. No HTML comments.

### Module pages

A module page has, in this order: the line under the title, one sentence that says what the module does, then **Features**, **Why**, **Sections** (the pages of the module menu, in the same order and with the same names), the sections that explain each part, **Settings**, **Widgets**, **Permissions**.

The line under the title holds the icon of the module and its badges. It is there only when it has at least one badge:

```mdx
<p class="cv-page-meta"><ModuleIcon name="photo_camera" size="lg" /><Tag kind="per-cluster" /></p>
```

- `ModuleIcon`: the icon of the main link of the module in its `Module.cs`, written the same way.
- `<Tag kind="ee" />` only when the whole module is Enterprise.
- `<Tag kind="per-cluster" />` or `<Tag kind="all-clusters" />` from the scope of the module.

### Subpages

A section becomes a page of its own when it is a subject on its own (readers look for it directly), or a long reference (a catalogue of tools, activities, settings, widgets) that takes a large part of the page. The parent page keeps the heading with two lines and a link, so links to that section keep working. In `astro.config.mjs` the module becomes a group, with `Overview` first.

Do not split a short page: a page under about 150 words is too small to stand alone.

## Components

### Cards

Cards are `FeatureGrid`, from the theme. One line per card:

```mdx
import FeatureGrid from '@corsinvest/cv4pve-docs-theme/components/FeatureGrid.astro';

<FeatureGrid
  items={[
    { icon: 'mdi:calendar-clock', title: "Scheduled Snapshots", text: "Create snapshots on a cron schedule.", href: '#jobs' },
    { icon: 'mdi:webhook', title: "Web API Hook", text: "HTTP calls on the snapshot phases.", href: '../autosnap-webhook/', ee: true },
  ]}
/>
```

- `icon` on every card: a Starlight icon (`padlock`), a Material Design one (`mdi:name`) or a Material Symbols one as in the code (`ms:photo_camera`).
- `href` when a section of the page, or another page, explains the card: `#section` or `../page/`.
- `ee: true` / `ce: true` for the edition badge beside the title; `color` to tell groups apart; `action: 'Label'` for a button in place of the link on the whole card.
- In `text`: `[EE]` `[CE]` badges, `[[Ctrl+K]]` a key or a label, backticks for code, `**bold**`, `*italic*`, `[text](../page/)` links, lines starting with `- ` for a list, an empty line between blocks. No components.
- Cards used by several pages are in `src/data/shared-features.js`.

### Badges

`<Tag kind="ee" />`, `ce`, `ssh`, `per-cluster`, `all-clusters`. In a heading the badge follows the text with no space, so the anchor is the text alone: `## Users<Tag kind="ee" />`.

### Proxmox VE privileges

The privileges a module needs on Proxmox VE are written once, in `src/data/pve-privileges.js`. The `PvePrivileges` component shows them where the reader looks for them:

```mdx
import PvePrivileges from '~/components/PvePrivileges.astro';

<PvePrivileges module="autosnap" />
```

| Use | Where | Shows |
|-----|-------|-------|
| `module="autosnap"` | Module page, at the end of **Permissions** | What the module needs beyond the base (`PVEAuditor`): privilege, what it is used for, what happens without it |
| `tool="MigrateVm"` | Block of an AI Server tool | The privilege of the tool, in the line `- **Proxmox VE privilege:**` |
| `activity="PVE Migrate"` | Block of a Workflow activity | The privilege of the activity, same line |
| `actions` | `configuration/pve-permissions.mdx` | The actions on guests and nodes of Dashboard, Resources and Portal |
| `table` | `configuration/pve-permissions.mdx` | The summary of every module |

- Do not write a privilege by hand in a page: add it to the data file.
- A new module needs its entry in the data file: without it the build stops.
- Change an entry only after reading which Proxmox VE API calls the module makes.

### Asides

`:::note[Title]`, `:::caution[Title]`, `:::danger[Title]`, closed by `:::`.

- `caution` and `danger` for security, loss of data, or something that stops the user. `note` for a requirement or a limit to know beforehand.
- At most two in a page, never one right after or inside another, no `:::tip`.
- An explanation ("why", "how it works"), an advice or a step of a procedure is plain text: a paragraph that starts with its title in bold (`**Title:** text`), a step of a list, or a section.

### Collapsed blocks

`Collapse`, from the theme, with an empty line before and after its content:

```mdx
import Collapse from '@corsinvest/cv4pve-docs-theme/components/Collapse.astro';

<Collapse title="Show all settings" kind="settings">

| Setting | Default | Purpose |
|---------|---------|---------|

</Collapse>
```

Collapse only a secondary reference of some length inside a page that reads as text: a table of settings, fields or permissions with six rows or more. Leave open what is short, and what is the main content of its page.

### Tables or blocks

A table when the cells are short. When a table has three columns or more and long cells, write one block per row instead: the name (a heading in a reference page, bold elsewhere), the text, then the other values as a short list.

```mdx
**Keep**

How many snapshots to keep for each guest.

- **Default:** 7
```

### Steps and tabs

A procedure is the Starlight `Steps` component; alternatives for the same step (Linux, Windows) are `Tabs`. Commands go in code blocks, so they can be copied.

## Icons and decoration

No emoji and no icon used as decoration. An icon is there when it is the icon of a module, of a card, or stands for a button the text describes.
