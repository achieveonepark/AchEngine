using System;
using System.Collections.Generic;

namespace AchEngine.UI
{
    internal sealed class RedDotTree
    {
        private readonly Dictionary<string, RedDotNode> _nodes = new();

        internal void Set(string key, int count)
        {
            GetOrCreate(key).SetOwnCount(Math.Max(0, count));
        }

        internal int Get(string key)
        {
            return _nodes.TryGetValue(key, out var node) ? node.TotalCount : 0;
        }

        internal void Subscribe(string key, Action<int> handler)
        {
            GetOrCreate(key).Changed += handler;
        }

        internal void Unsubscribe(string key, Action<int> handler)
        {
            if (_nodes.TryGetValue(key, out var node))
                node.Changed -= handler;
        }

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
