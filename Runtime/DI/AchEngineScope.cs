#if ACHENGINE_VCONTAINER
using UnityEngine;
using VContainer;
using VContainer.Unity;
using AchEngine.Table;
using AchEngine.UI;

namespace AchEngine.DI
{
    /// <summary>
    /// AchEngine의 VContainer LifetimeScope.
    /// ITableService, IUIService와 사용자 정의 서비스를 자동으로 등록합니다.
    ///
    /// 사용법:
    ///   1. 씬에 이 컴포넌트를 추가합니다.
    ///   2. Inspector에서 UIViewCatalog를 할당합니다.
    ///   3. 추가 서비스가 필요하면 AchEngineInstaller 서브클래스를 만들어
    ///      Installers 배열에 추가합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class AchEngineScope : LifetimeScope
    {
        [Header("UI")]
        [SerializeField] private UIViewCatalog catalog;
        [SerializeField] private UIRoot uiRoot;
        [SerializeField] private bool autoCreateRoot = true;
        [SerializeField] private bool prewarmOnStart = true;
        [SerializeField] private bool makePersistent = true;

        [Header("Installers")]
        [Tooltip("AchEngineInstaller를 상속한 컴포넌트들을 여기에 추가합니다.")]
        [SerializeField] private AchEngineInstaller[] installers;

        protected override void Configure(IContainerBuilder builder)
        {
            // Table
            builder.Register<TableDatabase>(Lifetime.Singleton).As<ITableDatabase>();
            builder.Register<TableService>(Lifetime.Singleton).As<ITableService>();

            // UI
            if (uiRoot == null && autoCreateRoot)
                uiRoot = FindObjectOfType<UIRoot>();

            if (uiRoot == null && autoCreateRoot)
                uiRoot = UIRoot.CreateDefault();

            if (catalog != null)
                builder.RegisterInstance(catalog);

            if (uiRoot != null)
                builder.RegisterInstance(uiRoot);

            builder.RegisterComponentInNewPrefab<UIService>(CreateUIServicePrefab(), Lifetime.Singleton)
                .DontDestroyOnLoad();

            // 사용자 정의 Installer 실행
            if (installers != null)
            {
                var serviceBuilder = new VContainerServiceBuilder(builder);
                foreach (var installer in installers)
                {
                    if (installer != null)
                        installer.Install(serviceBuilder);
                }
            }
        }

        private UIService CreateUIServicePrefab()
        {
            var go = new GameObject("UIService (DI)", typeof(UIService));
            go.SetActive(false);
            return go.GetComponent<UIService>();
        }

        private void Start()
        {
            // UI 서비스 초기화
            var uiService = Container.Resolve<UIService>();
            if (catalog != null && uiRoot != null)
            {
                uiService.Initialize(catalog, uiRoot);
                UI.UI.SetService(uiService);

                if (prewarmOnStart)
                    uiService.Prewarm();
            }

            // Table 서비스 정적 접근자 연결
            var tableService = Container.Resolve<ITableService>();
            TableManager.SetService(tableService);

            // ServiceLocator 설정 (VContainer 없이도 서비스 접근 가능)
            ServiceLocator.Setup(type => Container.Resolve(type));

            if (makePersistent)
            {
                DontDestroyOnLoad(gameObject);
                if (uiRoot != null)
                    DontDestroyOnLoad(uiRoot.gameObject);
            }
        }

        protected override void OnDestroy()
        {
            ServiceLocator.Reset();
            base.OnDestroy();
        }
    }
}
#endif
