using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.Samples.Full.Data;
using AchEngine.Samples.Full.UI;
using AchEngine.UI;
using UnityEngine;

namespace AchEngine.Samples.Full.Scenes
{
    /// <summary>
    /// 타이틀 씬 루트 GameObject에 붙이세요.
    /// AchSceneManager가 씬 전환 시 OnSceneStart / OnSceneEnd를 자동 호출합니다.
    /// </summary>
    public class TitleScene : MonoBehaviour, IScene
    {
        [SerializeField] private AudioClip titleBgm;

        public async AchTask OnSceneStart()
        {
            var input = ServiceLocator.Get<InputManager>();
            input.Disable();

            // 서버 상태 확인 (HttpLink)
            var config        = AchScriptableObject.GetOrAdd<GameConfig>();
            bool serverOnline = await CheckServerAsync(config.ServerUrl);

            // BGM 재생
            if (titleBgm != null)
                ServiceLocator.Get<SoundManager>().PlayBgm(titleBgm);

            // 네트워크 시간 표시 (TimeManager 초기화 후 제공)
            var time = ServiceLocator.Get<TimeManager>();
            Debug.Log($"[TitleScene] 서버 시간: {time.Now:HH:mm:ss}");

            // 타이틀 화면 열기
            var view = UI.Show<TitleView>("Title");
            view.SetServerStatus(serverOnline);

            input.Enable();
        }

        public AchTask OnSceneEnd()
        {
            ServiceLocator.Get<SoundManager>().StopBgm();
            return AchTask.CompletedTask;
        }

        private static async AchTask<bool> CheckServerAsync(string baseUrl)
        {
            var result = await new HttpLink.Builder()
                .SetUrl($"{baseUrl}/get")
                .SetTimeout(5)
                .GetAsync<object>();

            bool online = result != null;
            Debug.Log($"[TitleScene] 서버 상태: {(online ? "온라인" : "오프라인")}");
            return online;
        }
    }
}
