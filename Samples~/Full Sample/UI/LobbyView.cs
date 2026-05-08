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
    /// UIViewCatalog에 "Lobby" ID로 등록하세요. Layer: Screen.
    /// </summary>
    public class LobbyView : UIView
    {
        public class Payload
        {
            public string PlayerName;
            public int    Gold;
            public int    ItemCount;
        }

        [Header("Player Info")]
        [SerializeField] private Text lblPlayerName;
        [SerializeField] private Text lblGold;
        [SerializeField] private Text lblItemCount;

        [Header("Buttons")]
        [SerializeField] private Button btnPlay;
        [SerializeField] private Button btnInventory;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnShop;

#if ACHENGINE_R3
        private IDisposable _goldSub;
#endif

        protected override void OnInitialize()
        {
            btnPlay?.onClick.AddListener(OnPlayClicked);
            btnInventory?.onClick.AddListener(() => UI.Show("Inventory"));
            btnSettings?.onClick.AddListener(() => UI.Show("Settings"));
            btnShop?.onClick.AddListener(() => UI.Show("Purchase"));
        }

        protected override void OnOpened(object payload)
        {
            if (payload is not Payload p) return;

            if (lblPlayerName != null) lblPlayerName.text = p.PlayerName;
            if (lblItemCount  != null) lblItemCount.text  = $"아이템 {p.ItemCount}개";
            SetGold(p.Gold);

#if ACHENGINE_R3
            _goldSub = UIBindingManager.Subscribe<GoldChangedMessage>(msg => SetGold(msg.Current));
#endif
        }

        protected override void OnClosed()
        {
#if ACHENGINE_R3
            _goldSub?.Dispose();
#endif
        }

        private void SetGold(int amount)
        {
            if (lblGold != null)
                lblGold.text = $"골드: {amount:N0}";
        }

        private async void OnPlayClicked()
        {
            btnPlay.interactable = false;
            await ServiceLocator.Get<AchSceneManager>().LoadSceneAsync("IngameScene");
        }
    }
}
