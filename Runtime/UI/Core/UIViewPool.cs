using System;
using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.UI
{
    internal sealed class UIViewPool
    {
        private readonly Dictionary<string, Stack<UIView>> pools =
            new Dictionary<string, Stack<UIView>>(StringComparer.Ordinal);

        private readonly RectTransform poolRoot;

        public UIViewPool(RectTransform poolRoot)
        {
            this.poolRoot = poolRoot;
        }

        public void Prewarm(UIViewCatalogEntry entry, Func<UIViewCatalogEntry, Transform, UIView> factory)
        {
            if (!entry.Pooled || entry.PrewarmCount <= 0)
            {
                return;
            }

            var stack = GetOrCreateStack(entry.Id);
            for (var index = stack.Count; index < entry.PrewarmCount; index++)
            {
                var view = factory(entry, poolRoot);
                Return(entry, view);
            }
        }

        public UIView Get(UIViewCatalogEntry entry, Transform parent, Func<UIViewCatalogEntry, Transform, UIView> factory)
        {
            var stack = GetOrCreateStack(entry.Id);
            UIView view;
            if (stack.Count > 0)
            {
                view = stack.Pop();
                view.transform.SetParent(parent, false);
            }
            else
            {
                view = factory(entry, parent);
            }

            return view;
        }

        public void Return(UIViewCatalogEntry entry, UIView view)
        {
            if (view == null)
            {
                return;
            }

            view.gameObject.SetActive(false);
            view.transform.SetParent(poolRoot, false);
            GetOrCreateStack(entry.Id).Push(view);
        }

        private Stack<UIView> GetOrCreateStack(string id)
        {
            if (!pools.TryGetValue(id, out var stack))
            {
                stack = new Stack<UIView>();
                pools.Add(id, stack);
            }

            return stack;
        }
    }
}
