using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.Player;
using AchEngine.Samples.Full.Data;
using AchEngine.Samples.Full.Messages;
using AchEngine.Samples.Full.UI;
using AchEngine.UI;
using UnityEngine;

namespace AchEngine.Samples.Full.Scenes
{
    /// <summary>
    /// 로비 씬 루트 GameObject에 붙이세요.
    /// </summary>
    public class LobbyScene : MonoBehaviour, IScene
    {
        [SerializeField] private AudioClip lobbyBgm;

        private int _gold;

        public AchTask OnSceneStart()
        {
            if (lobbyBgm != null)
                ServiceLocator.Get<SoundManager>().PlayBgm(lobbyBgm);

            var config = AchScriptableObject.GetOrAdd<GameConfig>();
            var cm     = ServiceLocator.Get<ConfigManager>();
            var pm     = ServiceLocator.Get<PlayerManager>();

            string playerName = cm.GetConfig<string>(GameConfig.KeyPlayerName, config.DefaultPlayerName);
            _gold = config.StartingGold;

            var payload = new LobbyView.Payload
            {
                PlayerName = playerName,
                Gold       = _gold,
                ItemCount  = CountItems(pm),
            };

            UI.Show<LobbyView>("Lobby", payload);
            return AchTask.CompletedTask;
        }

        public AchTask OnSceneEnd()
        {
            ServiceLocator.Get<SoundManager>().StopBgm();
            UI.CloseAll();
            return AchTask.CompletedTask;
        }

        public void AddGold(int amount)
        {
            _gold += amount;
#if ACHENGINE_R3
            UIBindingManager.Publish(new GoldChangedMessage { Current = _gold, Delta = amount });
#endif
        }

        private static int CountItems(PlayerManager pm)
        {
            var inv = pm.GetContainer<InventoryContainer>();
            if (inv == null) return 0;
            int count = 0;
            foreach (var _ in inv.GetAll()) count++;
            return count;
        }
    }
}
