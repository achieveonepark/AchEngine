using System;
using System.Collections.Generic;

namespace AchEngine.UI
{
    /// <summary>
    /// 레드닷 트리의 단일 노드. 자신의 카운트와 자식 카운트의 합산을 관리한다.
    /// </summary>
    internal sealed class RedDotNode
    {
        private int _ownCount;
        private int _totalCount;
        private readonly List<RedDotNode> _children = new();

        /// <summary>이 노드의 전체 경로 키.</summary>
        public string Key { get; }

        /// <summary>부모 노드. 루트 노드는 null이다.</summary>
        public RedDotNode Parent { get; internal set; }

        /// <summary>자신의 카운트와 모든 자식 카운트의 합산.</summary>
        public int TotalCount => _totalCount;

        /// <summary>TotalCount가 변경될 때 발생하는 이벤트.</summary>
        internal event Action<int> Changed;

        /// <param name="key">이 노드에 할당할 키</param>
        internal RedDotNode(string key) => Key = key;

        /// <summary>이 노드 고유의 카운트를 설정하고 변경이 있으면 합산을 재계산한다.</summary>
        /// <param name="count">새 카운트 값</param>
        internal void SetOwnCount(int count)
        {
            if (_ownCount == count) return;
            _ownCount = count;
            Recalculate();
        }

        /// <summary>자식 노드를 추가한다.</summary>
        /// <param name="child">추가할 자식 노드</param>
        internal void AddChild(RedDotNode child)
        {
            _children.Add(child);
        }

        /// <summary>자신과 자식들의 카운트를 합산해 갱신하고, 변경이 있으면 부모로 전파한다.</summary>
        internal void Recalculate()
        {
            int total = _ownCount;
            foreach (var child in _children)
                total += child._totalCount;

            if (_totalCount == total) return;
            _totalCount = total;
            Changed?.Invoke(_totalCount);
            Parent?.Recalculate();
        }
    }
}
