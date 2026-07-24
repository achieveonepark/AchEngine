---
name: ecs-ingame-logic
description: Use when the user asks to implement in-game/gameplay logic (units, enemies, bullets, large-scale simulation) in a project that has the AchEngine package installed, and ECS/DOTS is a plausible architecture choice. Always ask the user whether they want ECS/DOTS before applying this skill — do not assume it.
---

# 인게임 로직 아키텍처 — ECS 사용 여부 확인 먼저

**중요: 이 스킬을 적용하기 전에 반드시 사용자에게 "이 컨텐츠를 Unity ECS(DOTS)로 구현할지, 일반 MonoBehaviour로 구현할지" 먼저 물어본다.** ECS는 `com.unity.entities` 패키지가 설치되어 있어야 컴파일되는 선택적 모듈이며, 별도 어셈블리(`AchEngine.ECS`, `defineConstraints: ["ACHENGINE_ENTITIES"]`)로 분리되어 있어 모든 프로젝트에 강제되지 않습니다.

사용자가 ECS를 원한다고 확인된 경우에만 아래 AchEngine ECS 래퍼(`AchEngine.ECS` 네임스페이스, `Runtime/ECS/*`)를 사용합니다.

## 제공 API

- **`Core/IEcsWorld` / `EcsWorld`** — `World`/`EntityManager` 래퍼. `CreateEntity`, `Instantiate`, `Destroy`, `Exists`, `HasComponent<T>`, `GetComponent<T>`, `SetComponent<T>`, `AddComponent<T>`, `RemoveComponent<T>`, 버퍼 변형(`HasBuffer`, `GetBuffer`, `AddBuffer`), `CreateQuery(...)`, `CreateCommandBuffer(Allocator)`.
- **`Core/EcsQueryDescription`** — `EntityQueryDesc` 래퍼. `AllOf`/`AnyOf`/`NoneOf` 팩토리 + `WithAny`/`WithNone` 체이닝, `.ToEntityQueryDesc()`로 변환.
- **`Core/IEcsCommandBuffer` / `EcsCommandBuffer`** — `EntityCommandBuffer` 래퍼. `CreateEntity`, `Instantiate`, `AddComponent/SetComponent/RemoveComponent`, `AddBuffer`, `Destroy`, `Playback(EntityManager)`, `IDisposable`.
- **`Systems/AchSystemBase`** — `SystemBase`를 대신 상속하는 추상 클래스. `Ecs`(`IEcsWorld`), `CreateCommandBuffer(...)`, `QueryAll(...)`/`Query(EcsQueryDescription)` 헬퍼 제공. 새 시스템은 `SystemBase` 대신 이걸 상속한다.
- **`Authoring/AchBaker<TAuthoring>`** — `Baker<TAuthoring>` 대신 상속. `PrimaryEntity(TransformUsageFlags)`, `AddComponentToPrimary<T>(component, flags)`, `AddBufferToPrimary<T>(flags)` 제공.
- **`DI/AchEngineEcsInstaller`** — VContainer를 쓴다면 `AchEngineInstaller`로서 `World`(기본값 `World.DefaultGameObjectInjectionWorld` 또는 이름 매칭)를 찾아 `IEcsWorld`로 DI 컨테이너에 등록한다. ECS↔VContainer 연결은 이것 하나뿐이며, 시스템 단위 자동 주입은 없다.

## 예시 패턴

```csharp
public partial class DamageSystem : AchSystemBase
{
    protected override void OnUpdate()
    {
        using var ecb = CreateCommandBuffer(Allocator.TempJob);
        foreach (var (health, entity) in QueryAll(EcsQueryDescription.AllOf(typeof(Health))))
        {
            // ...
        }
    }
}

public class UnitAuthoringBaker : AchBaker<UnitAuthoring>
{
    public override void Bake(UnitAuthoring authoring)
    {
        var entity = PrimaryEntity(TransformUsageFlags.Dynamic);
        AddComponentToPrimary(new Health { Value = authoring.MaxHealth });
    }
}
```

사용자가 ECS를 원하지 않는다고 답하면 일반 `MonoBehaviour` + AchEngine의 다른 시스템(`AchMover`, `IManager` 등)으로 구현한다.
