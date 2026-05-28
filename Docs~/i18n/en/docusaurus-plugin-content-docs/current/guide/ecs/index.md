# ECS Wrapper

The AchEngine ECS module is a thin convenience layer over Unity Entities `World`, `EntityManager`, `EntityCommandBuffer`, `Baker`, and `SystemBase`.

When `com.unity.entities` is installed, the `AchEngine.ECS` assembly is enabled automatically.

```csharp
using AchEngine.ECS;
using Unity.Entities;
```

## When To Use It

| Type | Purpose |
|---|---|
| `AchSystemBase` | Short helpers for CommandBuffer and Query usage in `SystemBase` |
| `AchBaker<TAuthoring>` | Add components or buffers to the primary entity with less boilerplate |
| `IEcsWorld` | Use ECS World and EntityManager from DI-backed services |
| `EntityManagerExtensions` | Remove repeated code with helpers such as `TryGetComponent`, `SetOrAddComponent`, and `GetOrAddBuffer` |

## Systems

Derive from `AchSystemBase` instead of `SystemBase`.

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

## EntityManager Extensions

The module adds convenience methods to the existing `EntityManager`.

```csharp
EntityManager.SetOrAddComponent(entity, new Health { Value = 100 });

if (EntityManager.TryGetComponent<Health>(entity, out var health))
{
    UnityEngine.Debug.Log(health.Value);
}

EntityManager.RemoveComponentIfExists<Health>(entity);
```

## Bakers

Derive from `AchBaker<T>` instead of `Baker<T>` to shorten primary entity authoring code.

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

## DI

Add `AchEngineEcsInstaller` to the `AchEngineScope` Installers list to inject `IEcsWorld` into services.

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

If World Name is empty, `AchEngineEcsInstaller` uses `World.DefaultGameObjectInjectionWorld`. If a name is provided, it registers the World with that exact name.
