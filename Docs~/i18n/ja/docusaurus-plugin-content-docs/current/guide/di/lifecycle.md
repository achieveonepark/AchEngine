# DI ライフサイクル設計

AchEngine において、シーン遷移、ポップアップ、データ供給、インゲームロジックを DI で接続する全体フローを扱います。

## 全体構造

```mermaid
graph TB
subgraph Bootstrap["🌐 Bootstrap シーン (常に維持)"]
AES[AchEngineScope<br/>グローバルサービス登録]
UIR[UIRoot<br/>レイヤー Canvas 管理]
end

subgraph Lobby["🏠 Lobby シーン (additive)"]
LS[LobbyScope<br/>ロビー専用サービス]
LV[LobbyView<br/>ShopPopup ...]
end

subgraph InGame["⚔ InGame シーン (additive)"]
GS[GameScope<br/>ゲーム専用サービス]
HV[HUDView<br/>PausePopup ...]
end

Bootstrap --> Lobby
Bootstrap --> InGame
AES -.->|ServiceLocator| LS
AES -.->|ServiceLocator| GS

style Bootstrap fill:#0f2d4a,stroke:#3b82f6,color:#93c5fd
style Lobby     fill:#0f3a1f,stroke:#10b981,color:#6ee7b7
style InGame    fill:#3a1010,stroke:#ef4444,color:#fca5a5
style AES       fill:#1e3a5f,stroke:#3b82f6,color:#e2e8f0
style UIR       fill:#1e3a5f,stroke:#8b5cf6,color:#e2e8f0
style LS        fill:#1a4a2a,stroke:#10b981,color:#e2e8f0
style LV        fill:#1a4a2a,stroke:#10b981,color:#e2e8f0
style GS        fill:#4a1a1a,stroke:#ef4444,color:#e2e8f0
style HV        fill:#4a1a1a,stroke:#ef4444,color:#e2e8f0
```

## 1. Bootstrap シーン — グローバルサービス登録

ゲーム全体の寿命にわたって維持されるサービスを `Bootstrap` シーンで登録します。

```csharp
// GlobalInstaller.cs
public class GlobalInstaller : AchEngineInstaller
{
    [SerializeField] private GameConfig _config;

    public override void Install(IServiceBuilder builder)
    {
        builder
            // 설정 데이터
            .RegisterInstance<IGameConfig>(_config)
            // 테이블 데이터 서비스
            .Register<ITableService, TableService>()
            // UI 서비스 (자동 등록되지만 명시도 가능)
            .Register<IUIService, UIService>()
            // 사운드, 네트워크 등 전역 서비스
            .Register<IAudioService, AudioService>()
            .Register<INetworkService, NetworkService>();
    }
}
```

```
[Bootstrap 씬]
 └── [AchEngineScope]  Installers: [GlobalInstaller]
 └── [UIRoot]
```

## 2. シーン遷移 — Lobby → InGame

シーン単位で別の `AchEngineScope` を配置し、シーン専用のサービスを登録・解除します。

```csharp
// SceneService.cs — Bootstrap 씬에 등록된 전역 서비스
public class SceneService : ISceneService
{
    public async Task LoadLobby()
    {
        // 기존 게임 씬 언로드
        await UnloadCurrentGameScene();

        // Lobby 씬 additive 로드
        await SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Additive);

        // 로비 진입 UI 표시
        ServiceLocator.Resolve<IUIService>().Show<LobbyView>();
    }

    public async Task LoadInGame(int stageId)
    {
        // 로비 UI 닫기
        ServiceLocator.Resolve<IUIService>().CloseAll();

        // 로비 씬 언로드 → InGame 씬 로드
        await SceneManager.UnloadSceneAsync("Lobby");
        await SceneManager.LoadSceneAsync("InGame", LoadSceneMode.Additive);

        // 게임 시작
        ServiceLocator.Resolve<IGameService>().StartStage(stageId);
    }
}
```

```csharp
// LobbyInstaller.cs — Lobby 씬 전용
public class LobbyInstaller : AchEngineInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder
            .Register<IShopService, ShopService>()
            .Register<IFriendService, FriendService>();
    }
}
```

```csharp
// GameInstaller.cs — InGame 씬 전용
public class GameInstaller : AchEngineInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder
            .Register<IGameService, GameService>()
            .Register<IEnemySpawner, EnemySpawner>()
            .Register<IStageService, StageService>();
    }
}
```

:::tip シーン Scope のライフサイクル
シーンがアンロードされると `AchEngineScope.OnDestroy()` が呼び出され、
そのシーンのサービスが自動的にコンテナから解除されます。
:::

## 3. ポップアップ生成 — データ伝達

ポップアップは `UIView` を継承し、`Show()` コールバックでデータを注入します。

```csharp
// ItemDetailPopup.cs
public class ItemDetailPopup : UIView
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _descText;
    [SerializeField] private Text _priceText;
    [SerializeField] private Image _iconImage;

    private ItemData _item;

    public override UILayerId Layer => UILayerId.Popup;

    protected override void OnInitialize()
    {
        // 닫기 버튼 등 한 번만 세팅
    }

    // 외부에서 데이터 주입 (Show 콜백에서 호출)
    public void SetItem(ItemData item, Sprite icon)
    {
        _item = item;
        _nameText.text  = LocalizationManager.Get(L.Item.Name(item.Id));
        _descText.text  = LocalizationManager.Get(L.Item.Desc(item.Id));
        _priceText.text = $"{item.Price:N0} G";
        _iconImage.sprite = icon;
    }

    protected override void OnClosed()
    {
        _item = null;
        _iconImage.sprite = null;
    }
}
```

```csharp
// 팝업 열기 (인벤토리 등에서 호출)
var ui   = ServiceLocator.Resolve<IUIService>();
var icon = await AddressableManager.LoadAsync<Sprite>($"icon_{item.Id}");

ui.Show<ItemDetailPopup>(popup => popup.SetItem(item, icon.Result));
```

## 4. データ供給 — Table → Service

`TableService` がベイクされたデータをラップし、他のサービスがそれを注入されて使用します。

```csharp
// GameService.cs
public class GameService : IGameService
{
    private readonly ITableService _tables;
    private readonly IUIService    _ui;

    // DI 생성자 주입
    public GameService(ITableService tables, IUIService ui)
    {
        _tables = tables;
        _ui     = ui;
    }

    public void StartStage(int stageId)
    {
        var stageData = _tables.Get<StageTable>().Get(stageId);
        var enemies   = _tables.Get<EnemyTable>().GetByStage(stageId);

        _ui.Show<GameHUDView>(hud => hud.SetStage(stageData));

        foreach (var enemy in enemies)
            SpawnEnemy(enemy);
    }
}
```

## 5. インゲーム — MonoBehaviour からのアクセス

`MonoBehaviour` は DI コンテナが直接生成しないため、`ServiceLocator` を使用します。

```csharp
// PlayerController.cs
public class PlayerController : MonoBehaviour
{
    private IGameService   _gameService;
    private IAudioService  _audioService;

    private void Start()
    {
        // ServiceLocator로 런타임 조회
        _gameService  = ServiceLocator.Resolve<IGameService>();
        _audioService = ServiceLocator.Resolve<IAudioService>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IEnemy>(out var enemy))
        {
            _gameService.OnPlayerHit(enemy.Damage);
            _audioService.PlaySFX("hit");
        }
    }
}
```

:::tip [Inject] が使用可能なケース
コンテナが生成するオブジェクト（`Register<PlayerController>()` 後に `Instantiate` なしで
VContainer が直接生成）には `[Inject]` を使用できます。
:::

## 全体フローのまとめ

```
앱 시작
 └── Bootstrap 씬 로드
      └── AchEngineScope → GlobalInstaller → 전역 서비스 등록
           └── ServiceLocator 초기화

씬 전환
 └── ISceneService.LoadLobby()
      └── Lobby 씬 additive 로드
           └── LobbyScope → LobbyInstaller → 로비 서비스 추가 등록

팝업
 └── IUIService.Show<ItemDetailPopup>(popup => popup.SetItem(...))

인게임
 └── MonoBehaviour → [Inject] 속성 주입
```

## 次のステップ

- [全モジュール連携例](/guide/integration)
- [AchEngineInstaller の詳細](/guide/di/installer)
