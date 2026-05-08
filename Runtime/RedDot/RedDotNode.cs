using System;
using System.Collections.Generic;

namespace AchEngine.UI
{
    internal sealed class RedDotNode
    {
        private int _ownCount;
        private int _totalCount;
        private readonly List<RedDotNode> _children = new();

        public string Key { get; }
        public RedDotNode Parent { get; internal set; }
        public int TotalCount => _totalCount;

        internal event Action<int> Changed;

        internal RedDotNode(string key) => Key = key;

        internal void SetOwnCount(int count)
        {
            if (_ownCount == count) return;
            _ownCount = count;
            Recalculate();
        }

        internal void AddChild(RedDotNode child)
        {
            _children.Add(child);
        }

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
