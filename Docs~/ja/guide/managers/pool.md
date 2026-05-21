# PoolManager

Unity の `ObjectPool<GameObject>` をラップしたオブジェクトプーリングマネージャーです。弾丸やパーティクルのように繰り返し生成・破棄されるオブジェクトの GC 負荷を軽減します。

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
