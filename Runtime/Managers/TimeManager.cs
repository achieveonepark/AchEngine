using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace AchEngine.Managers
{
    /// <summary>
    /// 네트워크 시간을 기반으로 현재 UTC 시각을 제공하고, 타임스케일을 조절하는 시간 매니저.
    /// 초기화 시 외부 API에서 시간을 받아오며 실패 시 로컬 UTC로 대체한다.
    /// </summary>
    public class TimeManager : IManager
    {
        private const string TimeApiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";

        /// <summary>
        /// Unity TimeScale을 설정한다. 값은 0~100으로 클램프된다.
        /// </summary>
        public float TimeScale
        {
            set => UnityEngine.Time.timeScale = Mathf.Clamp(value, 0f, 100f);
        }

        private DateTime _networkTime;
        private float _startUnscaled;

        /// <summary>
        /// 네트워크 시간을 기준으로 보정된 현재 UTC 시각.
        /// </summary>
        public DateTime Now => _networkTime.AddSeconds(UnityEngine.Time.unscaledTime - _startUnscaled);

        /// <summary>
        /// 매 1초마다 발생하는 이벤트.
        /// </summary>
        public event Action OnEvery1Sec;

        /// <summary>
        /// 네트워크 시간을 가져오고 1초 단위 틱 루프를 시작한다.
        /// </summary>
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

        private async Task FetchNetworkTimeAsync()
        {
            try
            {
                using var req = UnityWebRequest.Get(TimeApiUrl);
                await req.SendWebRequest().ToAchTask();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var resp = JsonConvert.DeserializeObject<NtpResponse>(req.downloadHandler.text);
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

        private class NtpResponse
        {
            public string dateTime;
        }
    }
}
