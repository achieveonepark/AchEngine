import path from 'node:path';
import { defineConfig } from '@rspress/core';
import remarkMermaid from './remark-mermaid.mjs';

const root = import.meta.dirname;

export default defineConfig({
  title: 'AchEngine',
  description: 'AchEngine documentation',
  root: path.join(root, 'rspress-content'),
  outDir: 'out',
  base: '/AchEngine/',
  lang: 'ko',
  locales: [
    { lang: 'ko', label: '한국어', title: 'AchEngine' },
    { lang: 'en', label: 'English', title: 'AchEngine' },
    { lang: 'ja', label: '日本語', title: 'AchEngine' },
    { lang: 'zh', label: '简体中文', title: 'AchEngine' },
  ],
  icon: '/logo.svg',
  logo: {
    light: '/logo.svg',
    dark: '/logo.svg',
  },
  globalStyles: path.join(root, 'styles.css'),
  route: {
    localeRedirect: 'never',
  },
  markdown: {
    remarkPlugins: [remarkMermaid],
    globalComponents: [
      path.join(root, 'components/Callout.tsx'),
      path.join(root, 'components/Mermaid.tsx'),
    ],
  },
  themeConfig: {
    darkMode: 'dark',
    socialLinks: [
      {
        icon: 'github',
        mode: 'link',
        content: 'https://github.com/achieveonepark/AchEngine',
      },
    ],
  },
});
