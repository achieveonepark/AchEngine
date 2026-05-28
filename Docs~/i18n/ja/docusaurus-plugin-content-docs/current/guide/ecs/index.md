# ECS ラッパー

AchEngine ECS モジュールは、Unity Entities の `World`、`EntityManager`、`EntityCommandBuffer`、`Baker`、`SystemBase` を薄くラップした便利 API です。

`com.unity.entities` パッケージがインストールされていると、`AchEngine.ECS` アセンブリが自動的に有効化されます。

```csharp
using AchEngine.ECS;
using Unity.Entities;
```

## どのような場合に使うのか

| 型 | 用途 |
|---|---|
| `AchSystemBase` | `SystemBase` で CommandBuffer と Query ヘルパーを短く使用 |
| `AchBaker<TAuthoring>` | Baker で Primary Entity にコンポーネントやバッファを簡単に追加 |
| `IEcsWorld` | DI サービスで ECS World と EntityManager を直接参照せずに使用 |
| `EntityManagerExtensions` | `TryGetComponent`、`SetOrAddComponent`、`GetOrAddBuffer` などの繰り返しコードを排除 |

## システムで使う

`SystemBase` の代わりに `AchSystemBase` を継承します。

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

## EntityManager 拡張メソッド

既存の `EntityManager` に便利メソッドを追加します。

```csharp
EntityManager.SetOrAddComponent(entity, new Health { Value = 100 });

if (EntityManager.TryGetComponent<Health>(entity, out var health))
{
    UnityEngine.Debug.Log(health.Value);
}

EntityManager.RemoveComponentIfExists<Health>(entity);
```

## Baker で使う

`Baker<T>` の代わりに `AchBaker<T>` を継承すると、Primary Entity にコンポーネントを付与するコードが短くなります。

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

## DI で ECS World を注入する

シーンの `AchEngineScope` Installers に `AchEngineEcsInstaller` を追加すると、`IEcsWorld` をサービスとして受け取れます。

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

`AchEngineEcsInstaller` の World Name を空のままにすると `World.DefaultGameObjectInjectionWorld` を使用し、値を入力すると同じ名前の World を探して登録します。
