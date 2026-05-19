using System;
using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.UI
{
    /// <summary>
    /// UIView 생명주기(열기·닫기·풀링)를 관리하는 핵심 서비스입니다.
    /// VContainer 환경에서는 AchEngineScope가, 아닌 경우 UIBootstrapper가 자동으로 초기화합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIService : MonoBehaviour, IUIService
    {
        private readonly Dictionary<string, UIViewCatalogEntry> catalogIndex =
            new Dictionary<string, UIViewCatalogEntry>(StringComparer.Ordinal);

        private readonly Dictionary<string, List<UIView>> activeViewsById =
            new Dictionary<string, List<UIView>>(StringComparer.Ordinal);

        private readonly List<UIView> activationStack = new List<UIView>();

        private UIViewCatalog catalog;
        private UIRoot root;
        private UIViewPool pool;

        /// <summary>현재 사용 중인 UIViewCatalog입니다.</summary>
        public UIViewCatalog Catalog => catalog;
        /// <summary>레이어 루트를 관리하는 UIRoot 컴포넌트입니다.</summary>
        public UIRoot Root => root;
        /// <summary>서비스가 Initialize된 상태인지 나타냅니다.</summary>
        public bool IsInitialized => catalog != null && root != null && pool != null;

        /// <summary>카탈로그와 루트를 지정해 UIService를 초기화합니다.</summary>
        public void Initialize(UIViewCatalog viewCatalog, UIRoot uiRoot)
        {
            if (viewCatalog == null)
            {
                throw new ArgumentNullException(nameof(viewCatalog));
            }

            if (uiRoot == null)
            {
                throw new ArgumentNullException(nameof(uiRoot));
            }

            catalog = viewCatalog;
            root = uiRoot;
            root.EnsureRuntimeStructure();
            pool = new UIViewPool(root.PoolRoot);

            BuildCatalogIndex();
        }

        /// <summary>카탈로그에 등록된 Pooled 뷰를 미리 생성해 풀에 채웁니다.</summary>
        public void Prewarm()
        {
            EnsureInitialized();
            foreach (var entry in catalogIndex.Values)
            {
                pool.Prewarm(entry, CreateViewInstance);
            }
        }

        /// <summary>지정한 ID의 뷰를 열고 인스턴스를 반환합니다.</summary>
        /// <param name="id">카탈로그에 등록된 뷰 ID.</param>
        /// <param name="payload">OnBeforeOpen에 전달할 데이터.</param>
        public UIView Show(string id, object payload = null)
        {
            EnsureInitialized();
            if (!catalogIndex.TryGetValue(id, out var entry))
            {
                Debug.LogError($"[{nameof(UIService)}] Unknown view id '{id}'.", this);
                return null;
            }

            if (entry.SingleInstance && TryGetOpen(id, out var existing))
            {
                AttachToLayer(existing, entry.Layer);
                TrackAsTopmost(existing);
                existing.Open(payload);
                return existing;
            }

            var parent = root.GetLayerRoot(entry.Layer);
            var view = entry.Pooled ? pool.Get(entry, parent, CreateViewInstance) : CreateViewInstance(entry, parent);

            AttachToLayer(view, entry.Layer);
            RegisterActiveView(entry.Id, view);
            view.Open(payload);
            return view;
        }

        /// <summary>지정한 ID의 뷰를 <typeparamref name="T"/>로 캐스팅해 반환합니다.</summary>
        public T Show<T>(string id, object payload = null)
            where T : UIView
        {
            return Show(id, payload) as T;
        }

        /// <summary>ID로 열린 뷰를 닫습니다. closeAll이 true면 같은 ID의 모든 인스턴스를 닫습니다.</summary>
        public bool Close(string id, bool closeAll = false)
        {
            if (!activeViewsById.TryGetValue(id, out var views) || views.Count == 0)
            {
                return false;
            }

            if (closeAll)
            {
                var snapshot = views.ToArray();
                for (var index = 0; index < snapshot.Length; index++)
                {
                    Close(snapshot[index]);
                }

                return snapshot.Length > 0;
            }

            return Close(views[views.Count - 1]);
        }

        /// <summary>뷰 인스턴스를 직접 닫습니다.</summary>
        public bool Close(UIView view)
        {
            if (view == null || view.IsClosing || !ContainsActiveView(view))
            {
                return false;
            }

            view.Close(() => HandleViewClosed(view));
            return true;
        }

        /// <summary>활성화 스택에서 가장 마지막에 열린 뷰를 닫습니다.</summary>
        public bool CloseTopmost()
        {
            if (activationStack.Count == 0)
            {
                return false;
            }

            return Close(activationStack[activationStack.Count - 1]);
        }

        /// <summary>현재 열려있는 모든 뷰를 닫습니다.</summary>
        public void CloseAll()
        {
            var snapshot = activationStack.ToArray();
            for (var index = snapshot.Length - 1; index >= 0; index--)
            {
                Close(snapshot[index]);
            }
        }

        /// <summary>지정한 ID의 열린 뷰를 가져옵니다. 없으면 false를 반환합니다.</summary>
        public bool TryGetOpen(string id, out UIView view)
        {
            if (activeViewsById.TryGetValue(id, out var views) && views.Count > 0)
            {
                view = views[views.Count - 1];
                return true;
            }

            view = null;
            return false;
        }

        /// <summary>활성화 스택에서 <typeparamref name="T"/> 타입의 뷰를 찾아 반환합니다.</summary>
        public bool TryGetOpen<T>(out T view)
            where T : UIView
        {
            for (var index = activationStack.Count - 1; index >= 0; index--)
            {
                if (activationStack[index] is T typedView)
                {
                    view = typedView;
                    return true;
                }
            }

            view = null;
            return false;
        }

        /// <summary>지정한 ID의 뷰가 현재 열려있는지 확인합니다.</summary>
        public bool IsOpen(string id)
        {
            return activeViewsById.TryGetValue(id, out var views) && views.Count > 0;
        }

        private void HandleViewClosed(UIView view)
        {
            if (view == null)
            {
                return;
            }

            UnregisterActiveView(view);
            if (!catalogIndex.TryGetValue(view.ViewId, out var entry))
            {
                Destroy(view.gameObject);
                return;
            }

            if (entry.Pooled)
            {
                pool.Return(entry, view);
            }
            else
            {
                Destroy(view.gameObject);
            }
        }

        private void BuildCatalogIndex()
        {
            catalogIndex.Clear();
            activeViewsById.Clear();
            activationStack.Clear();

            var entries = catalog.Entries;
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Id) || entry.Prefab == null)
                {
                    continue;
                }

                if (catalogIndex.ContainsKey(entry.Id))
                {
                    Debug.LogWarning($"[{nameof(UIService)}] Duplicate view id '{entry.Id}' skipped.", catalog);
                    continue;
                }

                catalogIndex.Add(entry.Id, entry);
            }
        }

        private UIView CreateViewInstance(UIViewCatalogEntry entry, Transform parent)
        {
            var instance = Instantiate(entry.Prefab, parent, false);
            instance.name = entry.Prefab.name;
            instance.Initialize(this, entry);
            return instance;
        }

        private void AttachToLayer(UIView view, UILayerId layer)
        {
            var layerRoot = root.GetLayerRoot(layer);
            if (view.transform.parent != layerRoot)
            {
                view.transform.SetParent(layerRoot, false);
            }

            view.transform.SetAsLastSibling();
        }

        private void RegisterActiveView(string id, UIView view)
        {
            if (!activeViewsById.TryGetValue(id, out var views))
            {
                views = new List<UIView>();
                activeViewsById.Add(id, views);
            }

            if (!views.Contains(view))
            {
                views.Add(view);
            }

            TrackAsTopmost(view);
        }

        private void UnregisterActiveView(UIView view)
        {
            activationStack.Remove(view);
            if (!activeViewsById.TryGetValue(view.ViewId, out var views))
            {
                return;
            }

            views.Remove(view);
            if (views.Count == 0)
            {
                activeViewsById.Remove(view.ViewId);
            }
        }

        private void TrackAsTopmost(UIView view)
        {
            activationStack.Remove(view);
            activationStack.Add(view);
        }

        private bool ContainsActiveView(UIView view)
        {
            return activeViewsById.TryGetValue(view.ViewId, out var views) && views.Contains(view);
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "UIService is not initialized. Add a AchEngineScope (VContainer) or UIBootstrapper to your scene first.");
            }
        }
    }
}
