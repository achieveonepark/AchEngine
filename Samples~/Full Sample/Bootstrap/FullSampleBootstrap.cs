using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.Player;
using AchEngine.Samples.Full.Data;
using UnityEngine;

namespace AchEngine.Samples.Full
{
    /// <summary>
    /// Full Sample 진입점 — 씬의 빈 GameObject에 붙이세요.
    ///
    /// 설정 방법:
    ///   1. 씬에 AchEngineScope GameObject를 만들고 AchManagerInstaller 컴포넌트 추가
    ///   2. 씬에 UIBootstrapper와 UIRoot를 추가하고 UIViewCatalog 할당
    ///   3. 씬에 빈 GameObject를 만들고 이 FullSampleBootstrap 컴포넌트 추가
    ///   4. Play → TitleScene으로 자동 전환
    /// </summary>
    public class FullSampleBootstrap : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string titleSceneName = "TitleScene";

        private async void Start()
        {
            if (!ServiceLocator.IsReady)
            {
                Debug.LogError("[FullSample] ServiceLocator가 준비되지 않았습니다. AchEngineScope를 씬에 추가하세요.");
                return;
            }

            var pm = ServiceLocator.Get<PlayerManager>();
            SetupPlayerData(pm);

            // USE_QUICK_SAVE 심볼이 정의된 경우 저장된 데이터 로드
            // pm.Configure(encryptionKey: "FullSample1234!", version: 1);
            // pm.Load();

            var config = AchScriptableObject.GetOrAdd<GameConfig>();
            var sound  = ServiceLocator.Get<SoundManager>();
            sound.BgmVolume = config.DefaultBgmVolume;
            sound.SfxVolume = config.DefaultSfxVolume;

            var cm = ServiceLocator.Get<ConfigManager>();
            cm.AddKey(GameConfig.KeyBgmVolume, config.DefaultBgmVolume);
            cm.AddKey(GameConfig.KeySfxVolume, config.DefaultSfxVolume);
            cm.AddKey(GameConfig.KeyPlayerName, config.DefaultPlayerName);

            var sceneManager = ServiceLocator.Get<AchSceneManager>();
            await sceneManager.LoadSceneAsync(titleSceneName);
        }

        private static void SetupPlayerData(PlayerManager pm)
        {
            pm.AddContainer(new InventoryContainer());

            var inv = pm.GetContainer<InventoryContainer>();
            if (inv.GetInfo(1) == null)
            {
                inv.Add(1, new InventoryItem { Id = 1, Name = "Starter Sword",  Quantity = 1 });
                inv.Add(2, new InventoryItem { Id = 2, Name = "Health Potion",  Quantity = 5 });
                inv.Add(3, new InventoryItem { Id = 3, Name = "Mana Crystal",   Quantity = 3 });
            }
        }
    }
}
