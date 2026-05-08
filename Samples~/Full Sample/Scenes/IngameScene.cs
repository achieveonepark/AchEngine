using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.Samples.Full.Data;
using AchEngine.Samples.Full.Game;
using AchEngine.Samples.Full.Messages;
using AchEngine.Samples.Full.UI;
using AchEngine.UI;
using System.Threading.Tasks;
using UnityEngine;

namespace AchEngine.Samples.Full.Scenes
{
    /// <summary>
    /// 인게임 씬 루트 GameObject에 붙이세요.
    /// </summary>
    public class IngameScene : MonoBehaviour, IScene
    {
        [Header("Prefabs for Pool")]
        [SerializeField] private GameObject cardPrefab;

        [SerializeField] private AudioClip ingameBgm;

        public async Task OnSceneStart()
        {
            var input = ServiceLocator.Get<InputManager>();
            input.Disable();

            if (ingameBgm != null)
                ServiceLocator.Get<SoundManager>().PlayBgm(ingameBgm);

            // 카드 오브젝트 풀 등록
            if (cardPrefab != null)
            {
                var pool = ServiceLocator.Get<PoolManager>();
                pool.RegisterPool(GameplayManager.CardPoolKey, cardPrefab, defaultCapacity: 16, maxSize: 32);
            }

            // TimeManager 1초 틱으로 카운트다운 연결
            var time = ServiceLocator.Get<TimeManager>();
            time.OnEvery1Sec += GameplayManager.Instance.OnSecondTick;

            // HUD 표시
            var config = AchScriptableObject.GetOrAdd<GameConfig>();
            UI.Show<IngameHUDView>("IngameHUD", new IngameHUDView.Payload
            {
                MaxHp        = config.MaxHp,
                TimeRemaining = config.RoundTimeSeconds,
            });

            // 게임 시작
            GameplayManager.Instance.StartGame(config.MaxHp, config.RoundTimeSeconds);

            await Task.CompletedTask;
            input.Enable();
        }

        public Task OnSceneEnd()
        {
            var time = ServiceLocator.Get<TimeManager>();
            time.OnEvery1Sec -= GameplayManager.Instance.OnSecondTick;

            ServiceLocator.Get<SoundManager>().StopBgm();

            if (cardPrefab != null)
                ServiceLocator.Get<PoolManager>().ClearPool(GameplayManager.CardPoolKey);

#if ACHENGINE_R3
            UIBindingManager.ClearAll();
#endif

            UI.CloseAll();
            return Task.CompletedTask;
        }
    }
}
