#!/usr/bin/env node
// Generates the "event order" diagrams referenced from README_JA.md.
//
//   node Docs/Assets/src/build-event-order-diagrams.mjs
//   Docs/Assets/src/render.sh
//
// The step lists below mirror the actual implementation. When the navigation
// flow changes, update the data here and re-run both commands.
//
//   Push   -> Meek.MVP/Runtime/MvpNavigator.cs:113-147
//   Pop    -> Meek.MVP/Runtime/MvpNavigator.cs:149-184
//   Remove -> Meek.MVP/Runtime/MvpNavigator.cs:186-216
//   Insert -> Meek.MVP/Runtime/MvpNavigator.cs:218-254
//   ScreenWillNavigate / ScreenDidNavigate ordering -> MvpNavigator.cs:59-105
//   Event emission -> Meek.NavigationStack/Runtime/Screen/StackScreen.cs

import { writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const OUT_DIR = dirname(fileURLToPath(import.meta.url));

// ---------------------------------------------------------------- theme ----

const C = {
  bg: '#22252A',
  laneRule: '#31353D',
  laneLabel: '#8B95A3',
  laneSub: '#5E6773',
  rail: '#343941',
  badgeFill: '#343941',
  badgeText: '#B7C0CC',
  title: '#F2F5F8',
  subtitle: '#8B95A3',
  panel: '#282C32',
  panelEdge: '#343941',
};

// tier -> visual weight. `req` and `life` are the ones a user actually writes
// code against, so they are the largest and the most saturated.
const TIER = {
  req: { fill: '#1F6FEB', edge: '#4A90F5', text: '#FFFFFF', note: '#CBDEFF', h: 82, fs: 27, ns: 17, bold: true },
  life: { fill: '#1E9C63', edge: '#3FC286', text: '#FFFFFF', note: '#C7EEDA', h: 82, fs: 27, ns: 17, bold: true },
  know: { fill: '#584A8C', edge: '#7A69B8', text: '#EDE7FF', note: '#B7ACD9', h: 64, fs: 22, ns: 16, bold: false },
  internal: { fill: '#2C3037', edge: '#3C424B', text: '#7E8794', note: '#666E7A', h: 46, fs: 18, ns: 15, bold: false },
  callout: { fill: '#38301F', edge: '#7E6526', text: '#E9B84A', note: '#E9B84A', h: 58, fs: 19, ns: 19, bold: false },
};

// ------------------------------------------------------------- geometry ----

const W = 1560;
const RAIL_X = 46;
const LANES = {
  a: { x: 86, w: 476 },
  core: { x: 582, w: 386 },
  b: { x: 988, w: 532 },
};
const FULL = { x: 86, w: 1434 };

const PAD_TOP = 44;
const ROW_GAP = 13;
const PAD_BOTTOM = 40;

const MONO = 'Menlo, "SF Mono", "DejaVu Sans Mono", monospace';
const SANS = '"Helvetica Neue", Helvetica, Arial, sans-serif';
const MONO_ADV = 0.6021; // Menlo advance width per em
const SANS_ADV = 0.55; // conservative average for mixed-case English

// ---------------------------------------------------------------- utils ----

const esc = (s) =>
  String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');

function wrap(text, maxWidth, fontSize, adv) {
  const max = Math.max(8, Math.floor(maxWidth / (fontSize * adv)));
  const lines = [];
  let line = '';
  for (const word of String(text).split(' ')) {
    const candidate = line ? `${line} ${word}` : word;
    if (candidate.length <= max) {
      line = candidate;
      continue;
    }
    if (line) lines.push(line);
    if (word.length <= max) {
      line = word;
    } else {
      let rest = word;
      while (rest.length > max) {
        lines.push(rest.slice(0, max));
        rest = rest.slice(max);
      }
      line = rest;
    }
  }
  if (line) lines.push(line);
  return lines;
}

const syncPillWidth = (label) => Math.round(label.length * 15 * SANS_ADV) + 26;

// --------------------------------------------------------------- layout ----

function measure(item) {
  const t = TIER[item.tier];
  const lane = item.lane === 'full' ? FULL : LANES[item.lane];
  const padX = item.tier === 'internal' ? 16 : 20;
  const syncW = item.sync ? syncPillWidth(item.sync) + 12 : 0;

  const titleW = lane.w - padX * 2 - syncW;

  // Shrink a long API name a little rather than wrapping it across two lines.
  let fs = t.fs;
  if (item.tier !== 'callout') {
    while (fs > t.fs - 6 && item.title.length * fs * MONO_ADV > titleW) fs -= 1;
  }

  const titleLines =
    item.tier === 'callout'
      ? wrap(item.title, lane.w - padX * 2 - 30, fs, SANS_ADV)
      : wrap(item.title, titleW, fs, MONO_ADV);
  const noteLines = item.note ? wrap(item.note, lane.w - padX * 2, t.ns, SANS_ADV) : [];

  const titleBlock = titleLines.length * (fs + 6);
  const noteBlock = noteLines.length ? noteLines.length * (t.ns + 5) + 6 : 0;
  const h = Math.max(t.h, titleBlock + noteBlock + (item.tier === 'internal' ? 18 : 26));

  return { t, fs, lane, padX, titleLines, noteLines, h };
}

// --------------------------------------------------------------- render ----

function node(item, y, m) {
  const { t, fs, lane, padX, titleLines, noteLines, h } = m;
  const r = item.tier === 'internal' ? 9 : item.tier === 'know' ? 12 : 14;
  const out = [];

  out.push(
    `<rect x="${lane.x}" y="${y}" width="${lane.w}" height="${h}" rx="${r}" fill="${t.fill}" stroke="${t.edge}" stroke-width="${item.tier === 'internal' || item.tier === 'callout' ? 1 : 0}"/>`
  );

  const titleBlock = titleLines.length * (fs + 6);
  const noteBlock = noteLines.length ? noteLines.length * (t.ns + 5) + 6 : 0;
  let ty = y + (h - titleBlock - noteBlock) / 2 + fs;

  if (item.tier === 'callout') {
    out.push(
      `<text x="${lane.x + padX}" y="${ty}" font-family="${SANS}" font-size="${fs}" font-weight="700" fill="${t.text}">★</text>`
    );
    for (const line of titleLines) {
      out.push(
        `<text x="${lane.x + padX + 30}" y="${ty}" font-family="${SANS}" font-size="${fs}" fill="${t.text}">${esc(line)}</text>`
      );
      ty += fs + 6;
    }
    return out.join('\n');
  }

  for (const line of titleLines) {
    out.push(
      `<text x="${lane.x + padX}" y="${ty}" font-family="${MONO}" font-size="${fs}" font-weight="${t.bold ? 700 : 500}" fill="${t.text}">${esc(line)}</text>`
    );
    ty += fs + 6;
  }

  if (noteLines.length) {
    ty += 4;
    for (const line of noteLines) {
      out.push(
        `<text x="${lane.x + padX}" y="${ty}" font-family="${SANS}" font-size="${t.ns}" fill="${t.note}">${esc(line)}</text>`
      );
      ty += t.ns + 5;
    }
  }

  if (item.sync) {
    const pw = syncPillWidth(item.sync);
    const px = lane.x + lane.w - padX - pw;
    const ph = 26;
    const py = y + 14;
    out.push(
      `<rect x="${px}" y="${py}" width="${pw}" height="${ph}" rx="${ph / 2}" fill="rgba(0,0,0,0.26)"/>` +
        `<text x="${px + pw / 2}" y="${py + 18}" text-anchor="middle" font-family="${SANS}" font-size="15" fill="rgba(255,255,255,0.8)">${esc(item.sync)}</text>`
    );
  }

  return out.join('\n');
}

function build(diagram) {
  const { title, subtitle, lanes, steps, footer } = diagram;

  // ---- header -------------------------------------------------------------
  const head = [];
  head.push(
    `<text x="46" y="70" font-family="${SANS}" font-size="40" font-weight="700" fill="${C.title}">${esc(title)}</text>`
  );
  // keep the subtitle clear of the legend panel on the right
  wrap(subtitle, W - 46 - 490 - 46 - 30, 20, SANS_ADV).forEach((line, i) => {
    head.push(
      `<text x="46" y="${106 + i * 27}" font-family="${SANS}" font-size="20" fill="${C.subtitle}">${esc(line)}</text>`
    );
  });

  // legend
  const legend = [
    ['#1F6FEB', '#4A90F5', 'You implement this'],
    ['#1E9C63', '#3FC286', 'Lifecycle event you hook'],
    ['#584A8C', '#7A69B8', 'Good to know'],
    ['#31363E', '#474D57', 'Meek internals (FYI)'],
  ];
  const syncNote = [
    'sync — Action handlers only',
    'sync → async — every Action handler runs, then each',
    'Func<Task> handler is awaited in registration order',
  ];
  const lgW = 490;
  const lgX = W - 46 - lgW;
  const lgH = 26 + legend.length * 31 + 14 + syncNote.length * 22 + 12;
  head.push(
    `<rect x="${lgX}" y="34" width="${lgW}" height="${lgH}" rx="12" fill="${C.panel}" stroke="${C.panelEdge}"/>`
  );
  legend.forEach(([fill, edge, label], i) => {
    const y = 34 + 24 + i * 31;
    head.push(
      `<rect x="${lgX + 20}" y="${y}" width="24" height="17" rx="5" fill="${fill}" stroke="${edge}"/>` +
        `<text x="${lgX + 56}" y="${y + 15}" font-family="${SANS}" font-size="17" fill="#A9B2BE">${esc(label)}</text>`
    );
  });
  const noteTop = 34 + 26 + legend.length * 31 + 14;
  head.push(
    `<line x1="${lgX + 20}" y1="${noteTop - 12}" x2="${lgX + lgW - 20}" y2="${noteTop - 12}" stroke="${C.panelEdge}"/>`
  );
  syncNote.forEach((line, i) => {
    head.push(
      `<text x="${lgX + 20}" y="${noteTop + 12 + i * 22}" font-family="${SANS}" font-size="15" fill="#727B87">${esc(line)}</text>`
    );
  });

  const headerBottom = Math.max(140, 34 + lgH) + 26;

  // ---- lane headers -------------------------------------------------------
  const laneHead = [];
  let laneY = headerBottom + 30;
  for (const key of ['a', 'core', 'b']) {
    const lane = LANES[key];
    const meta = lanes[key];
    laneHead.push(
      `<text x="${lane.x}" y="${laneY}" font-family="${SANS}" font-size="21" font-weight="700" fill="${C.laneLabel}">${esc(meta.label)}</text>`
    );
    if (meta.sub) {
      laneHead.push(
        `<text x="${lane.x}" y="${laneY + 24}" font-family="${SANS}" font-size="16" fill="${C.laneSub}">${esc(meta.sub)}</text>`
      );
    }
  }
  const contentTop = laneY + 46;

  // ---- rows ---------------------------------------------------------------
  const body = [];
  const badges = [];
  let y = contentTop + PAD_TOP;
  let n = 0;
  let firstCy = null;
  let lastCy = null;

  for (const item of steps) {
    const m = measure(item);
    body.push(node(item, y, m));

    if (item.tier !== 'callout') {
      n += 1;
      const cy = y + m.h / 2;
      if (firstCy === null) firstCy = cy;
      lastCy = cy;
      badges.push(
        `<line x1="${RAIL_X + 15}" y1="${cy}" x2="${m.lane.x - 6}" y2="${cy}" stroke="${C.rail}" stroke-width="1.5" stroke-dasharray="3 5"/>` +
          `<circle cx="${RAIL_X}" cy="${cy}" r="15" fill="${C.badgeFill}" stroke="#464C56"/>` +
          `<text x="${RAIL_X}" y="${cy + 6}" text-anchor="middle" font-family="${SANS}" font-size="16" font-weight="700" fill="${C.badgeText}">${n}</text>`
      );
    }
    y += m.h + ROW_GAP;
  }

  const lastNodeBottom = y - ROW_GAP;
  const contentBottom = lastNodeBottom + 34;

  // footer note
  let footerBlock = '';
  if (footer) {
    const lines = wrap(footer, FULL.w - 40, 18, SANS_ADV);
    const fh = lines.length * 25 + 26;
    footerBlock =
      `<rect x="${FULL.x}" y="${contentBottom}" width="${FULL.w}" height="${fh}" rx="10" fill="${C.panel}" stroke="${C.panelEdge}"/>` +
      lines
        .map(
          (line, i) =>
            `<text x="${FULL.x + 20}" y="${contentBottom + 32 + i * 25}" font-family="${SANS}" font-size="18" fill="#98A2AF">${esc(line)}</text>`
        )
        .join('\n');
    y = contentBottom + fh;
  } else {
    y = contentBottom;
  }

  const H = Math.round(y + PAD_BOTTOM);

  // lane rules + rail drawn behind the rows
  const rules = [];
  for (const key of ['core', 'b']) {
    const x = LANES[key].x - 18;
    rules.push(
      `<line x1="${x}" y1="${contentTop - 44}" x2="${x}" y2="${lastNodeBottom + 8}" stroke="${C.laneRule}" stroke-width="1"/>`
    );
  }
  rules.push(
    `<line x1="${RAIL_X}" y1="${firstCy}" x2="${RAIL_X}" y2="${lastCy}" stroke="${C.rail}" stroke-width="2"/>`
  );

  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}">
<rect width="${W}" height="${H}" fill="${C.bg}"/>
${head.join('\n')}
${laneHead.join('\n')}
${rules.join('\n')}
${badges.join('\n')}
${body.join('\n')}
${footerBlock}
</svg>`;

  return { svg, W, H };
}

// ------------------------------------------------------------------ data ----

const CORE = { label: 'Meek core', sub: 'navigator / service' };

const diagrams = {
  PushEventOrder: {
    title: 'Push — event order',
    subtitle: 'PushNavigation.PushAsync<ScreenB>()  ·  Screen A → Screen B',
    lanes: {
      a: { label: 'Screen A', sub: 'the screen being covered' },
      core: CORE,
      b: { label: 'Screen B', sub: 'the screen being pushed' },
    },
    steps: [
      { lane: 'core', tier: 'internal', title: 'IInputLocker.LockInput()', note: 'blocks input for the whole navigation' },
      { lane: 'core', tier: 'know', title: 'OnWillNavigate', note: 'StackNavigationService global hook' },
      { lane: 'a', tier: 'life', title: 'ScreenWillPause', sync: 'sync' },
      { lane: 'a', tier: 'internal', title: 'UI.LockInteractable()', note: 'Screen A stays locked until a Pop resumes it' },
      { lane: 'b', tier: 'internal', title: 'Initialize()', note: 'resolves AppServices / ScreenUI' },
      { lane: 'a', tier: 'life', title: 'ScreenDidPause', sync: 'sync → async' },
      { lane: 'full', tier: 'callout', title: "Screen A's ScreenDidPause fires before Screen B's CreateModelAsync()." },
      { lane: 'b', tier: 'know', title: 'GetNextScreenParameter<TParam>()', note: 'MVPScreen<TModel, TParam> only' },
      { lane: 'b', tier: 'req', title: 'CreateModelAsync()', note: 'you implement this · the returned Model is disposed with the screen' },
      { lane: 'b', tier: 'req', title: 'RegisterEvents(eventHolder, model)', note: 'you implement this · every handler below is registered here' },
      { lane: 'b', tier: 'internal', title: 'UI.LockInteractable()' },
      { lane: 'b', tier: 'life', title: 'ScreenWillStart', sync: 'sync → async', note: 'await LoadPresenterAsync<T>() here → instantiates the prefab, Presenter.OnInit, LoadAsync(model)' },
      { lane: 'b', tier: 'know', title: 'UI.Setup()', note: 'evaluates the Open animation at t = 0, then Presenter.OnSetup(model) → Bind(model)' },
      { lane: 'core', tier: 'internal', title: 'ScreenContainer.NavigateAsync()', note: 'pushes B onto the stack, then SortOrderInHierarchy()' },
      { lane: 'b', tier: 'know', title: 'ViewWillOpen', sync: 'sync → async' },
      { lane: 'core', tier: 'know', title: 'Transition animation', note: 'A: Hide → B: Open · parallel when IsCrossFade' },
      { lane: 'b', tier: 'know', title: 'ViewDidOpen', sync: 'sync → async' },
      { lane: 'b', tier: 'internal', title: 'interactableLock.Dispose()' },
      { lane: 'b', tier: 'life', title: 'ScreenDidStart', sync: 'sync' },
      { lane: 'core', tier: 'know', title: 'OnDidNavigate' },
      { lane: 'core', tier: 'internal', title: 'locker.Dispose()', note: 'input unlocked' },
    ],
  },

  PopEventOrder: {
    title: 'Pop — event order',
    subtitle: 'PopNavigation.PopAsync()  ·  Screen B → Screen A',
    lanes: {
      a: { label: 'Screen A', sub: 'the screen being resumed' },
      core: CORE,
      b: { label: 'Screen B', sub: 'the screen being popped' },
    },
    steps: [
      { lane: 'core', tier: 'internal', title: 'IInputLocker.LockInput()', note: 'blocks input for the whole navigation' },
      { lane: 'core', tier: 'know', title: 'OnWillNavigate', note: 'StackNavigationService global hook' },
      { lane: 'b', tier: 'life', title: 'ScreenWillDestroy', sync: 'sync → async', note: 'Model and Presenter are still alive here' },
      { lane: 'full', tier: 'callout', title: 'ScreenWillDestroy fires before the close animation and before DisposeAsync().' },
      { lane: 'b', tier: 'internal', title: '_interactableLocks.Clear()', note: 'force-unlocked when AutoDisposeLockerOnDestroy is true' },
      { lane: 'a', tier: 'life', title: 'ScreenWillResume', sync: 'sync → async' },
      { lane: 'core', tier: 'internal', title: 'ScreenContainer.NavigateAsync()', note: 'pops B off the stack' },
      { lane: 'b', tier: 'know', title: 'ViewWillClose', sync: 'sync → async' },
      { lane: 'core', tier: 'know', title: 'Transition animation', note: 'B: Close / A: Show · parallel when IsCrossFade' },
      { lane: 'b', tier: 'know', title: 'ViewDidClose', sync: 'sync → async' },
      { lane: 'b', tier: 'know', title: 'DisposeAsync()', note: 'destroys the prefab → Presenter.OnDeinit → disposes Model and Disposables' },
      { lane: 'a', tier: 'internal', title: 'interactableLock.Dispose()' },
      { lane: 'a', tier: 'life', title: 'ScreenDidResume', sync: 'sync' },
      { lane: 'core', tier: 'know', title: 'OnDidNavigate' },
      { lane: 'core', tier: 'internal', title: 'locker.Dispose()', note: 'input unlocked' },
    ],
  },

  InsertEventOrder: {
    title: 'Insert — event order',
    subtitle: 'InsertNavigation.InsertScreenBeforeAsync<ScreenB, ScreenX>()  ·  X is spliced in below the top screen',
    lanes: {
      a: { label: 'Screen C', sub: 'the current top screen — stays on top' },
      core: CORE,
      b: { label: 'Screen X', sub: 'the screen being inserted' },
    },
    steps: [
      { lane: 'core', tier: 'internal', title: 'IInputLocker.LockInput()', note: 'blocks input for the whole navigation' },
      { lane: 'core', tier: 'know', title: 'OnWillNavigate', note: 'StackNavigationService global hook' },
      { lane: 'b', tier: 'internal', title: 'Initialize()', note: 'resolves AppServices / ScreenUI' },
      { lane: 'b', tier: 'know', title: 'GetNextScreenParameter<TParam>()', note: 'MVPScreen<TModel, TParam> only' },
      { lane: 'b', tier: 'req', title: 'CreateModelAsync()', note: 'you implement this' },
      { lane: 'b', tier: 'req', title: 'RegisterEvents(eventHolder, model)', note: 'you implement this' },
      { lane: 'b', tier: 'internal', title: 'UI.LockInteractable()', note: 'released later, once X becomes the top screen' },
      { lane: 'b', tier: 'life', title: 'ScreenWillStart', sync: 'sync → async', note: 'await LoadPresenterAsync<T>() here' },
      { lane: 'b', tier: 'know', title: 'UI.Setup()', note: 'evaluates the Open animation at t = 0, then Presenter.OnSetup(model) → Bind(model)' },
      { lane: 'b', tier: 'life', title: 'ScreenDidPause', sync: 'sync → async' },
      { lane: 'full', tier: 'callout', title: 'ScreenWillPause never fires on Insert. The order is ScreenWillStart → ScreenDidPause → ScreenDidStart.' },
      { lane: 'core', tier: 'internal', title: 'ScreenContainer.NavigateAsync()', note: 'splices X in below Screen C, then SortOrderInHierarchy()' },
      { lane: 'a', tier: 'know', title: 'ViewWillOpen', sync: 'sync → async' },
      { lane: 'full', tier: 'callout', title: 'The View events fire on the top screen C — not on the inserted screen X.' },
      { lane: 'core', tier: 'know', title: 'InsertNavigatorAnimationStrategy', note: 'visibility fix-up only — no animation is played' },
      { lane: 'a', tier: 'know', title: 'ViewDidOpen', sync: 'sync → async' },
      { lane: 'b', tier: 'life', title: 'ScreenDidStart', sync: 'sync' },
      { lane: 'core', tier: 'know', title: 'OnDidNavigate' },
      { lane: 'core', tier: 'internal', title: 'locker.Dispose()', note: 'input unlocked' },
    ],
    footer: 'Inserting before the current top screen logs a warning and falls back to a full Push.',
  },

  RemoveEventOrder: {
    title: 'Remove — event order',
    subtitle: 'RemoveNavigation.RemoveAsync<ScreenX>()  ·  X is spliced out from below the top screen',
    lanes: {
      a: { label: 'Screen C', sub: 'the current top screen — no events at all' },
      core: CORE,
      b: { label: 'Screen X', sub: 'the screen being removed' },
    },
    steps: [
      { lane: 'core', tier: 'internal', title: 'IInputLocker.LockInput()', note: 'blocks input for the whole navigation' },
      { lane: 'core', tier: 'know', title: 'OnWillNavigate', note: 'StackNavigationService global hook' },
      { lane: 'b', tier: 'life', title: 'ScreenWillDestroy', sync: 'sync → async', note: 'Model and Presenter are still alive here' },
      { lane: 'full', tier: 'callout', title: 'ScreenWillDestroy fires here exactly as it does on Pop — before the disposal.' },
      { lane: 'b', tier: 'internal', title: '_interactableLocks.Clear()', note: 'force-unlocked when AutoDisposeLockerOnDestroy is true' },
      { lane: 'core', tier: 'internal', title: 'ScreenContainer.NavigateAsync()', note: 'splices X out of the stack' },
      { lane: 'b', tier: 'know', title: 'ViewWillClose', sync: 'sync → async' },
      { lane: 'core', tier: 'know', title: 'RemoveNavigatorAnimationStrategy', note: 'visibility fix-up only — no animation is played' },
      { lane: 'b', tier: 'know', title: 'ViewDidClose', sync: 'sync → async' },
      { lane: 'b', tier: 'know', title: 'DisposeAsync()', note: 'destroys the prefab → Presenter.OnDeinit → disposes Model and Disposables' },
      { lane: 'core', tier: 'know', title: 'OnDidNavigate' },
      { lane: 'core', tier: 'internal', title: 'locker.Dispose()', note: 'input unlocked' },
      { lane: 'full', tier: 'callout', title: 'The top screen C receives no lifecycle event and no animation during a Remove.' },
    ],
    footer: 'Removing the current top screen logs a warning and falls back to a full Pop.',
  },
};

// ----------------------------------------------------------------- write ----

const manifest = [];
for (const [name, diagram] of Object.entries(diagrams)) {
  const { svg, W: w, H: h } = build(diagram);
  writeFileSync(join(OUT_DIR, `${name}.svg`), svg);
  writeFileSync(
    join(OUT_DIR, `${name}.html`),
    `<!doctype html><meta charset="utf-8"><style>html,body{margin:0;padding:0;background:${C.bg}}svg{display:block}</style>${svg}`
  );
  manifest.push(`${name} ${w} ${h}`);
  console.error(`${name}.svg  ${w}x${h}`);
}
writeFileSync(join(OUT_DIR, 'manifest.txt'), `${manifest.join('\n')}\n`);
