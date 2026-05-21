# マネージャーシステム

AchEngine のマネージャーシステムは、ゲームで繰り返し必要となる機能を DI で登録し、`ServiceLocator` を通じてどこからでもアクセスできるようにします。

## 提供マネージャー

| マネージャー | 役割 |
|---|---|
| `ConfigManager` | PlayerPrefs ベースの設定値の保存・読み込み |
| `AudioManager` | BGM / SFX 再生、フェード、ミュート、3D 空間音響 |
| `AchSceneManager` | 非同期シーン遷移、IScene ライフサイクル |
| `InputManager` | 入力の有効化・無効化ラッパー |
| `TimeManager` | ネットワーク同期時刻、1 秒ごとのイベント |
| `PoolManager` | プレハブベースのオブジェクトプーリング |
| `PlayerManager` | プレイヤーデータコンテナの管理 (`Add` / `Get` / `Remove`) |
| `SaveManager` | 保存・読み込み・削除 — `ISaveService` ベースの抽象化(別途 DI 登録が必要) |
| `IAPManager` | Unity IAP 5.3.0 連携スタブ(別途 DI 登録が必要) |

## クイックスタート

### 1. AchManagerInstaller の追加

`AchEngineScope` の Installers 配列に `AchManagerInstaller` コンポーネントを追加します。

```csharp
// 씬의 AchEngineScope GameObject에 AchManagerInstaller 컴포넌트를 추가하기만 하면 됩니다.
// 모든 매니저가 자동으로 DI 컨테이너에 싱글톤으로 등록됩니다.
```

選択的に登録したい場合は `AchManagerInstaller` を継承してください。

```csharp
public class MyInstaller : AchManagerInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder
            .Register<ConfigManager>()
            .Register<AudioManager>();
        // 필요한 것만 등록
    }
}
```

### 2. マネージャーへのアクセス

```csharp
using AchEngine.DI;
using AchEngine.Managers;

var config = ServiceLocator.Resolve<ConfigManager>();
var audio  = ServiceLocator.Resolve<AudioManager>();
```

## IScene ライフサイクル

シーンのルート GameObject に `IScene` を実装した MonoBehaviour をアタッチすると、`AchSceneManager` がシーン遷移時に自動で `OnSceneStart` / `OnSceneEnd` を呼び出します。

```csharp
using AchEngine.Managers;

public class LobbyScene : MonoBehaviour, IScene
{
    public async Task OnSceneStart()
    {
        await LoadUserDataAsync();
    }

    public Task OnSceneEnd() => Task.CompletedTask;
}
```

> ランタイムの async API は `System.Threading.Tasks.Task` を使用します。
