// SPDX-FileCopyrightText: Copyright Corsinvest Srl
// SPDX-License-Identifier: AGPL-3.0-only

// Checks the documentation: the rules of CONTRIBUTING.md on the source pages, then links, anchors
// and sidebar on the built site. Run it with `npm run check` (it builds first), or after a build
// with `node scripts/check.mjs [dist folder]`. It ends with an error code when it finds a problem.
// The base path is the one of astro.config.mjs: DOCS_BASE, default `cv4pve-admin`.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const PAGES = path.join(ROOT, 'src', 'content', 'docs');
const DIST = path.resolve(ROOT, process.argv[2] ?? 'dist');
const BASE = '/' + (process.env.DOCS_BASE ?? 'cv4pve-admin').replace(/^\/+/, '');

const problems = [];
const problem = (rule, where, detail = '') => problems.push({ rule, where, detail });
const walk = (dir, test) =>
  fs.readdirSync(dir, { withFileTypes: true }).flatMap((e) => {
    const full = path.join(dir, e.name);
    return e.isDirectory() ? walk(full, test) : test(e.name) ? [full] : [];
  });
const slash = (p) => p.split(path.sep).join('/');

// ---------- Source pages

const MAX_ASIDES = 2;
// En dash and em dash, by code point so this file holds neither.
const DASHES = new RegExp(`[${String.fromCharCode(0x2013, 0x2014)}]`);
let pages = 0;
for (const file of walk(PAGES, (n) => n.endsWith('.mdx') || n.endsWith('.md'))) {
  const rel = slash(path.relative(PAGES, file));
  const source = fs.readFileSync(file, 'utf8').split('\r\n').join('\n');
  const frontmatter = source.match(/^---\n([\s\S]*?)\n---/)?.[1] ?? '';
  const draft = /^draft: true$/m.test(frontmatter);
  const body = source.replace(/^---\n[\s\S]*?\n---/, '');
  const prose = body.replace(/```[\s\S]*?```/g, '');
  pages++;

  if (!/^title:/m.test(frontmatter)) problem('title in the frontmatter', rel);
  if (!draft && rel !== 'index.mdx' && !/^description:/m.test(frontmatter)) problem('description in the frontmatter', rel);

  source.split('\n').forEach((line, i) => {
    const at = `${rel}:${i + 1}`;
    if (DASHES.test(line)) problem('no em dash or en dash', at);
    if (/\b(TODO|FIXME|TBD)\b|lorem ipsum/i.test(line)) problem('no placeholder text', at, line.trim().slice(0, 60));
    if (/!\[\]\(/.test(line)) problem('image with alternative text', at);
    if (/\[[^\]]*\]\(\s*\)/.test(line)) problem('link with an address', at);
    if (/<!--/.test(line)) problem('no HTML comment in MDX', at);
  });

  // Headings: no H1 (the title is in the frontmatter), no skipped level, no repeated text.
  const headings = [...prose.matchAll(/^(#{1,6}) (.+)$/gm)].map((m) => ({ level: m[1].length, text: m[2].replace(/<[^>]+>/g, '').trim() }));
  if (headings.some((h) => h.level === 1)) problem('no H1 in the body', rel);
  headings.forEach((h, i) => {
    if (i && h.level > headings[i - 1].level + 1) problem('no skipped heading level', rel, `"${headings[i - 1].text}" then "${h.text}"`);
  });
  for (const text of new Set(headings.map((h) => h.text).filter((t, i, all) => all.indexOf(t) !== i))) problem('no repeated heading in a page', rel, text);

  // Asides: few, never one inside or right after another, no tip.
  const asides = [...prose.matchAll(/^(:{3,})(note|tip|caution|danger)\b/gm)];
  if (asides.length > MAX_ASIDES) problem(`at most ${MAX_ASIDES} asides in a page`, rel, `${asides.length} found`);
  if (asides.some((a) => a[2] === 'tip')) problem('no :::tip aside', rel);
  if (asides.some((a) => a[1].length > 3)) problem('no aside inside another', rel);
  if (/^:::\n\n?:::(note|tip|caution|danger)/m.test(prose)) problem('no aside right after another', rel);

  // Cards: an icon on each. The line under the title: only with a badge.
  for (const m of prose.matchAll(/^\s*\{ title: ("(?:[^"\\]|\\.)*")/gm)) problem('icon on every card', rel, m[1]);
  const meta = prose.match(/^<p class="cv-page-meta">(.*)<\/p>$/m);
  if (meta && !/<Tag\b/.test(meta[1])) problem('line under the title only with a badge', rel);
  if (/<(FeatureCard|EditionBox)\b|class="cv-cards?"/.test(prose)) problem('cards are FeatureGrid', rel);
}

// ---------- Built site

if (!fs.existsSync(DIST)) {
  problem('built site', slash(path.relative(ROOT, DIST)), 'not found: run `npm run build` first');
} else {
  const files = walk(DIST, (n) => n.endsWith('.html'));
  const urlOf = (file) => {
    const dir = slash(path.relative(DIST, path.dirname(file)));
    return `${BASE}/${dir ? `${dir}/` : ''}`;
  };
  const html = new Map(files.map((f) => [f, fs.readFileSync(f, 'utf8')]));
  const ids = new Map([...html].map(([f, h]) => [urlOf(f), new Set([...h.matchAll(/\sid="([^"]+)"/g)].map((m) => m[1]))]));

  let links = 0;
  const inSidebar = new Set();
  for (const [file, page] of html) {
    if (file.endsWith('404.html')) continue;
    const from = urlOf(file);
    const here = new URL(from, 'https://site');
    const sidebarStart = page.indexOf('id="starlight__sidebar"');
    const sidebarEnd = sidebarStart < 0 ? -1 : page.indexOf('</nav>', sidebarStart);
    for (const m of page.matchAll(/<a\s[^>]*href="([^"]+)"/g)) {
      const href = m[1].replace(/&amp;/g, '&');
      if (/^([a-z][a-z0-9+.-]*:|#$)/i.test(href)) continue;
      const url = new URL(href, here);
      if (url.origin !== 'https://site') continue;
      links++;
      let target = decodeURIComponent(url.pathname);
      if (!target.startsWith(BASE)) {
        problem('link inside the site base', from, href);
        continue;
      }
      if (/\.[a-z0-9]+$/i.test(target)) {
        if (!fs.existsSync(path.join(DIST, target.slice(BASE.length)))) problem('linked file exists', from, href);
        continue;
      }
      if (!target.endsWith('/')) target += '/';
      if (m.index > sidebarStart && m.index < sidebarEnd) inSidebar.add(target);
      const anchors = ids.get(target);
      if (!anchors) problem('linked page exists', from, href);
      else if (url.hash.length > 1 && !anchors.has(decodeURIComponent(url.hash.slice(1)))) problem('linked section exists', from, href);
    }
    // Aside or tab syntax left as text: a block that was not closed or not indented.
    const text = page.slice(page.indexOf('<main')).replace(/<(script|style|pre|code)[\s\S]*?<\/\1>/g, '').replace(/<[^>]+>/g, ' ');
    if (/(^|\s):::/.test(text)) problem('no unrendered ::: in the page', from);
  }

  // Every page can be reached from the sidebar (the home page is the site title).
  for (const file of files) {
    const url = urlOf(file);
    if (file.endsWith('404.html') || url === `${BASE}/`) continue;
    if (!inSidebar.has(url)) problem('page in the sidebar', url);
  }
  console.log(`${pages} source pages, ${files.length} built pages, ${links} internal links (base ${BASE})`);
}

// ---------- Result

if (!problems.length) {
  console.log('No problems.');
} else {
  const byRule = new Map();
  for (const p of problems) byRule.set(p.rule, [...(byRule.get(p.rule) ?? []), p]);
  for (const [rule, list] of byRule) {
    console.log(`\n${rule} (${list.length})`);
    for (const p of list) console.log(`  ${p.where}${p.detail ? `: ${p.detail}` : ''}`);
  }
  console.log(`\n${problems.length} problems.`);
  process.exit(1);
}
