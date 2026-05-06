using System;
using UnityEngine;
using UnityEngine.Networking;
#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine.Managers
{
    public class TimeManager : IManager
    {
        private const string TimeApiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";

        public float TimeScale
        {
            set => UnityEngine.Time.timeScale = Mathf.Clamp(value, 0f, 100f);
        }

        private DateTime _networkTime;
        private float _startUnscaled;

        public DateTime Now => _networkTime.AddSeconds(UnityEngine.Time.unscaledTime - _startUnscaled);

        public event Action OnEvery1Sec;

#if ACHENGINE_UNITASK
        public async UniTask Initialize()
        {
            await FetchNetworkTimeAsync();
            _startUnscaled = UnityEngine.Time.unscaledTime;
            TickLoop().Forget();
        }

        private async UniTask TickLoop()
        {
            while (true)
            {
                await UniTask.Delay(1000);
                OnEvery1Sec?.Invoke();
            }
        }
#else
        public async Task Initialize()
        {
            await FetchNetworkTimeAsync();
            _startUnscaled = UnityEngine.Time.unscaledTime;
            _ = TickLoop();
        }

        private async Task TickLoop()
        {
            while (true)
            {
                await Task.Delay(1000);
                OnEvery1Sec?.Invoke();
            }
        }
#endif

        private async
#if ACHENGINE_UNITASK
        UniTask
#else
        Task
#endif
        FetchNetworkTimeAsync()
        {
            try
            {
                using var req = UnityWebRequest.Get(TimeApiUrl);
#if ACHENGINE_UNITASK
                await req.SendWebRequest().ToUniTask();
#else
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();
#endif
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var resp = JsonUtility.FromJson<NtpResponse>(req.downloadHandler.text);
                    if (resp != null && !string.IsNullOrEmpty(resp.dateTime))
                    {
                        _networkTime = DateTime.Parse(resp.dateTime);
                        return;
                    }
                }
            }
            catch { /* fall through to local time */ }

            _networkTime = DateTime.UtcNow;
            Debug.LogWarning("[TimeManager] Failed to fetch network time. Using local UTC.");
        }

        [Serializable]
        private class NtpResponse
        {
            public string dateTime;
        }
    }
}
