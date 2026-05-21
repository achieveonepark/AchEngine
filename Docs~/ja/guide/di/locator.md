# ServiceLocator

:::warning VContainer 未インストール時専用
`ACHENGINE_VCONTAINER` シンボルが定義された環境（VContainer インストール時）では、`ServiceLocator` はコンパイルされません。
VContainer プロジェクトでは `[Inject]` を使用してください。
:::

`ServiceLocator` は、VContainer なしでランタイムにサービスを型で検索する静的ファサードです。

## API

```csharp
namespace AchEngine.DI
{
    public static class ServiceLocator
    {
        // 컨테이너가 준비되었는지 여부
        public static bool IsReady { get; }

        // 서비스 조회 (없으면 InvalidOperationException)
        public static T Resolve<T>();

        // 안전한 서비스 조회 (없으면 false 반환)
        public static bool TryResolve<T>(out T result);
    }
}
```

## 使用例

```csharp
// 기본 조회
var ui = ServiceLocator.Resolve<IUIService>();
ui.Show<MainMenuView>();

// 안전한 조회
if (ServiceLocator.TryResolve<IAudioService>(out var audio))
{
    audio.PlayBGM("main_theme");
}

// 준비 여부 확인
if (!ServiceLocator.IsReady)
{
    Debug.LogWarning("서비스 컨테이너가 아직 초기화되지 않았습니다.");
    return;
}
```

## `[Inject]` vs `ServiceLocator`

| | `[Inject]` | `ServiceLocator` |
|---|---|---|
| VContainer | ✅ 必要 | ❌ VContainer 未インストール時のみ使用可能 |
| 使用場所 | DI コンテナが生成したオブジェクト | どこからでも（VContainer なしの環境） |
| 推奨される状況 | すべてのサービス・View | VContainer なしの MonoBehaviour |
| テストの容易さ | 高い | 中程度 |

## 手動初期化（VContainer がない場合）

VContainer なしで `ServiceLocator` を使用するには、エンジン内部で `Setup()` が呼び出されます。
直接呼び出すパブリック API は提供されておらず、`AchEngineScope` のような内部ブートストラップコードが
コンテナ準備時に自動的に接続します。

:::info 非 VContainer 環境
`ACHENGINE_VCONTAINER` シンボルなしでビルドする場合、カスタム `AchEngineInstaller` を実装して
`IServiceBuilder` にサービスを登録すれば、ランタイムで `ServiceLocator.Resolve<T>()` でアクセスできます。
:::

## 関連ドキュメント

- [DI システム概要](/ja/guide/di/index)
- [DI ライフサイクル設計](/ja/guide/di/lifecycle)
