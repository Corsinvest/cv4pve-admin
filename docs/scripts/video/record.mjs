// SPDX-FileCopyrightText: Copyright Corsinvest Srl
// SPDX-License-Identifier: AGPL-3.0-only

// Records the presentation video of cv4pve-admin from a running instance, as described in video.json:
// a title card, the scenes (a page, the setup actions done before the recording starts, the steps shown
// with a visible cursor, a caption at the bottom) and a closing card. Each part is recorded on its own
// from the screencast of the browser (sharper than the Playwright recorder, which also needs its own
// ffmpeg download), cut to its duration and joined with crossfades. The result lands in .media-out/videos/demo.mp4,
// with one still frame per part (the middle of it) next to it. The videos are uploaded by hand.
//
//   node scripts/video/record.mjs [--music file] [--only scene-id]
//
// --music adds a soundtrack: fade in 1 s, fade out 2 s, volume -6 dB, cut to the length of the video.
// Without it the video has no audio track.
// --only records that part again (title, closing or a scene id) and reuses the other parts recorded
// before, from .media-out/videos/work.
//
// Steps: { "moveTo": selector, "nth", "ms" }, { "click": selector, "nth" }, { "type": text },
// { "wait": ms }, { "scroll": pixels }, { "waitFor": selector }. "setup" takes the same steps and runs
// them before the recording starts, without cursor. A scene with "edition": "ee" runs on the
// Enterprise instance.
//
// Environment: CV4PVE_DOCS_URL (default http://127.0.0.1:7260), CV4PVE_DOCS_URL_EE (default
// http://127.0.0.1:7271), CV4PVE_DOCS_USER and CV4PVE_DOCS_PASSWORD (default admin@local / Password123!),
// CV4PVE_DOCS_BROWSER (default msedge). Needs ffmpeg and ffprobe on PATH.
// The instance must hold test data only. The recording only reads and navigates: no step may save.
import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';

const require = createRequire(import.meta.url);
const { chromium } = require('playwright-core');

const HERE = path.dirname(fileURLToPath(import.meta.url));
const OUT = path.resolve(HERE, '..', '..', '.media-out', 'videos');
const WORK = path.join(OUT, 'work');
const BASE = process.env.CV4PVE_DOCS_URL ?? 'http://127.0.0.1:7260';
const BASE_EE = process.env.CV4PVE_DOCS_URL_EE ?? 'http://127.0.0.1:7271';
const USER = process.env.CV4PVE_DOCS_USER ?? 'admin@local';
const PASSWORD = process.env.CV4PVE_DOCS_PASSWORD ?? 'Password123!';
const BROWSER = process.env.CV4PVE_DOCS_BROWSER ?? 'msedge';

const args = process.argv.slice(2);
const argOf = name => { const i = args.indexOf(name); return i >= 0 ? args[i + 1] : undefined; };
const music = argOf('--music');
const only = argOf('--only');

const config = JSON.parse(fs.readFileSync(path.join(HERE, 'video.json'), 'utf8'));
const { width, height } = config.resolution;
const fps = config.fps ?? 30;
const fade = config.crossfade ?? 0.5;
const parts = [
    { ...config.title, card: true },
    ...config.scenes,
    { ...config.closing, card: true }
];
if (only && !parts.some(a => a.id === only)) { throw new Error(`Unknown scene ${only}`); }
if (music && !fs.existsSync(music)) { throw new Error(`Music file not found: ${music}`); }

fs.mkdirSync(WORK, { recursive: true });

function ffmpeg(list) {
    const r = spawnSync('ffmpeg', ['-hide_banner', '-loglevel', 'error', '-y', ...list], { stdio: 'inherit' });
    if (r.status !== 0) { throw new Error(`ffmpeg failed: ${list.join(' ')}`); }
}

const browser = await chromium.launch({ channel: BROWSER });
const states = new Map();

async function storageStateOf(base) {
    if (!states.has(base)) {
        const context = await browser.newContext({ viewport: { width, height } });
        const page = await context.newPage();
        await page.goto(`${base}/Login`, { waitUntil: 'load' });
        await page.fill('input[name=Username]', USER);
        await page.fill('input[name=Password]', PASSWORD);
        await page.click('button[type=submit]');
        await page.waitForURL(a => !a.toString().toLowerCase().includes('/login'), { timeout: 30000 });
        states.set(base, await context.storageState());
        await context.close();
    }
    return states.get(base);
}

// The overlay lives under <html>, outside the part of the page that Blazor renders, so a render
// does not remove it.
async function addOverlay(page, caption, duration) {
    await page.evaluate(({ caption, duration }) => {
        const root = document.createElement('div');
        root.id = 'cv-video-overlay';
        root.innerHTML = `
<style>
#cv-video-overlay { position: fixed; inset: 0; pointer-events: none; z-index: 2147483647; }
#cv-cursor { position: fixed; left: 0; top: 0; width: 26px; height: 26px; transform: translate(-3px, -2px);
  filter: drop-shadow(0 2px 3px rgba(0,0,0,.35)); }
.cv-ripple { position: fixed; width: 44px; height: 44px; margin: -22px 0 0 -22px; border-radius: 50%;
  border: 3px solid #004e8c; background: rgba(0,78,140,.18); animation: cv-ripple .6s ease-out forwards; }
@keyframes cv-ripple { from { transform: scale(.3); opacity: 1; } to { transform: scale(1.4); opacity: 0; } }
#cv-caption { position: fixed; left: 50%; bottom: 56px; transform: translate(-50%, 16px); opacity: 0;
  background: #004e8c; color: #fff; padding: 16px 36px; border-radius: 12px;
  font: 500 34px/1.2 "Segoe UI", system-ui, sans-serif; white-space: nowrap;
  box-shadow: 0 8px 24px rgba(0,0,0,.25); transition: opacity .5s ease, transform .5s ease; }
#cv-caption.show { opacity: 1; transform: translate(-50%, 0); }
</style>
<svg id="cv-cursor" viewBox="0 0 26 26"><path d="M3 2 L3 21 L8 16.5 L11.5 24 L14.5 22.7 L11 15.3 L17.5 15.3 Z"
  fill="#fff" stroke="#111" stroke-width="1.6" stroke-linejoin="round"/></svg>
<div id="cv-caption"></div>`;
        document.documentElement.appendChild(root);

        const cursor = root.querySelector('#cv-cursor');
        document.addEventListener('mousemove', e => { cursor.style.left = `${e.clientX}px`; cursor.style.top = `${e.clientY}px`; }, true);
        document.addEventListener('mousedown', e => {
            const ripple = document.createElement('div');
            ripple.className = 'cv-ripple';
            ripple.style.left = `${e.clientX}px`;
            ripple.style.top = `${e.clientY}px`;
            root.appendChild(ripple);
            setTimeout(() => ripple.remove(), 700);
        }, true);

        const bar = root.querySelector('#cv-caption');
        if (caption) {
            bar.textContent = caption;
            window.__cvStartCaption = () => {
                setTimeout(() => bar.classList.add('show'), 250);
                setTimeout(() => bar.classList.remove('show'), duration * 1000 - 700);
            };
        } else {
            bar.remove();
            window.__cvStartCaption = () => { };
        }
    }, { caption, duration });
}

const cursor = { x: 0, y: 0 };
const ease = t => t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;

async function glide(page, x, y, ms = 800) {
    const frames = Math.max(8, Math.round(ms / 16));
    const from = { ...cursor };
    for (let i = 1; i <= frames; i++) {
        const t = ease(i / frames);
        await page.mouse.move(from.x + (x - from.x) * t, from.y + (y - from.y) * t);
        await page.waitForTimeout(ms / frames);
    }
    cursor.x = x;
    cursor.y = y;
}

async function centerOf(page, step, selector) {
    const locator = page.locator(selector).nth(step.nth ?? 0);
    await locator.waitFor({ state: 'visible', timeout: 30000 });
    const box = await locator.boundingBox();
    return { x: box.x + box.width / 2 + (step.dx ?? 0), y: box.y + box.height / 2 + (step.dy ?? 0) };
}

async function run(page, step, visible) {
    if (step.moveTo) {
        const p = await centerOf(page, step, step.moveTo);
        await glide(page, p.x, p.y, step.ms);
    } else if (step.click) {
        if (visible) {
            const p = await centerOf(page, step, step.click);
            await glide(page, p.x, p.y, step.ms ?? 500);
            await page.mouse.down();
            await page.waitForTimeout(90);
            await page.mouse.up();
        } else {
            await page.locator(step.click).nth(step.nth ?? 0).click();
        }
    } else if (step.type !== undefined) {
        await page.keyboard.type(step.type, { delay: step.delay ?? 70 });
    } else if (step.wait) {
        await page.waitForTimeout(step.wait);
    } else if (step.scroll) {
        const n = 20;
        for (let i = 0; i < n; i++) {
            await page.mouse.wheel(0, step.scroll / n);
            await page.waitForTimeout(25);
        }
    } else if (step.waitFor) {
        await page.waitForSelector(step.waitFor, { state: 'visible', timeout: 30000 });
    } else {
        throw new Error(`Unknown step ${JSON.stringify(step)}`);
    }
}

// The browser screencast sends a JPEG frame for each change of the page, with its time. The frames
// start once the page is ready, so the navigation never shows; ffmpeg turns them into a constant
// frame rate, each frame held until the next one.
async function startScreencast(page, dir) {
    fs.mkdirSync(dir, { recursive: true });
    const frames = [];
    const cdp = await page.context().newCDPSession(page);
    cdp.on('Page.screencastFrame', frame => {
        const file = path.join(dir, `${String(frames.length).padStart(5, '0')}.jpg`);
        fs.writeFileSync(file, Buffer.from(frame.data, 'base64'));
        frames.push({ file, time: frame.metadata.timestamp });
        cdp.send('Page.screencastFrameAck', { sessionId: frame.sessionId }).catch(() => { });
    });
    await cdp.send('Page.startScreencast', { format: 'jpeg', quality: 95, maxWidth: width, maxHeight: height, everyNthFrame: 1 });
    return { frames, stop: () => cdp.send('Page.stopScreencast').catch(() => { }) };
}

function encode(frames, from, duration, dir, segment) {
    const to = from + duration;
    let first = frames.findLastIndex(a => a.time <= from);
    if (first < 0) { first = 0; }
    const used = frames.slice(first).filter(a => a.time < to);
    if (used.length === 0) { throw new Error(`No frames in ${segment}`); }

    const lines = [];
    used.forEach((frame, i) => {
        const start = Math.max(frame.time, from);
        const end = i + 1 < used.length ? used[i + 1].time : to;
        lines.push(`file '${path.basename(frame.file)}'`, `duration ${Math.max(end - start, 0.001).toFixed(4)}`);
    });
    lines.push(`file '${path.basename(used.at(-1).file)}'`);
    const list = path.join(dir, 'frames.txt');
    fs.writeFileSync(list, lines.join('\n'));

    ffmpeg(['-f', 'concat', '-safe', '0', '-i', list, '-t', String(duration),
        '-vf', `fps=${fps},scale=${width}:${height}:out_range=tv,format=yuv420p`,
        '-c:v', 'libx264', '-preset', 'veryfast', '-crf', '14', '-an', segment]);
    return used.length;
}

async function record(part) {
    const base = part.edition === 'ee' ? BASE_EE : BASE;
    const dir = path.join(WORK, `raw-${part.id}`);
    fs.rmSync(dir, { recursive: true, force: true });
    const context = await browser.newContext({
        viewport: { width, height },
        deviceScaleFactor: 1,
        colorScheme: 'light',
        storageState: part.card ? undefined : await storageStateOf(base)
    });
    const page = await context.newPage();

    if (part.card) {
        const texts = { title: part.title, subtitle: part.subtitle, footer: part.footer };
        const html = fs.readFileSync(path.join(HERE, 'card.html'), 'utf8')
            .replace('<script>', `<script>window.__card = ${JSON.stringify(texts)};</script><script>`);
        await page.setContent(html, { waitUntil: 'load' });
        await page.waitForTimeout(300);
    } else {
        await page.goto(`${base}${part.url}`, { waitUntil: 'load' });
        await page.waitForLoadState('networkidle', { timeout: 10000 }).catch(() => { });
        await page.waitForTimeout(part.settle ?? 2000);
        if (page.url().toLowerCase().includes('/login')) { throw new Error(`${part.id}: not logged in`); }
        // The default password warning and the other notifications would cover the subject
        await page.addStyleTag({ content: '.rz-notification, .rz-notification-item { display: none !important; }' });
        for (const step of part.setup ?? []) { await run(page, step, false); }
        await addOverlay(page, part.caption, part.duration);
        cursor.x = width * 0.62;
        cursor.y = height * 0.55;
        await page.mouse.move(cursor.x, cursor.y);
    }

    const screencast = await startScreencast(page, dir);
    await page.waitForTimeout(300);
    const from = Date.now() / 1000;
    if (part.card) {
        await page.evaluate(() => document.body.classList.add('play'));
    } else {
        await page.evaluate(() => window.__cvStartCaption());
        for (const step of part.steps ?? []) { await run(page, step, true); }
    }

    const left = part.duration * 1000 + 300 - (Date.now() - from * 1000);
    if (left > 0) { await page.waitForTimeout(left); }
    else { console.warn(`${part.id}: the steps take ${(-left / 1000).toFixed(1)} s more than the duration`); }
    await screencast.stop();
    await context.close();

    const count = encode(screencast.frames, from, part.duration, dir, path.join(WORK, `${part.id}.mp4`));
    fs.rmSync(dir, { recursive: true, force: true });
    console.log(`${part.id}: ${count} frames in ${part.duration} s`);
}

try {
    for (const part of parts) {
        const segment = path.join(WORK, `${part.id}.mp4`);
        if (only ? part.id === only || !fs.existsSync(segment) : true) { await record(part); }
    }
} finally {
    await browser.close();
}

// Assembly: each crossfade overlaps the end of a part with the start of the next one.
const starts = [];
for (let i = 0, t = 0; i < parts.length; i++) { starts.push(t); t += parts[i].duration - fade; }
const total = parts.reduce((sum, a) => sum + a.duration, 0) - (parts.length - 1) * fade;

const output = path.join(OUT, config.output ?? 'demo.mp4');
const inputs = parts.flatMap(a => ['-i', path.join(WORK, `${a.id}.mp4`)]);
const filters = [];
let last = '0:v';
for (let i = 1; i < parts.length; i++) {
    const out = `v${i}`;
    filters.push(`[${last}][${i}:v]xfade=transition=fade:duration=${fade}:offset=${starts[i].toFixed(3)}[${out}]`);
    last = out;
}
filters.push(`[${last}]scale=out_range=tv,format=yuv420p[vout]`);

const audio = [];
if (music) {
    inputs.push('-i', music);
    filters.push(`[${parts.length}:a]atrim=0:${total.toFixed(3)},asetpts=PTS-STARTPTS,volume=-6dB,`
        + `afade=t=in:st=0:d=1,afade=t=out:st=${(total - 2).toFixed(3)}:d=2[aout]`);
    audio.push('-map', '[aout]', '-c:a', 'aac', '-b:a', '192k');
} else {
    audio.push('-an');
}

ffmpeg([...inputs, '-filter_complex', filters.join(';'), '-map', '[vout]', ...audio,
    '-r', String(fps), '-c:v', 'libx264', '-preset', 'slow', '-crf', '18', '-pix_fmt', 'yuv420p',
    '-movflags', '+faststart', '-t', total.toFixed(3), output]);

parts.forEach((part, i) => {
    const middle = starts[i] + part.duration / 2;
    ffmpeg(['-ss', middle.toFixed(3), '-i', output, '-frames:v', '1',
        path.join(OUT, `still-${String(i + 1).padStart(2, '0')}-${part.id}.png`)]);
});

console.log(`${output}: ${total.toFixed(1)} s`);
