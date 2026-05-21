# PoolManager

封装 Unity `ObjectPool<GameObject>` 的对象池管理器。可减少子弹、粒子等频繁创建/销毁对象所产生的 GC 压力。

## API

```csharp
var pool = ServiceLocator.Resolve<PoolManager>();

// 풀 등록
pool.RegisterPool("Bullet", bulletPrefab, defaultCapacity: 20, maxSize: 100);

// 꺼내기
var bullet = pool.Get<BulletComponent>("Bullet");
var go     = pool.Get("Bullet");   // 컴포넌트 없이 GameObject 그대로

// 반환
pool.Release("Bullet", go);

// 풀 정리
pool.ClearPool("Bullet");
pool.ClearAll();
```
