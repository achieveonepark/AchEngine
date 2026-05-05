using System;
using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.UI
{
    [CreateAssetMenu(fileName = "UIViewCatalog", menuName = "AchEngine/View Catalog")]
    public sealed class UIViewCatalog : ScriptableObject
    {
        [SerializeField] private List<UIViewCatalogEntry> entries = new List<UIViewCatalogEntry>();

        public IReadOnlyList<UIViewCatalogEntry> Entries => entries;

        public bool TryFind(string id, out UIViewCatalogEntry entry)
        {
            for (var index = 0; index < entries.Count; index++)
            {
                var candidate = entries[index];
                if (candidate != null && candidate.Id == id)
                {
                    entry = candidate;
                    return true;
                }
            }

            entry = null;
            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var usedIds = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.Id))
                {
                    Debug.LogWarning($"[{nameof(UIViewCatalog)}] Empty view id at index {index}.", this);
                    continue;
                }

                if (entry.Prefab == null)
                {
                    Debug.LogWarning($"[{nameof(UIViewCatalog)}] View '{entry.Id}' is missing its prefab.", this);
                }

                if (!usedIds.Add(entry.Id))
                {
                    Debug.LogWarning($"[{nameof(UIViewCatalog)}] Duplicate view id '{entry.Id}'.", this);
                }
            }
        }
#endif
    }

    [Serializable]
    public sealed class UIViewCatalogEntry
    {
        [SerializeField] private string id = "NewView";
        [SerializeField] private UIView prefab;
        [SerializeField] private UILayerId layer = UILayerId.Screen;
        [SerializeField] private bool pooled = true;
        [Min(0)]
        [SerializeField] private int prewarmCount;
        [SerializeField] private bool singleInstance = true;

        public string Id => id;
        public UIView Prefab => prefab;
        public UILayerId Layer => layer;
        public bool Pooled => pooled;
        public int PrewarmCount => Mathf.Max(0, prewarmCount);
        public bool SingleInstance => singleInstance;
    }
}
