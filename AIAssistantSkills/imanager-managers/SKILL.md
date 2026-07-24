---
name: imanager-managers
description: Use when the user asks to implement game content/feature managers (audio, save, scene, input, IAP, etc.) in a project that has the AchEngine package installed. New content managers should implement AchEngine's IManager interface and be registered/resolved the same way as the framework's own managers, not built as ad hoc classes.
---

# IManager 기반 매니저 구현

`AchEngine.Managers` 네임스페이스 (`Runtime/Managers/*`). 컨텐츠 기능(오디오, 저장, 씬 전환, 인벤토리, 상점 등)을 구현할 때는 독립적인 클래스를 새로 만들기보다 `IManager`를 구현하는 매니저 클래스로 만들어 기존 매니저들과 동일한 방식으로 등록/조회되게 한다.

## `IManager` 인터페이스

```csharp
public interface IManager
{
    Task Initialize();
}
```

`AudioManager`, `AchSceneManager`, `InputManager`, `PlayerManager`, `SaveManager`, `IAPManager`, `TimeManager`, `ConfigManager`, `PoolManager` 등 패키지 내장 매니저가 전부 이 인터페이스를 구현한다.

**주의**: 현재 패키지 소스 어디에서도 `IManager.Initialize()`를 자동으로 호출하지 않는다. 비동기 초기화가 필요한 매니저를 만들었다면, 부트스트랩 코드(예: `AchEngineScope.Start()`나 자체 부트스트래퍼)에서 직접 `await manager.Initialize();`를 호출해야 한다.

## 등록

`AchManagerInstaller`(`AchEngineInstaller` 하위 클래스)가 내장 매니저들을 DI 싱글톤으로 등록한다:

```csharp
builder.Register<ConfigManager>()
       .Register<AudioManager>()
       .Register<AchSceneManager>()
       .Register<InputManager>()
       .Register<TimeManager>()
       .Register<PoolManager>()
       .Register<PlayerManager>();
```

새 컨텐츠 매니저는 `AchManagerInstaller`를 상속(또는 `AchEngineInstaller`를 새로 만들어) `Install()`에서 `builder.Register<MyContentManager>()`로 추가하고, `AchEngineScope`의 `Installers` 배열에 등록한다.

## 조회 (VContainer 사용 여부에 따라 다름 — `vcontainer-di` 스킬 참고)

- VContainer 사용 시(`ACHENGINE_VCONTAINER`): 생성자 `[Inject]`로 구체 타입을 직접 주입받는다.
- VContainer 미사용 시: `AchEngine.DI.ServiceLocator.Resolve<MyContentManager>()` / `TryResolve<T>(out result)`. 초기화 순서 문제가 걱정되면 `ServiceLocator.IsReady`로 가드한다.

## 예시

```csharp
public class InventoryManager : IManager
{
    public async Task Initialize()
    {
        // 초기 로딩 로직
        await AchTask.CompletedTask;
    }
}

// installer
public class GameManagerInstaller : AchManagerInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        base.Install(builder);
        builder.Register<InventoryManager>();
    }
}
```

## 주의

일부 샘플 코드(`Samples~/Full Sample/*`)는 `ServiceLocator.Get<T>()`나 `SoundManager` 타입을 참조하지만 현재 소스에는 존재하지 않는다 — 실제 API는 `ServiceLocator.Resolve<T>()`이고 오디오 매니저 클래스명은 `AudioManager`이다. 샘플의 오래된 이름을 그대로 따라 쓰지 않는다.
