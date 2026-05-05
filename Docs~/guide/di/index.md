# DI 시스템 — 개요

AchEngine의 DI 레이어는 [VContainer](https://github.com/hadashiA/VContainer)를 직접 노출하지 않고,
간단한 추상화 레이어를 제공합니다.

:::info 선택적 모듈
VContainer(`jp.hadashikick.vcontainer`)가 설치된 경우에만 실제 DI 컨테이너가 활성화됩니다.
미설치 시에도 `ServiceLocator`는 수동 설정으로 사용할 수 있습니다.
:::

## 핵심 구성 요소

| 클래스 | 역할 |
|---|---|
| `AchEngineScope` | VContainer의 LifetimeScope를 래핑한 씬 진입점 |
| `AchEngineInstaller` | 서비스 등록을 정의하는 추상 클래스 |
| `IServiceBuilder` | 서비스 등록 인터페이스 (VContainer 비의존) |
| `ServiceLocator` | 런타임에 서비스를 조회하는 정적 파사드 |

## 기본 사용 흐름

```mermaid
flowchart TD
    A([씬 로드]) --> B[AchEngineScope.Awake]
    B --> C[/"AchEngineInstaller\n.Install(builder) × N"/]
    C --> D[(VContainer\n컨테이너 빌드)]
    D --> E[ServiceLocator.Setup]
    E --> F{런타임}
    F --> G["[Inject]\n어노테이션 주입"]
    F --> H["ServiceLocator\n.Resolve&lt;T&gt;()"]

    style A fill:#1e3a5f,stroke:#3b82f6,color:#e2e8f0
    style B fill:#1e3a5f,stroke:#3b82f6,color:#e2e8f0
    style C fill:#0f2d4a,stroke:#3b82f6,color:#93c5fd
    style D fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
    style E fill:#1e3a5f,stroke:#10b981,color:#6ee7b7
    style F fill:#162032,stroke:#64748b,color:#cbd5e1
    style G fill:#1e3a5f,stroke:#8b5cf6,color:#c4b5fd
    style H fill:#1e3a5f,stroke:#f59e0b,color:#fcd34d
```

## ServiceLifetime

```csharp
public enum ServiceLifetime
{
    Singleton,   // 컨테이너당 1개 인스턴스 (기본값)
    Transient,   // 요청마다 새 인스턴스
    Scoped,      // 스코프당 1개 인스턴스
}
```

## 다음 단계

- [AchEngineInstaller 자세히 보기](/guide/di/installer)
- [ServiceLocator 자세히 보기](/guide/di/locator)
- [DI 라이프사이클 가이드](/guide/di/lifecycle)
