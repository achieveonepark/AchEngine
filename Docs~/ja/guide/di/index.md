# DI システム — 概要

AchEngine の DI レイヤーは [VContainer](https://github.com/hadashiA/VContainer) を直接公開せず、
シンプルな抽象化レイヤーを提供します。

:::info オプションモジュール
VContainer（`jp.hadashikick.vcontainer`）がインストールされている場合にのみ、実際の DI コンテナが有効化されます。
未インストールの場合でも、`ServiceLocator` は手動セットアップで使用できます。
:::

## コアコンポーネント

| クラス | 役割 |
|---|---|
| `AchEngineScope` | VContainer の LifetimeScope をラップするシーンのエントリポイント |
| `AchEngineInstaller` | サービス登録を定義する抽象クラス |
| `IServiceBuilder` | サービス登録インターフェース（VContainer 非依存） |
| `ServiceLocator` | ランタイムでサービスを検索する静的ファサード |

## 基本的な使用フロー

```mermaid
flowchart TD
A([シーンロード]) --> B[AchEngineScope.Awake]
B --> C[/"AchEngineInstaller<br/>.Install(builder) × N"/]
C --> D[(VContainer<br/>コンテナビルド)]
D --> E[ServiceLocator.Setup]
E --> F{ランタイム}
F --> G["[Inject]<br/>アノテーション注入"]
F --> H["ServiceLocator<br/>.Resolve<T>()"]

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

## 次のステップ

- [AchEngineInstaller の詳細](/ja/guide/di/installer)
- [ServiceLocator の詳細](/ja/guide/di/locator)
- [DI ライフサイクルガイド](/ja/guide/di/lifecycle)
