# PoolManager

Unity `ObjectPool<GameObject>`를 래핑한 오브젝트 풀링 매니저입니다. 총알·파티클처럼 반복 생성/삭제되는 오브젝트의 GC 부하를 줄입니다.

## API

```csharp
var pool = ServiceLocator.Get<PoolManager>();

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
