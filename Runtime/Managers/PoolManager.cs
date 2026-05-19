using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace AchEngine.Managers
{
    /// <summary>
    /// GameObject 오브젝트 풀을 키 기반으로 등록·관리하는 매니저.
    /// Unity의 ObjectPool을 내부적으로 사용한다.
    /// </summary>
    public class PoolManager : IManager
    {
        private readonly Dictionary<string, IObjectPool<GameObject>> _pools = new();

        /// <summary>
        /// 초기화. PoolManager는 별도 초기화 작업이 없다.
        /// </summary>
        public Task Initialize() => Task.CompletedTask;

        /// <summary>
        /// 지정한 키로 프리팹 오브젝트 풀을 등록한다.
        /// 이미 같은 키가 등록되어 있으면 경고 후 무시한다.
        /// </summary>
        /// <param name="key">풀을 식별하는 고유 키.</param>
        /// <param name="prefab">풀에서 생성할 기준 프리팹.</param>
        /// <param name="defaultCapacity">풀의 초기 용량.</param>
        /// <param name="maxSize">풀의 최대 크기.</param>
        public void RegisterPool(string key, GameObject prefab, int defaultCapacity = 10, int maxSize = 100)
        {
            if (_pools.ContainsKey(key))
            {
                Debug.LogWarning($"[PoolManager] Pool '{key}' already registered.");
                return;
            }

            _pools[key] = new ObjectPool<GameObject>(
                createFunc:      () => Object.Instantiate(prefab),
                actionOnGet:     go => go.SetActive(true),
                actionOnRelease: go => go.SetActive(false),
                actionOnDestroy: Object.Destroy,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize:         maxSize
            );
        }

        /// <summary>
        /// 키에 해당하는 풀에서 오브젝트를 꺼내 지정 컴포넌트를 반환한다.
        /// 등록되지 않은 키거나 컴포넌트가 없으면 null을 반환한다.
        /// </summary>
        /// <typeparam name="T">가져올 컴포넌트 타입.</typeparam>
        /// <param name="key">풀을 식별하는 고유 키.</param>
        /// <returns>풀에서 꺼낸 GameObject의 컴포넌트.</returns>
        public T Get<T>(string key) where T : Component
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                Debug.LogError($"[PoolManager] No pool registered for key '{key}'.");
                return null;
            }
            var go = pool.Get();
            return go.TryGetComponent<T>(out var component) ? component : null;
        }

        /// <summary>
        /// 키에 해당하는 풀에서 GameObject를 꺼내 반환한다.
        /// 등록되지 않은 키이면 null을 반환한다.
        /// </summary>
        /// <param name="key">풀을 식별하는 고유 키.</param>
        /// <returns>풀에서 꺼낸 GameObject.</returns>
        public GameObject Get(string key)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                Debug.LogError($"[PoolManager] No pool registered for key '{key}'.");
                return null;
            }
            return pool.Get();
        }

        /// <summary>
        /// 오브젝트를 풀로 반환한다.
        /// 풀이 없으면 오브젝트를 즉시 파괴한다.
        /// </summary>
        /// <param name="key">풀을 식별하는 고유 키.</param>
        /// <param name="go">반환할 GameObject.</param>
        public void Release(string key, GameObject go)
        {
            if (_pools.TryGetValue(key, out var pool))
                pool.Release(go);
            else
                Object.Destroy(go);
        }

        /// <summary>
        /// 지정한 키의 풀을 비우고 등록을 해제한다.
        /// </summary>
        /// <param name="key">해제할 풀의 고유 키.</param>
        public void ClearPool(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Clear();
                _pools.Remove(key);
            }
        }

        /// <summary>
        /// 등록된 모든 풀을 비우고 초기화한다.
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
                pool.Clear();
            _pools.Clear();
        }
    }
}
