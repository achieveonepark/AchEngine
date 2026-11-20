import fs from 'node:fs/promises';
import path from 'node:path';

const root = path.resolve(import.meta.dirname, '..');
const outputRoot = path.join(root, 'out');
const locales = new Set(['ko', 'en', 'ja', 'zh']);
const basePath = '/AchEngine/';

async function collectHtml(directory, relativeDir = '') {
  const entries = await fs.readdir(directory, { withFileTypes: true });
  const files = [];

  for (const entry of entries) {
    if (
      relativeDir === '' &&
      (locales.has(entry.name) || entry.name === 'static')
    ) {
      continue;
    }

    const relativePath = path.join(relativeDir, entry.name);
    const absolutePath = path.join(directory, entry.name);

    if (entry.isDirectory()) {
      files.push(...(await collectHtml(absolutePath, relativePath)));
    } else if (entry.name.endsWith('.html') && entry.name !== '404.html') {
      files.push(relativePath);
    }
  }

  return files;
}

const htmlFiles = await collectHtml(outputRoot);
if (!htmlFiles.includes('index.html')) {
  throw new Error('Rspress did not generate the default-locale index.html');
}

for (const relativePath of htmlFiles) {
  const aliasPath = path.join(outputRoot, 'ko', relativePath);
  const target = (basePath + relativePath.split(path.sep).map(encodeURIComponent).join('/'))
    .replace(/\/index\.html$/, '/')
    .replace(/\.html$/, '');

  await fs.mkdir(path.dirname(aliasPath), { recursive: true });
  await fs.writeFile(
    aliasPath,
    `<!doctype html>
<html lang="ko">
  <head>
    <meta charset="utf-8">
    <link rel="canonical" href="${target}">
    <title>AchEngine</title>
    <script>location.replace(${JSON.stringify(target)} + location.search + location.hash);</script>
    <noscript><meta http-equiv="refresh" content="0;url=${target}"></noscript>
  </head>
  <body><a href="${target}">AchEngine 문서로 이동</a></body>
</html>
`,
  );
}
