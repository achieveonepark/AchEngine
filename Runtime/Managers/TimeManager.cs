using System;
using System.Globalization;
using System.Threading;
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
    public class TimeManager : IManager, IDisposable
    {
        private const string TimeApiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";

        /// <summary>
        /// Unity TimeScale을 설정한다. 값은 0~100으로 클램프된다.
        /// </summary>
        public float TimeScale
        {
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value), "타임스케일은 유한한 값이어야 합니다.");
                UnityEngine.Time.timeScale = Mathf.Clamp(value, 0f, 100f);
            }
        }

        private DateTime _networkTime = DateTime.UtcNow;
        private double _startUnscaled;
        private bool _initialized;
        private bool _disposed;
        private Task _initializationTask;
        private CancellationTokenSource _tickCancellation;

        /// <summary>
        /// 네트워크 시간을 기준으로 보정된 현재 UTC 시각.
        /// </summary>
        public DateTime Now => _initialized
            ? _networkTime.AddSeconds(UnityEngine.Time.unscaledTimeAsDouble - _startUnscaled)
            : DateTime.UtcNow;

        /// <summary>
        /// 매 1초마다 발생하는 이벤트.
        /// </summary>
        public event Action OnEvery1Sec;

        /// <summary>
        /// 네트워크 시간을 가져오고 1초 단위 틱 루프를 시작한다.
        /// </summary>
        public Task Initialize()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TimeManager));
            return _initializationTask ??= InitializeCoreAsync();
        }

        private async Task InitializeCoreAsync()
        {
            await FetchNetworkTimeAsync();
            if (_disposed) return;
            _startUnscaled = UnityEngine.Time.unscaledTimeAsDouble;
            _initialized = true;
            _tickCancellation = new CancellationTokenSource();
            _ = TickLoop(_tickCancellation.Token);
        }

        private async Task TickLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    await AchTimer.WaitRealtime(1f, cancellationToken);
                    InvokeTickHandlersSafely();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // 정상적인 종료입니다.
            }
        }

        private async Task FetchNetworkTimeAsync()
        {
            try
            {
                using var req = UnityWebRequest.Get(TimeApiUrl);
                req.timeout = 10;
                await req.SendWebRequest().ToAchTask();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var resp = JsonConvert.DeserializeObject<NtpResponse>(req.downloadHandler.text);
                    if (resp != null && DateTimeOffset.TryParse(
                            resp.dateTime,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out var parsed))
                    {
                        _networkTime = parsed.UtcDateTime;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TimeManager] 네트워크 시간 요청 중 오류가 발생했습니다: {e.Message}");
            }

            _networkTime = DateTime.UtcNow;
            Debug.LogWarning("[TimeManager] 네트워크 시간을 가져오지 못해 로컬 UTC를 사용합니다.");
        }

        private void InvokeTickHandlersSafely()
        {
            var handlers = OnEvery1Sec;
            if (handlers == null) return;

            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception e)
                {
                    Debug.LogError("[TimeManager] OnEvery1Sec 구독자 실행 중 예외가 발생했습니다.");
                    Debug.LogException(e);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _tickCancellation?.Cancel();
            _tickCancellation?.Dispose();
            _tickCancellation = null;
        }

        private class NtpResponse
        {
            public string dateTime;
        }
    }
}
