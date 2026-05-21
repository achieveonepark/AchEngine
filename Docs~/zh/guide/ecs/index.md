# ECS 封装

AchEngine ECS 模块是对 Unity Entities 的 `World`、`EntityManager`、`EntityCommandBuffer`、`Baker`、`SystemBase` 进行轻量封装的便捷 API。

安装 `com.unity.entities` 包后，`AchEngine.ECS` 程序集会自动启用。

```csharp
using AchEngine.ECS;
using Unity.Entities;
```

## 何时使用

| 类型 | 用途 |
|---|---|
| `AchSystemBase` | 在 `SystemBase` 中简短使用 CommandBuffer 和 Query 辅助方法 |
| `AchBaker<TAuthoring>` | 在 Baker 中向 Primary Entity 简便地添加组件或缓冲区 |
| `IEcsWorld` | 在 DI 服务中无需直接引用 ECS World 和 EntityManager 即可使用 |
| `EntityManagerExtensions` | 通过 `TryGetComponent`、`SetOrAddComponent`、`GetOrAddBuffer` 等消除重复代码 |

## 在系统中使用

继承 `AchSystemBase` 而非 `SystemBase`。

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

## EntityManager 扩展方法

为现有的 `EntityManager` 添加便捷方法。

```csharp
EntityManager.SetOrAddComponent(entity, new Health { Value = 100 });

if (EntityManager.TryGetComponent<Health>(entity, out var health))
{
    UnityEngine.Debug.Log(health.Value);
}

EntityManager.RemoveComponentIfExists<Health>(entity);
```

## 在 Baker 中使用

继承 `AchBaker<T>` 而非 `Baker<T>`，可以缩短向 Primary Entity 附加组件的代码。

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

## 通过 DI 注入 ECS World

将 `AchEngineEcsInstaller` 添加到场景的 `AchEngineScope` Installers 中，即可将 `IEcsWorld` 作为服务接收。

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

将 `AchEngineEcsInstaller` 的 World Name 留空时使用 `World.DefaultGameObjectInjectionWorld`，输入值则查找并注册同名的 World。
