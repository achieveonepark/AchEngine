# ECS 래퍼

AchEngine ECS 모듈은 Unity Entities의 `World`, `EntityManager`, `EntityCommandBuffer`, `Baker`, `SystemBase`를 얇게 감싼 편의 API입니다.

`com.unity.entities` 패키지가 설치되어 있으면 `AchEngine.ECS` 어셈블리가 자동으로 활성화됩니다.

```csharp
using AchEngine.ECS;
using Unity.Entities;
```

## 언제 사용하나요?

| 타입 | 용도 |
|---|---|
| `AchSystemBase` | `SystemBase`에서 CommandBuffer와 Query 헬퍼를 짧게 사용 |
| `AchBaker<TAuthoring>` | Baker에서 Primary Entity에 컴포넌트나 버퍼를 간단히 추가 |
| `IEcsWorld` | DI 서비스에서 ECS World와 EntityManager를 직접 참조하지 않고 사용 |
| `EntityManagerExtensions` | `TryGetComponent`, `SetOrAddComponent`, `GetOrAddBuffer` 같은 반복 코드 제거 |

## 시스템에서 사용하기

`SystemBase` 대신 `AchSystemBase`를 상속합니다.

```csharp
public struct Health : IComponentData
{
    public int Value;
}

public sealed partial class DamageSystem : AchSystemBase
{
    protected override void OnUpdate()
    {
        using var ecb = CreateCommandBuffer();

        foreach (var (health, entity) in
                 SystemAPI.Query<RefRW<Health>>().WithEntityAccess())
        {
            health.ValueRW.Value -= 1;

            if (health.ValueRO.Value <= 0)
            {
                ecb.Destroy(entity);
            }
        }

        ecb.Playback(EntityManager);
    }
}
```

## EntityManager 확장 메서드

기존 `EntityManager`에 편의 메서드를 추가합니다.

```csharp
EntityManager.SetOrAddComponent(entity, new Health { Value = 100 });

if (EntityManager.TryGetComponent<Health>(entity, out var health))
{
    UnityEngine.Debug.Log(health.Value);
}

EntityManager.RemoveComponentIfExists<Health>(entity);
```

## Baker에서 사용하기

`Baker<T>` 대신 `AchBaker<T>`를 상속하면 Primary Entity에 컴포넌트를 붙이는 코드가 짧아집니다.

```csharp
using AchEngine.ECS;
using Unity.Entities;
using UnityEngine;

public class CharacterAuthoring : MonoBehaviour
{
    public int Health = 100;
}

public class CharacterBaker : AchBaker<CharacterAuthoring>
{
    public override void Bake(CharacterAuthoring authoring)
    {
        AddComponentToPrimary(new Health { Value = authoring.Health });
    }
}
```

## DI에서 ECS World 주입받기

씬의 `AchEngineScope` Installers에 `AchEngineEcsInstaller`를 추가하면 `IEcsWorld`를 서비스로 받을 수 있습니다.

```csharp
using AchEngine.ECS;
using Unity.Entities;

public class SpawnService
{
    private readonly IEcsWorld _ecs;

    public SpawnService(IEcsWorld ecs)
    {
        _ecs = ecs;
    }

    public Entity Spawn(Entity prefab)
    {
        return _ecs.Instantiate(prefab);
    }
}
```

`AchEngineEcsInstaller`의 World Name을 비워두면 `World.DefaultGameObjectInjectionWorld`를 사용하고, 값을 입력하면 같은 이름의 World를 찾아 등록합니다.
