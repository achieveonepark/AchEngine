import type { DefaultTheme } from 'vitepress'
import { withMermaid } from 'vitepress-plugin-mermaid'

const repositoryUrl = 'https://github.com/achieveonepark/AchEngine'
const docsEditPattern = `${repositoryUrl}/edit/initial/Docs~/:path`

type LocaleCode = 'ko' | 'en'

const localeLabels = {
  ko: {
    label: '한국어',
    lang: 'ko-KR',
    description: 'VContainer DI · UI System · Addressables · Localization · Table Loader — Unity 통합 툴킷',
    navGuide: '가이드',
    navChangelog: '변경 내역',
    navGitHub: 'GitHub',
    startSection: '시작하기',
    introPage: 'AchEngine이란?',
    installationPage: '설치',
    quickStartPage: '빠른 시작',
    diSection: 'DI 시스템',
    overviewPage: '개요',
    installerPage: 'AchEngineInstaller',
    locatorPage: 'ServiceLocator',
    lifecyclePage: 'DI 라이프사이클',
    uiSection: 'UI 시스템',
    viewsPage: 'UIView & 수명 주기',
    workspacePage: 'UI Workspace',
    tableSection: 'Table Loader',
    setupPage: '설정 & 다운로드',
    codegenPage: '코드 생성 & 런타임',
    jsonCsvPage: 'JSON → CSV 내보내기',
    addressablesSection: 'Addressables',
    foldersPage: '감시 폴더 & 그룹',
    remotePage: '원격 콘텐츠',
    localizationSection: 'Localization',
    databasePage: '설정 & 데이터베이스',
    localizationCodegenPage: '키 상수 코드 생성',
    ecsSection: 'ECS',
    ecsPage: 'ECS 래퍼',
    managersSection: '매니저 시스템',
    managersOverviewPage: '개요',
    configManagerPage: 'ConfigManager',
    soundManagerPage: 'SoundManager',
    sceneManagerPage: 'AchSceneManager',
    poolManagerPage: 'PoolManager',
    playerDataPage: 'PlayerData & QuickSave',
    iapManagerPage: 'IAPManager',
    singletonSection: '싱글턴',
    extensionsSection: '확장 메서드',
    scriptableObjectPage: 'AchScriptableObject',
    webRequestSection: 'Web Request',
    httpLinkPage: 'HttpLink',
    bindingPage: 'UIBindingManager',
    draggablePage: 'Draggable & 터치',
    movementSection: '이동',
    movementPage: 'AchMover',
    pathfindingSection: '길찾기',
    pathfindingPage: 'A* 길찾기',
    integrationSection: '모듈 연계',
    integrationPage: '통합 가이드',
    editLinkText: '이 페이지 수정하기',
    lastUpdatedText: '마지막 업데이트',
    outlineLabel: '이 페이지에서',
    prevLabel: '이전 페이지',
    nextLabel: '다음 페이지',
    langMenuLabel: '언어 변경',
    sidebarMenuLabel: '메뉴',
    returnToTopLabel: '맨 위로',
    skipToContentLabel: '본문으로 건너뛰기',
    darkModeSwitchLabel: '테마',
    lightModeSwitchTitle: '라이트 모드로 전환',
    darkModeSwitchTitle: '다크 모드로 전환',
  },
  en: {
    label: 'English',
    lang: 'en-US',
    description: 'VContainer DI · UI System · Addressables · Localization · Table Loader — Unity development toolkit',
    navGuide: 'Guide',
    navChangelog: 'Changelog',
    navGitHub: 'GitHub',
    startSection: 'Getting Started',
    introPage: 'What Is AchEngine?',
    installationPage: 'Installation',
    quickStartPage: 'Quick Start',
    diSection: 'DI System',
    overviewPage: 'Overview',
    installerPage: 'AchEngineInstaller',
    locatorPage: 'ServiceLocator',
    lifecyclePage: 'DI Lifecycle',
    uiSection: 'UI System',
    viewsPage: 'UIView & Lifecycle',
    workspacePage: 'UI Workspace',
    tableSection: 'Table Loader',
    setupPage: 'Setup & Download',
    codegenPage: 'Code Generation & Runtime',
    jsonCsvPage: 'JSON to CSV Export',
    addressablesSection: 'Addressables',
    foldersPage: 'Watched Folders & Groups',
    remotePage: 'Remote Content',
    localizationSection: 'Localization',
    databasePage: 'Setup & Database',
    localizationCodegenPage: 'Key Constant Code Generation',
    ecsSection: 'ECS',
    ecsPage: 'ECS Wrapper',
    managersSection: 'Manager System',
    managersOverviewPage: 'Overview',
    configManagerPage: 'ConfigManager',
    soundManagerPage: 'SoundManager',
    sceneManagerPage: 'AchSceneManager',
    poolManagerPage: 'PoolManager',
    playerDataPage: 'PlayerData & QuickSave',
    iapManagerPage: 'IAPManager',
    singletonSection: 'Singleton',
    extensionsSection: 'Extensions',
    scriptableObjectPage: 'AchScriptableObject',
    webRequestSection: 'Web Request',
    httpLinkPage: 'HttpLink',
    bindingPage: 'UIBindingManager',
    draggablePage: 'Draggable & Touch',
    movementSection: 'Movement',
    movementPage: 'AchMover',
    pathfindingSection: 'Pathfinding',
    pathfindingPage: 'A* Pathfinding',
    integrationSection: 'Module Integration',
    integrationPage: 'Integration Guide',
    editLinkText: 'Edit this page',
    lastUpdatedText: 'Last updated',
    outlineLabel: 'On this page',
    prevLabel: 'Previous page',
    nextLabel: 'Next page',
    langMenuLabel: 'Change language',
    sidebarMenuLabel: 'Menu',
    returnToTopLabel: 'Return to top',
    skipToContentLabel: 'Skip to content',
    darkModeSwitchLabel: 'Appearance',
    lightModeSwitchTitle: 'Switch to light theme',
    darkModeSwitchTitle: 'Switch to dark theme',
  },
} as const

function localePath(locale: LocaleCode, path = ''): string {
  const base = locale === 'ko' ? '/' : '/en/'
  return `${base}${path}`.replace(/\/{2,}/g, '/')
}

function createSidebar(locale: LocaleCode): DefaultTheme.SidebarItem[] {
  const text = localeLabels[locale]

  return [
    {
      text: text.startSection,
      items: [
        { text: text.introPage, link: localePath(locale, 'guide/') },
        { text: text.installationPage, link: localePath(locale, 'guide/installation') },
        { text: text.quickStartPage, link: localePath(locale, 'guide/getting-started') },
      ],
    },
    {
      text: text.diSection,
      items: [
        { text: text.overviewPage, link: localePath(locale, 'guide/di/') },
        { text: text.installerPage, link: localePath(locale, 'guide/di/installer') },
        { text: text.locatorPage, link: localePath(locale, 'guide/di/locator') },
        { text: text.lifecyclePage, link: localePath(locale, 'guide/di/lifecycle') },
      ],
    },
    {
      text: text.uiSection,
      items: [
        { text: text.overviewPage, link: localePath(locale, 'guide/ui/') },
        { text: text.viewsPage, link: localePath(locale, 'guide/ui/views') },
        { text: text.workspacePage, link: localePath(locale, 'guide/ui/workspace') },
        { text: text.bindingPage, link: localePath(locale, 'guide/ui/binding') },
        { text: text.draggablePage, link: localePath(locale, 'guide/ui/draggable') },
      ],
    },
    {
      text: text.tableSection,
      items: [
        { text: text.overviewPage, link: localePath(locale, 'guide/table/') },
        { text: text.setupPage, link: localePath(locale, 'guide/table/setup') },
        { text: text.jsonCsvPage, link: localePath(locale, 'guide/table/json-to-csv') },
        { text: text.codegenPage, link: localePath(locale, 'guide/table/codegen') },
      ],
    },
    {
      text: text.addressablesSection,
      items: [
        { text: text.overviewPage, link: localePath(locale, 'guide/addressables/') },
        { text: text.foldersPage, link: localePath(locale, 'guide/addressables/folders') },
        { text: text.remotePage, link: localePath(locale, 'guide/addressables/remote') },
      ],
    },
    {
      text: text.localizationSection,
      items: [
        { text: text.overviewPage, link: localePath(locale, 'guide/localization/') },
        { text: text.databasePage, link: localePath(locale, 'guide/localization/setup') },
        { text: text.localizationCodegenPage, link: localePath(locale, 'guide/localization/codegen') },
      ],
    },
    {
      text: text.ecsSection,
      items: [{ text: text.ecsPage, link: localePath(locale, 'guide/ecs/') }],
    },
    {
      text: text.managersSection,
      items: [
        { text: text.managersOverviewPage, link: localePath(locale, 'guide/managers/') },
        { text: text.configManagerPage,    link: localePath(locale, 'guide/managers/config') },
        { text: text.soundManagerPage,     link: localePath(locale, 'guide/managers/sound') },
        { text: text.sceneManagerPage,     link: localePath(locale, 'guide/managers/scene') },
        { text: text.poolManagerPage,      link: localePath(locale, 'guide/managers/pool') },
        { text: text.playerDataPage,       link: localePath(locale, 'guide/managers/player-data') },
        { text: text.iapManagerPage,       link: localePath(locale, 'guide/managers/iap') },
      ],
    },
    {
      text: text.webRequestSection,
      items: [
        { text: text.httpLinkPage, link: localePath(locale, 'guide/web-request') },
      ],
    },
    {
      text: text.singletonSection,
      items: [
        { text: text.overviewPage, link: localePath(locale, 'guide/singleton/') },
      ],
    },
    {
      text: text.extensionsSection,
      items: [
        { text: text.overviewPage, link: localePath(locale, 'guide/extensions/') },
        { text: text.scriptableObjectPage, link: localePath(locale, 'guide/extensions/scriptable-object') },
      ],
    },
    {
      text: text.movementSection,
      items: [
        { text: text.movementPage, link: localePath(locale, 'guide/movement') },
      ],
    },
    {
      text: text.pathfindingSection,
      items: [
        { text: text.pathfindingPage, link: localePath(locale, 'guide/pathfinding') },
      ],
    },
    {
      text: text.integrationSection,
      items: [{ text: text.integrationPage, link: localePath(locale, 'guide/integration') }],
    },
  ]
}

function createThemeConfig(locale: LocaleCode): DefaultTheme.Config {
  const text = localeLabels[locale]

  return {
    logo: '/logo.svg',
    siteTitle: 'AchEngine',
    nav: [
      { text: text.navGuide, link: localePath(locale, 'guide/') },
      { text: text.navChangelog, link: localePath(locale, 'changelog') },
      { text: text.navGitHub, link: repositoryUrl, target: '_blank' },
    ],
    sidebar: {
      [localePath(locale, 'guide/')]: createSidebar(locale),
    },
    socialLinks: [{ icon: 'github', link: repositoryUrl }],
    footer: {
      message: 'MIT License',
      copyright: 'Copyright © 2024 AchEngine',
    },
    search: {
      provider: 'local',
    },
    editLink: {
      pattern: docsEditPattern,
      text: text.editLinkText,
    },
    lastUpdated: {
      text: text.lastUpdatedText,
    },
    outline: {
      label: text.outlineLabel,
      level: [2, 3],
    },
    docFooter: {
      prev: text.prevLabel,
      next: text.nextLabel,
    },
    langMenuLabel: text.langMenuLabel,
    sidebarMenuLabel: text.sidebarMenuLabel,
    returnToTopLabel: text.returnToTopLabel,
    skipToContentLabel: text.skipToContentLabel,
    darkModeSwitchLabel: text.darkModeSwitchLabel,
    lightModeSwitchTitle: text.lightModeSwitchTitle,
    darkModeSwitchTitle: text.darkModeSwitchTitle,
  }
}

export default withMermaid({
  base: '/AchEngine/',
  lang: localeLabels.ko.lang,
  title: 'AchEngine',
  description: localeLabels.ko.description,

  locales: {
    root: {
      label: localeLabels.ko.label,
      lang: localeLabels.ko.lang,
      link: '/',
      title: 'AchEngine',
      description: localeLabels.ko.description,
      themeConfig: createThemeConfig('ko'),
    },
    en: {
      label: localeLabels.en.label,
      lang: localeLabels.en.lang,
      link: '/en/',
      title: 'AchEngine',
      description: localeLabels.en.description,
      themeConfig: createThemeConfig('en'),
    },
  },

  head: [
    ['link', { rel: 'icon', href: '/AchEngine/favicon.svg' }],
    ['meta', { name: 'theme-color', content: '#5d9ecc' }],
  ],

  mermaid: {
    theme: 'base',
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
      stateBkg: '#1e3a5f',
      stateStart: '#3b82f6',
      stateEnd: '#10b981',
      transitionColor: '#64748b',
      actorBkg: '#1e3a5f',
      actorBorder: '#3b82f6',
      actorTextColor: '#e2e8f0',
      actorLineColor: '#64748b',
      signalColor: '#93c5fd',
      signalTextColor: '#e2e8f0',
      activationBkgColor: '#0f2d4a',
      activationBorderColor: '#3b82f6',
      fontFamily: '"Inter", "Noto Sans KR", sans-serif',
      fontSize: '14px',
    },
  },
})
