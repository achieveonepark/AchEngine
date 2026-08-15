using UnityEngine;

namespace AchEngine.UI
{
    /// <summary>
    /// VContainer ?놁씠 UIService瑜?珥덇린?뷀븯??遺?몄뒪?몃옒??
    /// VContainer瑜??ъ슜?섎뒗 寃쎌슦 AchEngineScope瑜?????ъ슜?섏꽭??
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIBootstrapper : MonoBehaviour
    {
        [SerializeField] private UIViewCatalog catalog;
        [SerializeField] private UIRoot root;
        [SerializeField] private bool autoCreateRoot = true;
        [SerializeField] private bool prewarmOnAwake = true;
        [SerializeField] private bool makePersistent = true;

        private UIService service;

        public UIService Service => service;

        private void Awake()
        {
            service = GetComponent<UIService>();
            if (service == null)
            {
                service = gameObject.AddComponent<UIService>();
            }

            if (UI.Current != null && UI.Current != service)
            {
                Debug.LogWarning($"[{nameof(UIBootstrapper)}] Another UIService is already active. Destroying duplicate bootstrapper.", this);
                Destroy(gameObject);
                return;
            }

            if (root == null && autoCreateRoot)
            {
                root = FindFirstObjectByType<UIRoot>();
            }

            if (root == null && autoCreateRoot)
            {
                root = UIRoot.CreateDefault();
            }

            if (catalog == null)
            {
                Debug.LogError($"[{nameof(UIBootstrapper)}] Missing UIViewCatalog reference.", this);
                enabled = false;
                return;
            }

            if (root == null)
            {
                Debug.LogError($"[{nameof(UIBootstrapper)}] Missing UIRoot reference and auto-create is disabled.", this);
                enabled = false;
                return;
            }

            service.Initialize(catalog, root);
            UI.SetService(service);

            if (makePersistent)
            {
                DontDestroyOnLoad(gameObject);
                // 자식 UIRoot는 이 부트스트래퍼와 함께 유지되므로 별도로 호출하면 경고가 발생한다.
                if (root != null && root.gameObject != gameObject && root.transform.parent == null)
                {
                    DontDestroyOnLoad(root.gameObject);
                }
            }

            if (prewarmOnAwake)
            {
                service.Prewarm();
            }
        }

        private void OnDestroy()
        {
            if (UI.Current == service)
            {
                UI.SetService(null);
            }
        }
    }
}
