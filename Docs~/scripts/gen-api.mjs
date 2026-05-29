#!/usr/bin/env node
/**
 * DocFX metadata YAML → Docusaurus Markdown 변환기
 *
 *  1. docfx~/docfx.json 으로 `docfx metadata` 실행 (이미 결과가 있으면 스킵)
 *  2. docfx~/api/*.yml 파싱
 *  3. 네임스페이스마다 폴더를 만들고 그 안에 index.md + 타입별 .md 를 생성
 *     → 사이드바가 자동으로 Namespace > Type 계층으로 펼쳐진다
 *  4. i18n/en|ja|zh 로 동일 트리 미러링
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
const KIND_LABEL = {
  Method: '메서드',
  Property: '프로퍼티',
  Field: '필드',
  Event: '이벤트',
  Constructor: '생성자',
  Operator: '연산자',
};

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

function copyRecursive(src, dest) {
  fs.mkdirSync(dest, { recursive: true });
  for (const entry of fs.readdirSync(src, { withFileTypes: true })) {
    const s = path.join(src, entry.name);
    const d = path.join(dest, entry.name);
    if (entry.isDirectory()) copyRecursive(s, d);
    else fs.copyFileSync(s, d);
  }
}

function mirrorToLocales() {
  for (const locale of I18N_LOCALES) {
    const dest = path.join(DOCS_ROOT, 'i18n', locale, 'docusaurus-plugin-content-docs', 'current', 'api');
    fs.rmSync(dest, { recursive: true, force: true });
    copyRecursive(OUT_DIR, dest);
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
  const body = raw.replace(/^###[^\n]*\n/, '');
  let doc;
  try { doc = yaml.load(body); }
  catch (e) { warn(`parse failed for ${f}: ${e.message}`); continue; }
  if (!doc || !Array.isArray(doc.items)) continue;
  for (const item of doc.items) {
    if (!item || !item.uid) continue;
    items.set(item.uid, item);
    // DocFX 가 메타데이터에 참조용 외부 네임스페이스(Cysharp, UnityEngine, VContainer 등)
    // 까지 적어두는데, 우리 프로젝트가 가진 타입이 아니므로 페이지 생성 대상에서 제외한다.
    if (item.type === 'Namespace' &&
        (item.uid === 'AchEngine' || item.uid.startsWith('AchEngine.'))) {
      namespaces.add(item.uid);
    }
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

// ── 4. 헬퍼 ─────────────────────────────────────────────
function fenceCsharp(code) {
  if (!code) return '';
  return '```csharp\n' + String(code).trim() + '\n```';
}

/**
 * MDX strict 안전망(format: md 와 함께 이중 안전망 역할). CommonMark
 * 모드에서는 어차피 JSX/expression 파싱이 비활성화되지만 본문에 노출되는
 * 위험 문자를 한 번 더 엔티티화해 둔다.
 */
function escapeAngles(s) {
  if (!s) return '';
  return String(s)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
}

function cleanSummary(text) {
  if (!text) return '';
  let s = String(text)
    .replace(/<see\s+cref="([^"]+)"\s*\/?>/g, (_, ref) => ref.replace(/^[A-Z]:/, ''))
    .replace(/<see\s+langword="([^"]+)"\s*\/?>/g, '$1')
    .replace(/<seealso\s+cref="([^"]+)"\s*\/?>/g, (_, ref) => ref.replace(/^[A-Z]:/, ''))
    .replace(/<\/?[a-zA-Z][^>]*>/g, '')
    .replace(/\r\n/g, '\n')
    .trim();
  return escapeAngles(s);
}

/** "AchEngine" → "Core", "AchEngine.DI" → "DI" */
function nsFolder(ns) {
  if (ns === 'AchEngine') return 'Core';
  return ns.replace(/^AchEngine\./, '');
}

/** 파일명 안전화 — 제네릭/연산자 문자 정리 */
function safeName(id) {
  return String(id)
    .replace(/[<>]/g, '_')
    .replace(/`/g, '')
    .replace(/[^A-Za-z0-9._-]/g, '_')
    .slice(0, 100);
}

// ── 5. 렌더러 ────────────────────────────────────────────
function renderApiIndex(sortedNs) {
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
    '왼쪽 사이드바에서 네임스페이스를 펼쳐 원하는 타입으로 바로 이동할 수 있습니다.',
    '',
    '## 네임스페이스',
    '',
    '| 네임스페이스 | 타입 수 | 요약 |',
    '|---|---:|---|',
  ];
  for (const ns of sortedNs) {
    const item = items.get(ns);
    const summary = item && item.summary ? cleanSummary(item.summary).split('\n')[0] : '—';
    const count = (typesByNs.get(ns) || []).length;
    lines.push(`| [\`${ns}\`](./${nsFolder(ns)}/) | ${count} | ${summary.replace(/\|/g, '\\|')} |`);
  }
  lines.push('');
  return lines.join('\n');
}

function renderNamespaceIndex(ns) {
  const item = items.get(ns);
  const types = (typesByNs.get(ns) || []).slice().sort((a, b) => a.id.localeCompare(b.id));
  const folder = nsFolder(ns);

  // slug 는 명시하지 않음 — 파일 경로에서 자동 추론 (docs/api/UI/index.md → /api/UI/)
  // 명시하면 _category_.json link 와 중복 라우트로 잡힘
  const lines = [
    '---',
    `title: ${ns}`,
    'sidebar_label: 개요',
    'sidebar_position: 0',
    'format: md',
    '---',
    '',
    `# ${ns}`,
    '',
  ];
  if (item && item.summary) {
    lines.push(cleanSummary(item.summary), '');
  }
  if (types.length === 0) {
    lines.push('*(공개 타입 없음)*', '');
    return lines.join('\n');
  }
  lines.push('## 타입', '');
  lines.push('| 타입 | 종류 | 요약 |', '|---|---|---|');
  for (const t of types) {
    const summary = t.summary ? cleanSummary(t.summary).split('\n')[0] : '—';
    lines.push(`| [\`${t.id}\`](./${safeName(t.id)}) | ${t.type} | ${summary.replace(/\|/g, '\\|')} |`);
  }
  lines.push('');
  return lines.join('\n');
}

function renderTypePage(item, sidebarPosition) {
  const lines = [
    '---',
    `title: ${item.id}`,
    `sidebar_label: ${item.id}`,
    `sidebar_position: ${sidebarPosition}`,
    'format: md',
    '---',
    '',
    `# \`${item.id}\``,
    '',
    `**${item.type}**${item.namespace ? ` · ${item.namespace}` : ''}`,
    '',
  ];
  if (item.summary) lines.push(cleanSummary(item.summary), '');
  if (item.syntax && item.syntax.content) lines.push(fenceCsharp(item.syntax.content), '');

  const members = (item.children || [])
    .map(uid => items.get(uid))
    .filter(m => m && MEMBER_KINDS.has(m.type));

  if (members.length === 0) return lines.join('\n');

  // 종류별 그룹핑
  const byKind = new Map();
  for (const m of members) {
    if (!byKind.has(m.type)) byKind.set(m.type, []);
    byKind.get(m.type).push(m);
  }
  // 정렬 순서: 생성자 → 프로퍼티 → 메서드 → 이벤트 → 필드 → 연산자
  const KIND_ORDER = ['Constructor', 'Property', 'Method', 'Event', 'Field', 'Operator'];
  const kinds = [...byKind.keys()].sort((a, b) => KIND_ORDER.indexOf(a) - KIND_ORDER.indexOf(b));

  for (const kind of kinds) {
    const ms = byKind.get(kind);
    lines.push(`## ${KIND_LABEL[kind] || kind} (${ms.length})`, '');
    for (const m of ms) {
      lines.push(`### \`${m.id}\``, '');
      if (m.summary) lines.push(cleanSummary(m.summary), '');
      if (m.syntax && m.syntax.content) lines.push(fenceCsharp(m.syntax.content), '');
    }
  }
  return lines.join('\n');
}

// ── 6. 파일 쓰기 ──────────────────────────────────────────
fs.rmSync(OUT_DIR, { recursive: true, force: true });
fs.mkdirSync(OUT_DIR, { recursive: true });

const sortedNs = [...namespaces].sort();
fs.writeFileSync(path.join(OUT_DIR, 'index.md'), renderApiIndex(sortedNs));

let nsPos = 2; // index.md 가 1번
let typeFileCount = 0;
for (const ns of sortedNs) {
  const folder = nsFolder(ns);
  const folderPath = path.join(OUT_DIR, folder);
  fs.mkdirSync(folderPath, { recursive: true });

  // _category_.json: link 를 제거하면 중복 라우트 경고가 사라진다.
  // 카테고리는 클릭 시 그냥 펼쳐지고, 안의 '개요' (index.md) 가 첫 항목으로 잡힘.
  fs.writeFileSync(path.join(folderPath, '_category_.json'), JSON.stringify({
    label: ns,
    position: nsPos++,
    collapsed: true,
  }, null, 2));

  fs.writeFileSync(path.join(folderPath, 'index.md'), renderNamespaceIndex(ns));

  const sortedTypes = (typesByNs.get(ns) || []).slice().sort((a, b) => a.id.localeCompare(b.id));
  sortedTypes.forEach((t, i) => {
    fs.writeFileSync(path.join(folderPath, `${safeName(t.id)}.md`), renderTypePage(t, i + 1));
    typeFileCount++;
  });
}

log(`wrote ${sortedNs.length} namespace folders, ${typeFileCount} type pages`);

mirrorToLocales();
log(`mirrored to ${I18N_LOCALES.length} locales`);
