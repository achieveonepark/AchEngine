import fs from 'node:fs/promises';
import path from 'node:path';

const root = path.resolve(import.meta.dirname, '..');
const sourceRoot = path.join(root, 'content/docs');
const targetRoot = path.join(root, 'rspress-content');
const locales = ['ko', 'en', 'ja', 'zh'];
const legacyUsageRoutes = new Set([
  'addressables',
  'async',
  'di',
  'integration',
  'localization',
  'mcp',
  'movement',
  'table',
  'timer',
  'ui',
  'web-request',
]);

function splitLocale(filename, extension) {
  const localizedPattern = new RegExp(`^(.*)\\.(en|ja|zh)\\.${extension}$`);
  const localized = filename.match(localizedPattern);
  if (localized) {
    return { locale: localized[2], filename: `${localized[1]}.${extension}` };
  }

  const defaultPattern = new RegExp(`^(.*)\\.${extension}$`);
  const defaultMatch = filename.match(defaultPattern);
  if (defaultMatch) {
    return { locale: 'ko', filename };
  }

  return undefined;
}

async function copyDirectory(sourceDir, relativeDir = '') {
  const entries = await fs.readdir(sourceDir, { withFileTypes: true });

  for (const entry of entries) {
    const sourcePath = path.join(sourceDir, entry.name);
    if (entry.isDirectory()) {
      await copyDirectory(sourcePath, path.join(relativeDir, entry.name));
      continue;
    }

    const sourceParentDir =
      relativeDir === '' ? sourceRoot : path.join(sourceRoot, relativeDir);

    if (entry.name.startsWith('meta') && entry.name.endsWith('.json')) {
      const localized = splitLocale(entry.name, 'json');
      if (!localized) continue;

      const metadata = JSON.parse(await fs.readFile(sourcePath, 'utf8'));
      const sideMeta = [];

      for (const page of metadata.pages ?? []) {
        const pagePath = path.join(sourceParentDir, page);
        const isDirectory = await fs
          .stat(pagePath)
          .then((stat) => stat.isDirectory())
          .catch(() => false);
        const childMetaPath = path.join(
          pagePath,
          `meta.${localized.locale === 'ko' ? '' : `${localized.locale}.`}json`,
        );
        const childTitle = await fs
          .readFile(childMetaPath, 'utf8')
          .then((content) => JSON.parse(content).title)
          .catch(() => undefined);

        sideMeta.push({
          type: isDirectory ? 'dir' : 'file',
          name: page,
          ...(isDirectory && childTitle ? { label: childTitle } : {}),
        });
      }

      const destination = path.join(
        targetRoot,
        localized.locale,
        relativeDir,
        '_meta.json',
      );
      await fs.mkdir(path.dirname(destination), { recursive: true });
      await fs.writeFile(destination, `${JSON.stringify(sideMeta, null, 2)}\n`);
      continue;
    }

    const localized = splitLocale(entry.name, 'mdx');
    const locale = localized?.locale ?? 'ko';
    const filename = localized?.filename ?? entry.name;
    const destination = path.join(targetRoot, locale, relativeDir, filename);
    await fs.mkdir(path.dirname(destination), { recursive: true });
    let content = await fs.readFile(sourcePath, 'utf8');
    if (locale === 'ko') {
      content = content.replace(
        /(\]\()\/(?:(?:docs\/)?usage\/)?([^/()[\]?#]+)(?=\/|[?#)])/g,
        (match, prefix, route) =>
          legacyUsageRoutes.has(route) ? `${prefix}/usage/${route}` : match,
      );
    }
    await fs.writeFile(destination, content);
  }
}

await fs.rm(targetRoot, { recursive: true, force: true });
await Promise.all(
  locales.map((locale) =>
    fs.mkdir(path.join(targetRoot, locale), { recursive: true }),
  ),
);
await fs.cp(path.join(root, 'public'), path.join(targetRoot, 'public'), {
  recursive: true,
});
await copyDirectory(sourceRoot);
