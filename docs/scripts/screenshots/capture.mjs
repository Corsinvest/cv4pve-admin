// SPDX-FileCopyrightText: Copyright Corsinvest Srl
// SPDX-License-Identifier: AGPL-3.0-only

// Takes the screenshots of the documentation from a running cv4pve-admin, as described in shots.json:
// the page, the actions before the shot (fill, click, wait), the part to keep and the numbered badges or
// boxes that point at the controls named in the steps of the page. The images land in
// src/assets/images/screenshots. Run it again after a change of the interface: the images are rebuilt.
//
//   node scripts/screenshots/capture.mjs [id ...]
//
// Environment: CV4PVE_DOCS_URL (default http://127.0.0.1:7260), CV4PVE_DOCS_USER and
// CV4PVE_DOCS_PASSWORD (default admin@local / Password123!), CV4PVE_DOCS_BROWSER (default msedge).
// The instance must hold test data only: what is on the screen ends up in the public documentation.
import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';

const require = createRequire(import.meta.url);
const { chromium } = require('playwright-core');

const HERE = path.dirname(fileURLToPath(import.meta.url));
const OUT = path.resolve(HERE, '..', '..', 'src', 'assets', 'images', 'screenshots');
const BASE = process.env.CV4PVE_DOCS_URL ?? 'http://127.0.0.1:7260';
const USER = process.env.CV4PVE_DOCS_USER ?? 'admin@local';
const PASSWORD = process.env.CV4PVE_DOCS_PASSWORD ?? 'Password123!';
const BROWSER = process.env.CV4PVE_DOCS_BROWSER ?? 'msedge';

const config = JSON.parse(fs.readFileSync(path.join(HERE, 'shots.json'), 'utf8'));
const only = process.argv.slice(2);
const shots = config.shots.filter(a => only.length === 0 || only.includes(a.id));

const browser = await chromium.launch({ channel: BROWSER });
const newContext = () => browser.newContext({ viewport: config.viewport, deviceScaleFactor: 2, colorScheme: 'light' });

async function login(page) {
    await page.goto(`${BASE}/Login`, { waitUntil: 'load' });
    await page.fill('input[name=Username]', USER);
    await page.fill('input[name=Password]', PASSWORD);
    await page.click('button[type=submit]');
    await page.waitForURL(a => !a.toString().toLowerCase().includes('/login'), { timeout: 30000 });
}

async function run(page, action) {
    if (action.fill) { await page.fill(action.fill, action.value ?? ''); }
    else if (action.click) { await page.click(action.click); }
    else if (action.press) { await page.keyboard.press(action.press); }
    else if (action.waitFor) { await page.waitForSelector(action.waitFor, { state: 'visible', timeout: 30000 }); }
    else if (action.wait) { await page.waitForTimeout(action.wait); }
    else { throw new Error(`Unknown action ${JSON.stringify(action)}`); }
}

// Badges and boxes are drawn in the page, on top of the controls, so that they follow the real layout.
// A field is marked as a whole (its .rz-form-field), not by the input inside it. A badge sits outside
// the control, on its left (or on the side asked with "side"), at its middle, so that it never covers
// the text or the icon of the control.
// "inside-right" puts it inside the control, at its right end, for a field with nothing on its sides.
const BADGE = 20;
const GAP = 2;

async function annotate(page, shot) {
    const marks = [
        ...(shot.boxes ?? []).map(a => ({ kind: 'box', selector: a.selector ?? a })),
        ...(shot.badges ?? []).map(a => ({ kind: 'badge', selector: a.selector, n: a.n, side: a.side ?? 'left' }))
    ];

    // Playwright resolves the selectors, so that text selectors such as button:has-text('Add') work too.
    for (const mark of marks) {
        const locator = page.locator(mark.selector).first();
        if (await locator.boundingBox() === null) { throw new Error(`${shot.id}: nothing visible matches ${mark.selector}`); }

        mark.anchor = await locator.evaluate(element => {
            const field = element.closest('.rz-form-field');
            const rect = (field ?? element).getBoundingClientRect();
            return { left: rect.left, top: rect.top, right: rect.right, bottom: rect.bottom, field: field !== null };
        });

        if (mark.kind === 'badge') {
            const a = mark.anchor;
            const cy = (a.top + a.bottom) / 2;
            const cx = mark.side === 'right' ? a.right + GAP + BADGE / 2
                     : mark.side === 'inside-right' ? a.right - 2 * GAP - BADGE / 2
                     : a.left - GAP - BADGE / 2;
            mark.rect = { left: cx - BADGE / 2, top: cy - BADGE / 2, right: cx + BADGE / 2, bottom: cy + BADGE / 2 };
        } else {
            const a = mark.anchor;
            mark.rect = { left: a.left - 4, top: a.top - 4, right: a.right + 4, bottom: a.bottom + 4 };
        }
    }

    const badges = marks.filter(a => a.kind === 'badge');
    for (const badge of badges) {
        const r = badge.rect;
        if (r.left < 0 || r.top < 0 || r.right > config.viewport.width || r.bottom > config.viewport.height) {
            throw new Error(`${shot.id}: badge ${badge.n} falls outside the page, try another side`);
        }
        for (const other of badges.filter(a => a !== badge)) {
            const o = other.rect;
            if (r.left < o.right && o.left < r.right && r.top < o.bottom && o.top < r.bottom) {
                throw new Error(`${shot.id}: badges ${badge.n} and ${other.n} overlap`);
            }
        }

        // A badge must not cover another control: buttons, fields, links, icons, labels.
        const covered = await page.evaluate(({ r, anchor }) => {
            const hit = (e) => {
                const b = e.getBoundingClientRect();
                return b.width > 0 && b.height > 0 && r.left < b.right && b.left < r.right && r.top < b.bottom && b.top < r.bottom;
            };
            const own = (e) => e.getBoundingClientRect().left >= anchor.left - 1 && e.getBoundingClientRect().right <= anchor.right + 1
                               && e.getBoundingClientRect().top >= anchor.top - 1 && e.getBoundingClientRect().bottom <= anchor.bottom + 1;
            const found = [...document.querySelectorAll('button, input:not([type=hidden]), a, .rzi, label, .rz-form-field, .rz-text-h6, h1, h2, h3, h4, h5, h6')]
                .filter(e => getComputedStyle(e).visibility !== 'hidden' && hit(e) && !own(e));
            return found.map(e => `${e.tagName.toLowerCase()} "${(e.innerText || e.getAttribute('name') || e.textContent || '').trim().slice(0, 30)}"`)[0] ?? null;
        }, { r, anchor: badge.anchor });
        if (covered) { throw new Error(`${shot.id}: badge ${badge.n} covers ${covered}, try another side`); }
    }

    await page.evaluate(items => {
        const color = '#004e8c';
        for (const item of items) {
            const r = item.rect;
            const mark = document.createElement('div');
            Object.assign(mark.style, {
                position: 'fixed', zIndex: '2147483647', pointerEvents: 'none', boxSizing: 'border-box',
                left: `${r.left}px`, top: `${r.top}px`, width: `${r.right - r.left}px`, height: `${r.bottom - r.top}px`
            });

            if (item.kind === 'box') {
                Object.assign(mark.style, {
                    border: `2px solid ${color}`, borderRadius: '6px',
                    boxShadow: '0 0 0 4px rgba(0, 78, 140, 0.18)'
                });
            } else {
                Object.assign(mark.style, {
                    borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center',
                    background: color, color: '#fff', border: '1.5px solid #fff',
                    boxShadow: '0 1px 4px rgba(0, 0, 0, 0.35)',
                    font: '600 11px/1 "Segoe UI", system-ui, sans-serif',
                    fontVariantNumeric: 'tabular-nums'
                });
                mark.textContent = String(item.n);
            }

            document.body.appendChild(mark);
        }
    }, marks.map(a => ({ kind: a.kind, n: a.n, rect: a.rect })));

    return marks.map(a => a.rect);
}

// A message (notification, alert, dialog) shown on its own: the page behind it is covered by a plain
// backdrop and the positioned box that holds the element is raised above the backdrop.
async function isolate(page, shot) {
    const target = page.locator(shot.clip[0]).first();
    if (await target.boundingBox() === null) { throw new Error(`${shot.id}: nothing visible matches ${shot.clip[0]}`); }

    await target.evaluate(element => {
        const backdrop = document.createElement('div');
        Object.assign(backdrop.style, {
            position: 'fixed', inset: '0', zIndex: '2147483000',
            background: getComputedStyle(document.body).backgroundColor || '#f5f5f5'
        });
        document.body.appendChild(backdrop);

        // Every positioned box from the element up to the body is raised: any of them can be a
        // stacking context that would keep the element under the backdrop.
        for (let holder = element; holder && holder !== document.body; holder = holder.parentElement) {
            if (getComputedStyle(holder).position !== 'static' || holder === element) {
                if (getComputedStyle(holder).position === 'static') { holder.style.position = 'relative'; }
                holder.style.zIndex = '2147483001';
            }
        }
    });

    const visible = await target.evaluate(element => {
        const rect = element.getBoundingClientRect();
        const top = document.elementFromPoint(rect.left + rect.width / 2, rect.top + rect.height / 2);
        return top !== null && (element === top || element.contains(top));
    });
    if (!visible) { throw new Error(`${shot.id}: ${shot.clip[0]} is still covered after isolate`); }
}

async function clipOf(page, shot, marks) {
    if (!shot.clip) { return undefined; }

    const rects = marks.map(a => ({ x: a.left, y: a.top, r: a.right, b: a.bottom }));
    for (const selector of shot.clip) {
        const box = await page.locator(selector).first().boundingBox();
        if (box === null) { throw new Error(`${shot.id}: nothing visible matches the clip ${selector}`); }
        rects.push({ x: box.x, y: box.y, r: box.x + box.width, b: box.y + box.height });
    }

    const pad = shot.padding ?? 16;
    const x = Math.max(0, Math.min(...rects.map(a => a.x)) - pad);
    const y = Math.max(0, Math.min(...rects.map(a => a.y)) - pad);
    return {
        x, y,
        width: Math.min(config.viewport.width, Math.max(...rects.map(a => a.r)) + pad) - x,
        height: Math.min(config.viewport.height, Math.max(...rects.map(a => a.b)) + pad) - y
    };
}

let failed = 0;
for (const shot of shots) {
    const context = await newContext();
    const page = await context.newPage();
    try {
        if (shot.auth !== false) { await login(page); }

        await page.goto(`${BASE}${shot.url}`, { waitUntil: 'load' });
        await page.waitForLoadState('networkidle', { timeout: 10000 }).catch(() => { });
        await page.waitForTimeout(shot.settle ?? 1500);
        for (const action of shot.actions ?? []) { await run(page, action); }

        // A spinner still on screen means the page is not ready: better no image than a wrong one.
        if (await page.locator('.rz-progressbar-circular, .rz-datatable-loading, .rz-progressbar-indeterminate').count() > 0) {
            await page.waitForTimeout(3000);
        }

        for (const check of shot.expect ?? []) {
            if (check.absent) {
                if (await page.locator(check.selector).count() > 0) { throw new Error(`${check.selector} should not be there`); }
                continue;
            }
            const value = await page.locator(check.selector).first().inputValue();
            if (check.value !== undefined && value !== check.value) { throw new Error(`${check.selector} is "${value}", expected "${check.value}"`); }
            if (check.notEmpty && !value) { throw new Error(`${check.selector} is empty`); }
        }

        // Notifications (the default password warning, the confirmations) would cover the subject:
        // hidden unless the shot is about a notification or keeps them on purpose.
        const aboutNotification = (shot.clip ?? []).some(a => a.includes('rz-notification'));
        if (!aboutNotification && !shot.keepNotifications) {
            await page.addStyleTag({ content: '.rz-notification, .rz-notification-item { display: none !important; }' });
            const left = await page.locator('.rz-notification-item').evaluateAll(items => items.filter(a => a.getBoundingClientRect().width > 0).length);
            if (left > 0) { throw new Error(`${shot.id}: ${left} notification(s) still on screen`); }
        }

        if (shot.isolate) { await isolate(page, shot); }
        const marks = await annotate(page, shot);

        const file = path.join(OUT, shot.file);
        fs.mkdirSync(path.dirname(file), { recursive: true });
        // A .jpg file for a shot with a photo in it: as PNG it would weigh megabytes.
        const jpeg = /\.jpe?g$/i.test(file);
        await page.screenshot({ path: file, clip: await clipOf(page, shot, marks), ...(jpeg ? { type: 'jpeg', quality: 85 } : {}) });
        console.log(`ok   ${shot.id} -> ${path.relative(process.cwd(), file)}`);
    } catch (error) {
        failed++;
        console.error(`FAIL ${shot.id} at ${page.url()}: ${error.message.split('\n')[0]}`);
    } finally {
        await context.close();
    }
}

await browser.close();
process.exit(failed === 0 ? 0 : 1);
