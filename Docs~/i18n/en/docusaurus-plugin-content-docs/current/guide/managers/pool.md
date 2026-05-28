# PoolManager

Object pooling manager wrapping Unity's `ObjectPool<GameObject>`. Reduces GC pressure for frequently spawned/despawned objects like bullets and particles.

## API

```csharp
var pool = ServiceLocator.Resolve<PoolManager>();

// Register a pool
pool.RegisterPool("Bullet", bulletPrefab, defaultCapacity: 20, maxSize: 100);

// Retrieve an instance
var bullet = pool.Get<BulletComponent>("Bullet");
var go     = pool.Get("Bullet");   // returns the raw GameObject

// Return an instance
pool.Release("Bullet", go);

// Clean up
pool.ClearPool("Bullet");
pool.ClearAll();
```
