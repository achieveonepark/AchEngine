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
    } else if (entry.name.endsWith('.html')) {
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
  const sourcePath = path.join(outputRoot, relativePath);
  const aliasPath = path.join(outputRoot, 'ko', relativePath);
  let html = await fs.readFile(sourcePath, 'utf8');

  html = html.replace(
    /(href|content)="\/AchEngine\/([^"]*)"/g,
    (match, attribute, target) => {
      const pathname = target.split(/[?#]/, 1)[0];
      if (
        pathname.startsWith('static/') ||
        [...locales].some((locale) => pathname.startsWith(`${locale}/`)) ||
        path.posix.extname(pathname)
      ) {
        return match;
      }

      return `${attribute}="${basePath}ko/${target}"`;
    },
  );

  await fs.mkdir(path.dirname(aliasPath), { recursive: true });
  await fs.writeFile(aliasPath, html);
}

await fs.writeFile(
  path.join(outputRoot, 'index.html'),
  `<!doctype html>
<html lang="ko">
  <head>
    <meta charset="utf-8">
    <meta http-equiv="refresh" content="0;url=${basePath}ko/">
    <link rel="canonical" href="${basePath}ko/">
    <title>AchEngine</title>
  </head>
  <body>
    <script>window.location.replace('${basePath}ko/');</script>
    <a href="${basePath}ko/">AchEngine 문서로 이동</a>
  </body>
</html>
`,
);
