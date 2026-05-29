import React from 'react';
import Link from '@docusaurus/Link';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';
import Translate, { translate } from '@docusaurus/Translate';
import styles from './index.module.css';

type Feature = {
  key: string;
  emoji: string;
  titleId: string;
  titleDefault: string;
  descId: string;
  descDefault: string;
  href: string;
};

const features: Feature[] = [
  {
    key: 'di',
    emoji: '🧩',
    titleId: 'home.feature.di.title',
    titleDefault: 'DI 시스템',
    descId: 'home.feature.di.desc',
    descDefault: 'VContainer 를 직접 다루지 않고 AchEngineInstaller 와 ServiceLocator 로 서비스를 등록·조회합니다.',
    href: '/guide/di/',
  },
  {
    key: 'managers',
    emoji: '⚙️',
    titleId: 'home.feature.managers.title',
    titleDefault: '매니저 시스템',
    descId: 'home.feature.managers.desc',
    descDefault: 'AudioManager, AchSceneManager, PoolManager, SaveManager — 핵심 서비스를 일관된 API 로 제공합니다.',
    href: '/guide/managers/',
  },
  {
    key: 'ui',
    emoji: '🖼️',
    titleId: 'home.feature.ui.title',
    titleDefault: 'UI 시스템',
    descId: 'home.feature.ui.desc',
    descDefault: '레이어 기반 View 관리·풀링·전환 애니메이션을 한 번에 처리합니다.',
    href: '/guide/ui/',
  },
  {
    key: 'table',
    emoji: '📊',
    titleId: 'home.feature.table.title',
    titleDefault: 'Table Loader',
    descId: 'home.feature.table.desc',
    descDefault: 'Google Sheets 에서 C# 모델을 자동 생성하고 타입 안전하게 사용합니다.',
    href: '/guide/table/',
  },
  {
    key: 'addressables',
    emoji: '📦',
    titleId: 'home.feature.addressables.title',
    titleDefault: 'Addressables',
    descId: 'home.feature.addressables.desc',
    descDefault: '자동 그룹·참조 카운트 캐싱·씬 단위 수명 관리 — 실전 자산 로딩.',
    href: '/guide/addressables/',
  },
  {
    key: 'localization',
    emoji: '🌐',
    titleId: 'home.feature.localization.title',
    titleDefault: 'Localization',
    descId: 'home.feature.localization.desc',
    descDefault: 'JSON / CSV 다국어 워크플로우, 로케일 전환, 키 상수 생성.',
    href: '/guide/localization/',
  },
];

function Hero() {
  return (
    <header className={styles.hero}>
      <div className={styles.heroInner}>
        <span className={styles.eyebrow}>
          <Translate id="home.eyebrow">Unity 통합 개발 툴킷</Translate>
        </span>
        <Heading as="h1" className={styles.title}>
          AchEngine
        </Heading>
        <p className={styles.tagline}>
          <Translate id="home.tagline">
            VContainer DI · UI System · Table Loader · Addressables · Localization 을 하나의 Unity 패키지로
          </Translate>
        </p>
        <div className={styles.buttons}>
          <Link
            className={`button button--primary button--lg ${styles.cta}`}
            to="/guide/installation"
          >
            <Translate id="home.cta.primary">시작하기 →</Translate>
          </Link>
          <Link
            className={`button button--secondary button--lg ${styles.cta}`}
            to="https://github.com/achieveonepark/AchEngine"
          >
            GitHub
          </Link>
        </div>
      </div>
    </header>
  );
}

function FeatureGrid() {
  return (
    <section className={styles.features}>
      <div className={styles.featureGrid}>
        {features.map(f => (
          <Link key={f.key} to={f.href} className={styles.card}>
            <span className={styles.cardEmoji} aria-hidden="true">{f.emoji}</span>
            <Heading as="h3" className={styles.cardTitle}>
              <Translate id={f.titleId}>{f.titleDefault}</Translate>
            </Heading>
            <p className={styles.cardDesc}>
              <Translate id={f.descId}>{f.descDefault}</Translate>
            </p>
            <span className={styles.cardArrow} aria-hidden="true">→</span>
          </Link>
        ))}
      </div>
    </section>
  );
}

export default function Home() {
  return (
    <Layout
      title="AchEngine"
      description={translate({
        id: 'home.meta.desc',
        message: 'Unity 통합 개발 툴킷 — VContainer DI, UI System, Addressables, Localization, Table Loader.',
      })}
    >
      <Hero />
      <FeatureGrid />
    </Layout>
  );
}
