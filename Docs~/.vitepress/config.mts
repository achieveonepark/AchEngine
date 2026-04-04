import { defineConfig } from 'vitepress'

export default defineConfig({
  base: '/AchEngine/',
  lang: 'ko-KR',
  title: 'AchEngine',
  description: 'VContainer DI · UI System · Addressables · Localization · Table Loader — Unity 통합 툴킷',

  head: [
    ['link', { rel: 'icon', href: '/AchEngine/favicon.svg' }],
    ['meta', { name: 'theme-color', content: '#5d9ecc' }],
  ],

  themeConfig: {
    logo: '/logo.svg',
    siteTitle: 'AchEngine',

    nav: [
      { text: '가이드', link: '/guide/' },
      {
        text: '모듈',
        items: [
          { text: 'DI (VContainer 래퍼)', link: '/di/' },
          { text: 'UI System', link: '/ui/' },
          { text: 'Table Loader', link: '/table/' },
          { text: 'Addressables', link: '/addressables/' },
          { text: 'Localization', link: '/localization/' },
        ],
      },
      { text: 'GitHub', link: 'https://github.com/achieveonepark/AchEngine', target: '_blank' },
    ],

    sidebar: {
      '/guide/': [
        {
          text: '시작하기',
          items: [
            { text: 'AchEngine이란?', link: '/guide/' },
            { text: '설치', link: '/guide/installation' },
            { text: '빠른 시작', link: '/guide/getting-started' },
          ],
        },
      ],
      '/di/': [
        {
          text: 'DI (VContainer 래퍼)',
          items: [
            { text: '개요', link: '/di/' },
            { text: 'AchEngineInstaller', link: '/di/installer' },
            { text: 'ServiceLocator', link: '/di/service-locator' },
          ],
        },
      ],
      '/ui/': [
        {
          text: 'UI System',
          items: [
            { text: '개요', link: '/ui/' },
            { text: 'UIView & 수명 주기', link: '/ui/views' },
            { text: '레이어 시스템', link: '/ui/layers' },
            { text: '트랜지션', link: '/ui/transitions' },
          ],
        },
      ],
      '/table/': [
        {
          text: 'Table Loader',
          items: [
            { text: '개요', link: '/table/' },
            { text: '설정 & 다운로드', link: '/table/setup' },
            { text: '코드 생성 & 베이크', link: '/table/codegen' },
          ],
        },
      ],
      '/addressables/': [
        {
          text: 'Addressables',
          items: [
            { text: '개요', link: '/addressables/' },
            { text: '감시 폴더 & 그룹', link: '/addressables/watched-folders' },
            { text: '원격 콘텐츠', link: '/addressables/remote' },
          ],
        },
      ],
      '/localization/': [
        {
          text: 'Localization',
          items: [
            { text: '개요', link: '/localization/' },
            { text: '설정 & 데이터베이스', link: '/localization/setup' },
            { text: '키 상수 코드 생성', link: '/localization/codegen' },
          ],
        },
      ],
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/achieveonepark/AchEngine' },
    ],

    footer: {
      message: 'MIT License',
      copyright: 'Copyright © 2024 AchEngine',
    },

    search: {
      provider: 'local',
    },

    editLink: {
      pattern: 'https://github.com/achieveonepark/AchEngine/edit/initial/Docs~/:path',
      text: '이 페이지 수정하기',
    },

    lastUpdated: {
      text: '마지막 업데이트',
    },
  },
})
