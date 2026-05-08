using System;
using UnityEngine;

namespace AchEngine.UI
{
    public static class RedDot
    {
        private static RedDotTree _tree;
        private static RedDotTree Tree => _tree ??= new RedDotTree();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain() => _tree = null;

        /// <summary>
        /// Sets the count for a node. Parent nodes aggregate automatically.
        /// Use '/' as a separator for hierarchy: e.g. "Shop/New", "Quest/Daily".
        /// </summary>
        public static void Set(string key, int count) => Tree.Set(key, count);

        /// <summary>Adds delta to the current count of the node.</summary>
        public static void Add(string key, int delta) => Tree.Set(key, Math.Max(0, Tree.Get(key) + delta));

        /// <summary>Returns the aggregated count (own + all children) for a node.</summary>
        public static int Get(string key) => Tree.Get(key);

        /// <summary>Returns true if the aggregated count is greater than zero.</summary>
        public static bool HasDot(string key) => Tree.Get(key) > 0;

        /// <summary>Sets the count to zero for the node.</summary>
        public static void Clear(string key) => Tree.Set(key, 0);

        /// <summary>Subscribes to count changes for a node (including propagated parent changes).</summary>
        public static void Subscribe(string key, Action<int> handler) => Tree.Subscribe(key, handler);

        /// <summary>Unsubscribes from count changes for a node.</summary>
        public static void Unsubscribe(string key, Action<int> handler) => Tree.Unsubscribe(key, handler);
    }
}
