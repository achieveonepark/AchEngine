#if ACHENGINE_VCONTAINER
using AchEngine.DI;
using AchEngine.Table;
using AchEngine.UI;
using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace AchEngine.Samples
{
    /// <summary>
    /// VContainer DI 湲곕컲 遺?몄뒪?몃옪 ?덉떆.
    ///
    /// ?ㅼ젙 諛⑸쾿:
    ///   1. ?ъ뿉 AchEngineScope (LifetimeScope) 異붽??섍퀬 UIViewCatalog ?좊떦
    ///   2. ?ъ뿉 鍮?GameObject瑜?留뚮뱾怨???而댄룷?뚰듃 異붽?
    ///   3. SampleGameScope瑜?LifetimeScope濡?吏??    ///
    /// AchEngineScope?먯꽌 ITableService, IUIService媛 ?먮룞 ?깅줉?⑸땲??
    /// </summary>
    public class SampleGameScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 寃뚯엫 吏꾩엯???깅줉
            builder.RegisterEntryPoint<GameBootstrap>();
        }
    }

    /// <summary>
    /// 寃뚯엫 ?쒖옉 吏꾩엯??
    /// IStartable? VContainer媛 ???쒖옉 ???먮룞?쇰줈 Start()瑜??몄텧?⑸땲??
    /// </summary>
    public class GameBootstrap : IStartable
    {
        private readonly ITableService _tableService;
        private readonly IUIService    _uiService;

        [Inject]
        public GameBootstrap(ITableService tableService, IUIService uiService)
        {
            _tableService = tableService;
            _uiService    = uiService;
        }

        public void Start()
        {
            // 1. ?뚯씠釉??곗씠??濡쒕뱶
            //    (TableLoaderGenerated.cs??Tools > AchEngine > Table Loader濡??앹꽦)
            // TableLoaderGenerated.LoadAll(_tableService);

            // 2. 濡쒕뵫 ?붾㈃ ?쒖떆 ??硫붿씤 硫붾돱濡??꾪솚
            _uiService.Show("Loading", new LoadingView.Payload
            {
                message  = "寃뚯엫 ?곗씠?곕? 遺덈윭?ㅻ뒗 以?..",
                duration = 1.5f
            });

            // 3. 濡쒕뵫 ??硫붿씤 硫붾돱
            _uiService.Show("MainMenu");

            Debug.Log("[Sample] VContainer DI濡?寃뚯엫 ?쒖옉");
        }
    }
}
#endif
