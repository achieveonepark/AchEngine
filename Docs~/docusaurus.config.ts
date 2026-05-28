import { themes as prismThemes } from 'prism-react-renderer';
import type { Config } from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const repositoryUrl = 'https://github.com/achieveonepark/AchEngine';

const config: Config = {
  title: 'AchEngine',
  tagline: 'VContainer DI · UI System · Addressables · Localization · Table Loader — Unity 통합 툴킷',
  favicon: 'favicon.svg',

  // GitHub Pages
  url: 'https://achieveonepark.github.io',
  baseUrl: '/AchEngine/',
  organizationName: 'achieveonepark',
  projectName: 'AchEngine',

  onBrokenLinks: 'warn',
  onBrokenMarkdownLinks: 'warn',

  markdown: {
    mermaid: true,
  },
  themes: ['@docusaurus/theme-mermaid'],

  i18n: {
    defaultLocale: 'ko',
    locales: ['ko', 'en', 'ja', 'zh'],
    localeConfigs: {
      ko: { label: '한국어', htmlLang: 'ko-KR' },
      en: { label: 'English', htmlLang: 'en-US' },
      ja: { label: '日本語', htmlLang: 'ja-JP' },
      zh: { label: '中文', htmlLang: 'zh-CN' },
    },
  },

  presets: [
    [
      'classic',
      {
        docs: {
          path: 'docs',
          routeBasePath: '/',
          sidebarPath: './sidebars.ts',
          editUrl: ({ locale, versionDocsDirPath, docPath }) => {
            if (locale === 'ko') return `${repositoryUrl}/edit/main/Docs~/${versionDocsDirPath}/${docPath}`;
            return `${repositoryUrl}/edit/main/Docs~/i18n/${locale}/docusaurus-plugin-content-docs/current/${docPath}`;
          },
          showLastUpdateTime: true,
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    image: 'logo.svg',
    colorMode: {
      defaultMode: 'dark',
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'AchEngine',
      logo: {
        alt: 'AchEngine',
        src: 'logo.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'guide',
          position: 'left',
          label: 'Docs',
        },
        { to: '/api/', label: 'API', position: 'left' },
        { to: '/changelog', label: 'Change Log', position: 'left' },
        {
          type: 'localeDropdown',
          position: 'right',
        },
        {
          href: repositoryUrl,
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [],
      copyright: 'MIT License · Copyright © AchEngine',
    },
    prism: {
      theme: prismThemes.oneDark,
      darkTheme: prismThemes.oneDark,
      additionalLanguages: ['csharp', 'json', 'yaml', 'bash'],
    },
    mermaid: {
      theme: { light: 'base', dark: 'base' },
      options: {
        themeVariables: {
          primaryColor: '#1e3a5f',
          primaryTextColor: '#e2e8f0',
          primaryBorderColor: '#3b82f6',
          lineColor: '#64748b',
          secondaryColor: '#0f2d4a',
          tertiaryColor: '#162032',
          background: '#0d1b2a',
          mainBkg: '#1e3a5f',
          nodeBorder: '#3b82f6',
          clusterBkg: '#0f2d4a',
          titleColor: '#93c5fd',
          edgeLabelBackground: '#162032',
          fontFamily: '"Inter", "Noto Sans KR", sans-serif',
          fontSize: '14px',
        },
      },
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
