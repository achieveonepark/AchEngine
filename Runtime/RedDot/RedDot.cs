using System;
using UnityEngine;

namespace AchEngine.UI
{
    /// <summary>
    /// 레드닷(알림 배지) 카운트를 전역으로 관리하는 정적 진입점.
    /// '/'로 구분된 계층 키를 사용하며, 부모 노드는 자식 카운트를 자동으로 합산한다.
    /// 예: "Shop/New", "Quest/Daily"
    /// </summary>
    public static class RedDot
    {
        private static RedDotTree _tree;
        private static RedDotTree Tree => _tree ??= new RedDotTree();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain() => _tree = null;

        /// <summary>
        /// 지정한 키의 카운트를 설정한다. 부모 노드는 자동으로 합산된다.
        /// '/'를 구분자로 계층을 표현한다 (예: "Shop/New", "Quest/Daily").
        /// </summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <param name="count">설정할 카운트 (음수는 0으로 처리)</param>
        public static void Set(string key, int count) => Tree.Set(key, count);

        /// <summary>지정한 키의 카운트에 delta를 더한다. 결과가 0 미만이면 0으로 유지한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <param name="delta">더할 값 (음수 가능)</param>
        public static void Add(string key, int delta) => Tree.Set(key, Math.Max(0, Tree.Get(key) + delta));

        /// <summary>지정한 키의 집계 카운트(자기 + 모든 자식)를 반환한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <returns>집계된 카운트</returns>
        public static int Get(string key) => Tree.Get(key);

        /// <summary>집계 카운트가 0보다 크면 true를 반환한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        public static bool HasDot(string key) => Tree.Get(key) > 0;

        /// <summary>지정한 키의 카운트를 0으로 초기화한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        public static void Clear(string key) => Tree.Set(key, 0);

        /// <summary>카운트 변경 이벤트를 구독한다. 부모 노드로 전파된 변경도 포함된다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <param name="handler">카운트가 변경될 때 호출될 콜백</param>
        public static void Subscribe(string key, Action<int> handler) => Tree.Subscribe(key, handler);

        /// <summary>카운트 변경 이벤트 구독을 해제한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <param name="handler">해제할 콜백</param>
        public static void Unsubscribe(string key, Action<int> handler) => Tree.Unsubscribe(key, handler);

        /// <summary>모든 노드의 카운트를 0으로 초기화한다.</summary>
        public static void ClearAll() => Tree.ClearAll();
    }
}
