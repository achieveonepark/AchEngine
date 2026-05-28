# 빠른 시작

AchEngine의 핵심 기능을 5분 안에 경험해보는 가이드입니다.

## 1. DI 스코프 설정

씬에 빈 GameObject를 생성하고 `AchEngineScope` 컴포넌트를 추가합니다.

```
Hierarchy
└── [AchEngineScope]   ← AchEngineScope 컴포넌트 추가
```

## 2. Installer 작성

```csharp
using AchEngine.DI;
using UnityEngine;

public class GameInstaller : AchEngineInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder
            .Register<IGameService, GameService>()
            .Register<IPlayerService, PlayerService>(ServiceLifetime.Transient);
    }
}
```

`AchEngineScope` Inspector의 **Installers** 배열에 `GameInstaller`를 드래그합니다.

## 3. UI Root 생성

**Project Settings › AchEngine › UI Workspace** 에서 **UI Root 생성** 버튼을 클릭하거나,
씬에 `UIRoot` 프리팹을 배치합니다.

## 4. UIView 정의

```csharp
using AchEngine.UI;

public class MainMenuView : UIView
{
    protected override void OnInitialize()
    {
        // 최초 생성 시 1회 호출
    }

    protected override void OnOpened()
    {
        // Show() 후 트랜지션 완료 시 호출
    }

    protected override void OnClosed()
    {
        // Close() 후 Pool에 반환됨
    }
}
```

## 5. View 표시

```csharp
// [Inject] 사용 (VContainer 필요)
[Inject] readonly IUIService _ui;
_ui.Show<MainMenuView>();

// ServiceLocator 사용 (MonoBehaviour 등)
ServiceLocator.Resolve<IUIService>().Show("MainMenu");

// 닫기
_ui.Close<MainMenuView>();
```

## 6. 테이블 데이터 로드

```csharp
// TableManager를 통해 타입-세이프하게 접근
var itemTable = TableManager.Get<ItemTable>();
var sword = itemTable.Get(101);
Debug.Log(sword.Name); // "Iron Sword"
```

## 다음 단계

각 모듈의 상세 설명은 좌측 사이드바를 통해 확인하세요.

- [DI 시스템 자세히 보기](/guide/di/)
- [UI System 자세히 보기](/guide/ui/)
- [Table Loader 자세히 보기](/guide/table/)
- [Addressables 자세히 보기](/guide/addressables/)
- [Localization 자세히 보기](/guide/localization/)
- [모듈 연계 가이드](/guide/integration)
