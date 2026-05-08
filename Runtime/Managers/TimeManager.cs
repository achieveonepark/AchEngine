using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

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

        public async AchTask Initialize()
        {
            await FetchNetworkTimeAsync();
            _startUnscaled = UnityEngine.Time.unscaledTime;
            _ = TickLoop();
        }

        private async AchTask TickLoop()
        {
            while (true)
            {
                await AchTask.Delay(1000);
                OnEvery1Sec?.Invoke();
            }
        }

        private async AchTask FetchNetworkTimeAsync()
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
