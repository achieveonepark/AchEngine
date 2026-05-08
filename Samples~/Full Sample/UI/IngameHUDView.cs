using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.Samples.Full.Messages;
using AchEngine.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples.Full.UI
{
    /// <summary>
    /// UIViewCatalog에 "IngameHUD" ID로 등록하세요. Layer: Screen.
    /// </summary>
    public class IngameHUDView : UIView
    {
        public class Payload
        {
            public int MaxHp;
            public int TimeRemaining;
        }

        [Header("Labels")]
        [SerializeField] private Text lblHp;
        [SerializeField] private Text lblScore;
        [SerializeField] private Text lblTime;

        [Header("Buttons")]
        [SerializeField] private Button btnPause;
        [SerializeField] private Button btnQuit;

#if ACHENGINE_R3
        private IDisposable _hpSub;
        private IDisposable _scoreSub;
#endif

        protected override void OnInitialize()
        {
            btnPause?.onClick.AddListener(OnPauseClicked);
            btnQuit?.onClick.AddListener(OnQuitClicked);
        }

        protected override void OnOpened(object payload)
        {
            if (payload is not Payload p) return;

            SetHp(p.MaxHp, p.MaxHp);
            SetScore(0);
            SetTime(p.TimeRemaining);

#if ACHENGINE_R3
            _hpSub    = UIBindingManager.Subscribe<HpChangedMessage>(msg  => SetHp(msg.Current, msg.Max));
            _scoreSub = UIBindingManager.Subscribe<ScoreChangedMessage>(msg => SetScore(msg.Score));
#endif
        }

        protected override void OnClosed()
        {
#if ACHENGINE_R3
            _hpSub?.Dispose();
            _scoreSub?.Dispose();
#endif
        }

        public void SetTime(int seconds)
        {
            if (lblTime != null)
                lblTime.text = $"{seconds / 60:00}:{seconds % 60:00}";
        }

        private void SetHp(int current, int max)
        {
            if (lblHp != null)
                lblHp.text = $"HP {current}/{max}";
        }

        private void SetScore(int score)
        {
            if (lblScore != null)
                lblScore.text = $"점수 {score}";
        }

        private void OnPauseClicked()
        {
            var input = ServiceLocator.Get<InputManager>();
            bool paused = Time.timeScale == 0f;
            Time.timeScale = paused ? 1f : 0f;
            input.IsEnabled.Equals(!paused); // 참고용
        }

        private async void OnQuitClicked()
        {
            await ServiceLocator.Get<AchSceneManager>().LoadSceneAsync("LobbyScene");
        }
    }
}
