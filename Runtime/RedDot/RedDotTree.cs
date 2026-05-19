using System;
using System.Collections.Generic;

namespace AchEngine.UI
{
    /// <summary>
    /// 레드닷 노드를 키 기반으로 관리하는 트리 자료구조.
    /// 노드가 없을 경우 GetOrCreate로 자동 생성하며, 부모 계층도 함께 생성한다.
    /// </summary>
    internal sealed class RedDotTree
    {
        private readonly Dictionary<string, RedDotNode> _nodes = new();

        /// <summary>지정한 키의 고유 카운트를 설정한다. 음수는 0으로 처리한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <param name="count">설정할 카운트</param>
        internal void Set(string key, int count)
        {
            GetOrCreate(key).SetOwnCount(Math.Max(0, count));
        }

        /// <summary>지정한 키의 집계 카운트(자기 + 모든 자식)를 반환한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <returns>집계된 카운트. 노드가 없으면 0.</returns>
        internal int Get(string key)
        {
            return _nodes.TryGetValue(key, out var node) ? node.TotalCount : 0;
        }

        /// <summary>지정한 키의 카운트 변경 이벤트를 구독한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <param name="handler">카운트 변경 시 호출될 콜백</param>
        internal void Subscribe(string key, Action<int> handler)
        {
            GetOrCreate(key).Changed += handler;
        }

        /// <summary>지정한 키의 카운트 변경 이벤트 구독을 해제한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <param name="handler">해제할 콜백</param>
        internal void Unsubscribe(string key, Action<int> handler)
        {
            if (_nodes.TryGetValue(key, out var node))
                node.Changed -= handler;
        }

        /// <summary>모든 노드의 고유 카운트를 0으로 초기화한다.</summary>
        internal void ClearAll()
        {
            foreach (var key in _nodes.Keys)
                Set(key, 0);
        }

        /// <summary>키에 해당하는 노드를 반환하거나, 없으면 새로 생성해 부모 계층과 연결한다.</summary>
        /// <param name="key">레드닷 노드 키</param>
        /// <returns>생성되거나 기존에 존재하는 RedDotNode</returns>
        private RedDotNode GetOrCreate(string key)
        {
            if (_nodes.TryGetValue(key, out var existing))
                return existing;

            var node = new RedDotNode(key);
            _nodes[key] = node;

            int sep = key.LastIndexOf('/');
            if (sep > 0)
            {
                var parent = GetOrCreate(key[..sep]);
                node.Parent = parent;
                parent.AddChild(node);
            }

            return node;
        }
    }
}
