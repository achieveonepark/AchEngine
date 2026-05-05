using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples
{
    /// <summary>
    /// 寃뚯엫 HUD ?ㅻ쾭?덉씠 (?덉씠?? Overlay).
    /// UI.Show("GameHUD") 濡??대━怨? int payload濡?珥덇린 HP瑜?諛쏆뒿?덈떎.
    /// </summary>
    public class GameHUDView : UIView
    {
        [SerializeField] private Text  hpText;
        [SerializeField] private Text  scoreText;
        [SerializeField] private Button btnMenu;
        [SerializeField] private Button btnPause;

        private int score;

        protected override void OnInitialize()
        {
            btnMenu? .onClick.AddListener(() =>
            {
                UI.Close("GameHUD");
                UI.Show("MainMenu");
            });

            btnPause?.onClick.AddListener(() => UI.Show("PausePopup"));
        }

        protected override void OnOpened(object payload)
        {
            score = 0;
            UpdateScore(0);

            var hp = payload is int v ? v : 100;
            UpdateHp(hp);
        }

        public void UpdateHp(int hp)
        {
            if (hpText != null) hpText.text = $"HP: {hp}";
        }

        public void UpdateScore(int delta)
        {
            score += delta;
            if (scoreText != null) scoreText.text = $"?먯닔: {score}";
        }
    }
}
