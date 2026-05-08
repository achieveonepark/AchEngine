using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples.Full.UI
{
    /// <summary>
    /// UIViewCatalog에 "Title" ID로 등록하세요. Layer: Screen.
    /// </summary>
    public class TitleView : UIView
    {
        [Header("Buttons")]
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnQuit;

        [Header("Labels")]
        [SerializeField] private Text lblServerStatus;
        [SerializeField] private Text lblVersion;

        private bool _serverOnline;

        protected override void OnInitialize()
        {
            btnStart?.onClick.AddListener(OnStartClicked);
            btnQuit?.onClick.AddListener(Application.Quit);

            if (lblVersion != null)
                lblVersion.text = $"v{Application.version}";
        }

        protected override void OnOpened(object payload) { }

        public void SetServerStatus(bool online)
        {
            _serverOnline = online;
            if (lblServerStatus != null)
                lblServerStatus.text = online ? "● 서버 온라인" : "○ 오프라인 모드";
        }

        private async void OnStartClicked()
        {
            btnStart.interactable = false;
            await ServiceLocator.Get<AchSceneManager>().LoadSceneAsync("LobbyScene");
        }
    }
}
