using AchEngine.Table;
using AchEngine.DI;
using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples
{
    /// <summary>
    /// 紐ъ뒪???곸꽭 ?앹뾽 (?덉씠?? Popup).
    /// payload濡?紐ъ뒪??ID(int)瑜?諛쏆븘 TableManager?먯꽌 ?곗씠?곕? 議고쉶?⑸땲??
    ///
    /// ?ъ슜 ??
    ///   UI.Show("MonsterDetail", 1);   // ID 1踰?紐ъ뒪???쒖떆
    /// </summary>
    public class MonsterDetailPopup : UIView
    {
        [SerializeField] private Text  nameText;
        [SerializeField] private Text  hpText;
        [SerializeField] private Text  speedText;
        [SerializeField] private Button btnClose;

        protected override void OnInitialize()
        {
            btnClose?.onClick.AddListener(CloseSelf);
        }

        protected override void OnOpened(object payload)
        {
            if (payload is not int monsterId)
            {
                Debug.LogWarning("[MonsterDetailPopup] payload媛 int(紐ъ뒪??ID)媛 ?꾨떃?덈떎.");
                return;
            }

            if (!TableManager.TryGet<MonsterData>(monsterId, out var data))
            {
                Debug.LogWarning($"[MonsterDetailPopup] ID {monsterId} 紐ъ뒪?곕? 李얠쓣 ???놁뒿?덈떎.");
                return;
            }

            if (nameText  != null) nameText.text  = data.Name;
            if (hpText    != null) hpText.text    = $"HP: {data.Hp}";
            if (speedText != null) speedText.text = $"?띾룄: {data.Speed:F1}";
        }
    }
}
