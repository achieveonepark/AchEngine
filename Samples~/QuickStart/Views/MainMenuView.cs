using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples
{
    public class MainMenuView : UIView
    {
        [Header("Buttons")]
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnQuit;

        [Header("Text")]
        [SerializeField] private Text titleText;

        protected override void OnInitialize()
        {
            btnStart?.onClick.AddListener(() => UI.Show("GameHUD"));
            btnSettings?.onClick.AddListener(() => UI.Show("Settings"));
            btnQuit?.onClick.AddListener(Application.Quit);
        }

        protected override void OnOpened(object payload)
        {
            if (titleText != null)
            {
                titleText.text = "AchEngine Sample";
            }

            Debug.Log("[Sample] Main menu opened.");
        }
    }
}
