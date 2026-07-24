---
name: vcontainer-di
description: Use when the user asks to structure a project's architecture, wire up dependency injection, or set up a bootstrap scene in a project that has the AchEngine package installed. Always ask the user whether they want to use VContainer-based DI (AchEngineScope) or the no-DI path (ServiceLocator/UIBootstrapper) before choosing — do not assume either.
---

# 아키텍처 — VContainer DI 사용 여부 확인 먼저

**중요: 이 스킬을 적용하기 전에 반드시 사용자에게 "이 프로젝트의 아키텍처를 VContainer 기반 DI로 구성할지" 먼저 물어본다.** AchEngine은 VContainer 사용 여부에 따라 완전히 다른 두 경로를 모두 지원하며, 어느 쪽이 적합한지는 프로젝트 규모/팀 성향에 달려 있다.

`AchEngine.DI` 네임스페이스 (`Runtime/DI/*`). 전환은 `ACHENGINE_VCONTAINER` 스크립팅 심볼 하나로 이루어지며(`jp.hadashikick.vcontainer` 패키지 설치 시 자동 설정), 수동 토글은 없다.

## VContainer 경로 (사용자가 DI를 원할 때)

- **`AchEngineScope`** (`#if ACHENGINE_VCONTAINER`, `LifetimeScope` 하위, `[DisallowMultipleComponent]`) — 메인 부트스트랩 컴포넌트. 인스펙터 필드: `catalog`(`UIViewCatalog`), `uiRoot`(`autoCreateRoot`면 자동 탐색/생성), `prewarmOnStart`, `makePersistent`, `installers`(`AchEngineInstaller[]`). `Configure()`에서 `TableDatabase`→`ITableDatabase`, `TableService`→`ITableService`, `UIService` 프리팹 싱글톤을 자동 등록하고 지정된 `AchEngineInstaller.Install()`을 실행한다. `Start()`에서 `UIService`를 resolve해 `Initialize(catalog, uiRoot)`, `UI.SetService(...)`, `TableManager.SetService(...)`를 연결한다.
- **`AchEngineInstaller`** — 사용자 정의 installer의 추상 베이스: `public abstract void Install(IServiceBuilder builder);`. `AchEngineScope.Installers[]`에 추가.
- **`IServiceBuilder` / `ServiceLifetime`**(`Singleton|Transient|Scoped`) — VContainer에 종속되지 않는 등록 파사드: `Register<T>(lifetime)`, `Register<TInterface,TImpl>(lifetime)`, `RegisterInstance<T>(instance)`, `RegisterComponent<T>(component)`.
- 매니저 등록은 `AchManagerInstaller`(`imanager-managers` 스킬 참고), ECS는 `AchEngineEcsInstaller`(`ecs-ingame-logic` 스킬 참고)로 `AchEngineScope.Installers[]`에 추가한다.

```csharp
public class SampleGameScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
        => builder.RegisterEntryPoint<GameBootstrap>();
}
public class GameBootstrap : IStartable
{
    [Inject] public GameBootstrap(ITableService tableService, IUIService uiService) { ... }
    public void Start() => _uiService.Show("MainMenu");
}
```

## No-DI 경로 (사용자가 DI를 원하지 않을 때)

- **`ServiceLocator`** (`#if !ACHENGINE_VCONTAINER`, VContainer가 설치되어 있으면 아예 컴파일 안 됨) — `Resolve<T>()`(설정 전 호출 시 예외), `TryResolve<T>(out T)`, `IsReady`. `Setup(Func<Type,object> resolver)`로 직접 초기화해야 하며, 패키지 안에 자동 호출하는 곳은 없다 — 이 경로를 쓰려면 부트스트랩 코드에서 직접 `Setup`을 호출해야 한다.
- UI는 `UIBootstrapper`(`ui-catalog` 스킬 참고)로 부트스트랩한다.
- 즉석 전역 객체는 `Singleton<T>`/`MonoSingleton<T>`(`singleton` 스킬 참고)를 사용한다.

```csharp
public class GameBootstrapNoDI : MonoBehaviour
{
    private void Start()
    {
        if (!UI.IsReady) return;   // 씬에 UIBootstrapper 필요
        UI.Show("MainMenu");
    }
}
```

## 참고

`TableManager`(static, `ITableService` 파사드: `Load<T>`, `Get<T>(id)`, `TryGet<T>`, `GetAll<T>`)도 `AchEngineScope`를 통해 자동 등록되며, DI 서비스가 없으면 내부 `TableService` 인스턴스로 폴백한다.
