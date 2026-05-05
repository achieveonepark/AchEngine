using AchEngine.DI;
using AchEngine.UI;
using UnityEngine;

namespace AchEngine.Samples
{
    /// <summary>
    /// VContainer ?놁씠 ?ъ슜?섎뒗 遺?몄뒪?몃옪 ?덉떆.
    ///
    /// ?ㅼ젙 諛⑸쾿:
    ///   1. ?ъ뿉 UIBootstrapper? UIRoot 異붽?, UIViewCatalog ?좊떦
    ///   2. ?ъ뿉 鍮?GameObject瑜?留뚮뱾怨???而댄룷?뚰듃 異붽?
    /// </summary>
    public class GameBootstrapNoDI : MonoBehaviour
    {
        private void Start()
        {
            // UI媛 以鍮꾨릺吏 ?딆븯?쇰㈃ ?湲?            if (!UI.IsReady)
            {
                Debug.LogWarning("[Sample] UI媛 ?꾩쭅 以鍮꾨릺吏 ?딆븯?듬땲?? UIBootstrapper瑜??ъ뿉 異붽??섏꽭??");
                return;
            }

            // ?뚯씠釉??곗씠??濡쒕뱶 (?먮룞 ?앹꽦 肄붾뱶 ?ъ슜)
            // TableLoaderGenerated.LoadAll();

            // ?뚯뒪?몄슜 吏곸젒 JSON 濡쒕뱶
            // TableManager.LoadFromJsonText<MonsterData>("[{\"Id\":1,\"Name\":\"Slime\",\"Hp\":100,\"Speed\":1.5}]");

            // 濡쒕뵫 ??硫붿씤 硫붾돱
            UI.Show("Loading", new LoadingView.Payload
            {
                message  = "寃뚯엫 ?곗씠?곕? 遺덈윭?ㅻ뒗 以?..",
                duration = 1.5f
            });

            UI.Show("MainMenu");

            Debug.Log("[Sample] VContainer ?놁씠 寃뚯엫 ?쒖옉");
        }
    }
}
