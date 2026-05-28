#!/usr/bin/env node
/**
 * DocFX metadata YAML → Docusaurus Markdown 변환기
 *
 *  1. docfx~/docfx.json 으로 `docfx metadata` 실행 (이미 결과가 있으면 스킵)
 *  2. docfx~/api/*.yml 파싱
 *  3. 네임스페이스 단위로 docs/api/<Namespace>.md 생성 (포함된 타입 + 멤버 시그니처)
 *  4. i18n/en|ja|zh 로 동일 파일 복사 — 로케일별로 분리해 라우팅이 깨지지 않게 함
 *
 *  CI 환경에 docfx 가 없으면 경고만 찍고 통과한다 (가이드 빌드는 그대로 진행).
 */
import fs from 'node:fs';
import path from 'node:path';
import yaml from 'js-yaml';
import { execSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const DOCS_ROOT = path.resolve(__dirname, '..');
const REPO_ROOT = path.resolve(DOCS_ROOT, '..');
const DOCFX_DIR = path.join(REPO_ROOT, 'docfx~');
const METADATA_DIR = path.join(DOCFX_DIR, 'api');
const OUT_DIR = path.join(DOCS_ROOT, 'docs', 'api');
const I18N_LOCALES = ['en', 'ja', 'zh'];

const TYPE_KINDS = new Set(['Class', 'Interface', 'Struct', 'Enum', 'Delegate']);
const MEMBER_KINDS = new Set(['Method', 'Property', 'Field', 'Event', 'Constructor', 'Operator']);

function log(msg) { console.log(`[gen-api] ${msg}`); }
function warn(msg) { console.warn(`[gen-api] ${msg}`); }

// ── 1. metadata 보장 ──────────────────────────────────────
function ensureMetadata() {
  const have = fs.existsSync(METADATA_DIR) &&
    fs.readdirSync(METADATA_DIR).some(f => f.endsWith('.yml'));
  if (have) {
    log(`reusing existing metadata in ${path.relative(REPO_ROOT, METADATA_DIR)}`);
    return true;
  }
  log('running `docfx metadata docfx.json`...');
  try {
    execSync('docfx metadata docfx.json', { cwd: DOCFX_DIR, stdio: 'inherit' });
    return true;
  } catch (e) {
    warn(`docfx metadata failed: ${e.message}`);
    warn('skipping API generation. install: `dotnet tool install -g docfx`');
    return false;
  }
}

function mirrorToLocales() {
  for (const locale of I18N_LOCALES) {
    const dest = path.join(DOCS_ROOT, 'i18n', locale, 'docusaurus-plugin-content-docs', 'current', 'api');
    fs.rmSync(dest, { recursive: true, force: true });
    fs.mkdirSync(dest, { recursive: true });
    for (const f of fs.readdirSync(OUT_DIR)) {
      fs.copyFileSync(path.join(OUT_DIR, f), path.join(dest, f));
    }
  }
}

function writePlaceholder(message) {
  fs.rmSync(OUT_DIR, { recursive: true, force: true });
  fs.mkdirSync(OUT_DIR, { recursive: true });
  fs.writeFileSync(path.join(OUT_DIR, 'index.md'),
    `---\ntitle: API 레퍼런스\nslug: /api/\nsidebar_label: API 개요\nsidebar_position: 1\nformat: md\n---\n\n# API 레퍼런스\n\n*${message}*\n`);
  mirrorToLocales();
}

if (!ensureMetadata()) {
  writePlaceholder('DocFX 가 설치되지 않아 자동 생성을 건너뛰었습니다. CI 빌드에서 채워집니다.');
  process.exit(0);
}

// ── 2. YAML 로드 ──────────────────────────────────────────
const items = new Map(); // uid → item
const namespaces = new Set();

for (const f of fs.readdirSync(METADATA_DIR)) {
  if (!f.endsWith('.yml') || f === 'toc.yml') continue;
  // DocFX YAML 에 가끔 끼는 C0 컨트롤 캐릭터(\x00-\x08, \x0B-\x0C, \x0E-\x1F)
  // 제거 후 파싱한다. TAB/LF/CR 은 유지.
  const raw = fs.readFileSync(path.join(METADATA_DIR, f), 'utf8')
    .replace(/[\x00-\x08\x0B\x0C\x0E-\x1F]/g, '');
  // DocFX header: `### YamlMime:ManagedReference` 제거
  const body = raw.replace(/^###[^\n]*\n/, '');
  let doc;
  try { doc = yaml.load(body); }
  catch (e) { warn(`parse failed for ${f}: ${e.message}`); continue; }
  if (!doc || !Array.isArray(doc.items)) continue;
  for (const item of doc.items) {
    if (!item || !item.uid) continue;
    items.set(item.uid, item);
    if (item.type === 'Namespace') namespaces.add(item.uid);
  }
}

if (namespaces.size === 0) {
  warn('no namespaces found — emitting placeholder');
  writePlaceholder('메타데이터가 비어있습니다.');
  process.exit(0);
}

// ── 3. 네임스페이스별 타입 그룹핑 ──────────────────────────
const typesByNs = new Map();
for (const ns of namespaces) typesByNs.set(ns, []);

for (const item of items.values()) {
  if (!TYPE_KINDS.has(item.type)) continue;
  const ns = item.namespace || item.parent;
  if (!ns || !typesByNs.has(ns)) continue;
  typesByNs.get(ns).push(item);
}

// ── 4. 마크다운 생성 ───────────────────────────────────────
function fenceCsharp(code) {
  if (!code) return '';
  return '```csharp\n' + String(code).trim() + '\n```';
}

/**
 * MDX 3 는 본문의 `<X>` 를 JSX 로, `{X}` 를 expression 으로 strict 하게 파싱한다.
 * code fence/inline code 안에서는 안전하므로 본문에 들어가는 텍스트만 엔티티화한다.
 */
function escapeMdx(s) {
  if (!s) return '';
  return String(s)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/\{/g, '&#123;')
    .replace(/\}/g, '&#125;');
}

function cleanSummary(text) {
  if (!text) return '';
  // 1) 알려진 cross-ref 태그를 단순 텍스트로 환원 (cref 의 T:/M: 접두어 제거)
  let s = String(text)
    .replace(/<see\s+cref="([^"]+)"\s*\/?>/g, (_, ref) => ref.replace(/^[A-Z]:/, ''))
    .replace(/<see\s+langword="([^"]+)"\s*\/?>/g, '$1')
    .replace(/<seealso\s+cref="([^"]+)"\s*\/?>/g, (_, ref) => ref.replace(/^[A-Z]:/, ''))
    // 2) 남은 모든 XML doc 태그(<c>, <para>, <list>, <item>, <code>, …) 제거
    .replace(/<\/?[a-zA-Z][^>]*>/g, '')
    .replace(/\r\n/g, '\n')
    .trim();
  // 3) 본문에 박힌 generic 표기 `Task<T>` · 자리표시자 `{name}` 등을 엔티티로
  return escapeMdx(s);
}

function shortLabel(ns) {
  const trimmed = ns.replace(/^AchEngine\.?/, '');
  return trimmed || 'AchEngine';
}

/** 헤딩에 부여할 안정적인 anchor (영숫자만) */
function anchor(id) {
  return String(id).toLowerCase().replace(/[^a-z0-9]+/g, '').slice(0, 80) || 'item';
}

function renderType(item) {
  const lines = [];
  // 헤딩에 명시 anchor 부여 → 동일 페이지 내 타입 테이블 링크와 매칭
  lines.push(`## ${item.type} · \`${item.id}\` {#${anchor(item.id)}}`);
  if (item.summary) {
    lines.push('');
    lines.push(cleanSummary(item.summary));
  }
  if (item.syntax && item.syntax.content) {
    lines.push('');
    lines.push(fenceCsharp(item.syntax.content));
  }

  const members = (item.children || [])
    .map(uid => items.get(uid))
    .filter(m => m && MEMBER_KINDS.has(m.type));

  if (members.length === 0) return lines.join('\n') + '\n';

  // 멤버를 종류별로 묶어서 표시
  const byKind = new Map();
  for (const m of members) {
    if (!byKind.has(m.type)) byKind.set(m.type, []);
    byKind.get(m.type).push(m);
  }

  for (const [kind, ms] of byKind) {
    lines.push('');
    lines.push(`### ${kind} (${ms.length})`);
    for (const m of ms) {
      lines.push('');
      lines.push(`#### \`${m.id}\``);
      if (m.summary) {
        lines.push('');
        lines.push(cleanSummary(m.summary));
      }
      if (m.syntax && m.syntax.content) {
        lines.push('');
        lines.push(fenceCsharp(m.syntax.content));
      }
    }
  }
  return lines.join('\n') + '\n';
}

function renderNamespace(ns) {
  const item = items.get(ns);
  const types = (typesByNs.get(ns) || []).slice()
    .sort((a, b) => a.id.localeCompare(b.id));

  const front = [
    '---',
    `title: ${ns}`,
    `sidebar_label: ${shortLabel(ns)}`,
    'format: md',
    '---',
    '',
    `# ${ns}`,
    '',
  ];
  if (item && item.summary) front.push(cleanSummary(item.summary), '');
  if (types.length === 0) {
    front.push('*(공개 타입 없음)*', '');
    return front.join('\n');
  }
  front.push('| 타입 | 종류 | 요약 |', '|---|---|---|');
  for (const t of types) {
    const summary = t.summary ? cleanSummary(t.summary).split('\n')[0] : '';
    front.push(`| [\`${t.id}\`](#${anchor(t.id)}) | ${t.type} | ${summary.replace(/\|/g, '\\|')} |`);
  }
  front.push('');
  for (const t of types) front.push(renderType(t));
  return front.join('\n');
}

function renderIndex(sortedNs) {
  const lines = [
    '---',
    'title: API 레퍼런스',
    'slug: /api/',
    'sidebar_label: API 개요',
    'sidebar_position: 1',
    'format: md',
    '---',
    '',
    '# API 레퍼런스',
    '',
    'C# XML 주석에서 자동 생성된 네임스페이스·타입·멤버 레퍼런스입니다.',
    '',
    '## 네임스페이스',
    '',
    '| 네임스페이스 | 타입 수 | 요약 |',
    '|---|---|---|',
  ];
  for (const ns of sortedNs) {
    const item = items.get(ns);
    const summary = item && item.summary ? cleanSummary(item.summary).split('\n')[0] : '';
    const count = (typesByNs.get(ns) || []).length;
    lines.push(`| [\`${ns}\`](./${ns}) | ${count} | ${summary.replace(/\|/g, '\\|')} |`);
  }
  lines.push('');
  return lines.join('\n');
}

// ── 5. 파일 쓰기 ──────────────────────────────────────────
fs.rmSync(OUT_DIR, { recursive: true, force: true });
fs.mkdirSync(OUT_DIR, { recursive: true });

const sortedNs = [...namespaces].sort();
fs.writeFileSync(path.join(OUT_DIR, 'index.md'), renderIndex(sortedNs));
for (const ns of sortedNs) {
  fs.writeFileSync(path.join(OUT_DIR, `${ns}.md`), renderNamespace(ns));
}

log(`wrote ${sortedNs.length + 1} files to docs/api/`);

// ── 6. i18n 로케일에 복사 (XML 주석은 한국어라 동일 내용 사용) ──
mirrorToLocales();
log(`mirrored to ${I18N_LOCALES.length} locales`);
