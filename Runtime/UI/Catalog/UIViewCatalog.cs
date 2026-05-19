using System;
using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.UI
{
    /// <summary>
    /// UIView 프리팹과 배치 설정을 등록하는 ScriptableObject 카탈로그입니다.
    /// UIService는 이 카탈로그를 통해 뷰를 생성하고 레이어를 결정합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "UIViewCatalog", menuName = "AchEngine/View Catalog")]
    public sealed class UIViewCatalog : ScriptableObject
    {
        [SerializeField] private List<UIViewCatalogEntry> entries = new List<UIViewCatalogEntry>();

        /// <summary>카탈로그에 등록된 모든 뷰 항목 목록입니다.</summary>
        public IReadOnlyList<UIViewCatalogEntry> Entries => entries;

        /// <summary>ID로 항목을 검색합니다. 없으면 false를 반환합니다.</summary>
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

    /// <summary>카탈로그에 등록된 개별 UIView 항목의 설정을 담는 직렬화 클래스입니다.</summary>
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

        /// <summary>뷰를 식별하는 고유 ID.</summary>
        public string Id => id;
        /// <summary>인스턴스화할 UIView 프리팹.</summary>
        public UIView Prefab => prefab;
        /// <summary>뷰가 배치될 UI 레이어.</summary>
        public UILayerId Layer => layer;
        /// <summary>true면 닫힐 때 파괴하지 않고 풀에 반환합니다.</summary>
        public bool Pooled => pooled;
        /// <summary>사전 생성할 인스턴스 수. 0이면 사전 생성하지 않습니다.</summary>
        public int PrewarmCount => Mathf.Max(0, prewarmCount);
        /// <summary>true면 이미 열려 있는 인스턴스를 재사용하고 중복 생성을 막습니다.</summary>
        public bool SingleInstance => singleInstance;
    }
}
