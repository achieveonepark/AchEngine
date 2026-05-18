using System.Collections.Generic;
using UnityEngine;

namespace AchEngine
{
    /// <summary>
    /// 활성 AchTimerHandle을 매 프레임 업데이트하는 내부 싱글톤 MonoBehaviour.
    /// 씬이 로드되기 전에 자동으로 생성되므로 수동 설치가 불필요하다.
    /// </summary>
    internal sealed class AchTimerRunner : MonoBehaviour
    {
        private static AchTimerRunner _instance;

        // 현재 프레임에 업데이트 중인 타이머 목록
        private readonly List<AchTimerHandle> _active = new();

        // 다음 프레임에 _active에 추가될 타이머 (Update 도중 추가 시 동시성 문제 방지)
        private readonly List<AchTimerHandle> _pending = new();

        internal static AchTimerRunner Instance
        {
            get
            {
                // 씬 로드 이전에 이미 생성되므로 실제 null이 되는 경우는 거의 없다
                if (_instance == null)
                    CreateRunner();
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateRunner()
        {
            if (_instance != null) return;

            var go = new GameObject("[AchEngine] TimerRunner");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AchTimerRunner>();
        }

        /// <summary>새 타이머 핸들을 등록한다.</summary>
        internal void Register(AchTimerHandle handle)
        {
            _pending.Add(handle);
        }

        private void Update()
        {
            // 이번 프레임에 등록된 타이머를 활성 목록으로 이동
            if (_pending.Count > 0)
            {
                _active.AddRange(_pending);
                _pending.Clear();
            }

            // 활성 타이머를 역순으로 순회해 완료된 것은 제거
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var handle = _active[i];
                float dt = handle.UseUnscaledTime
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;

                if (handle.Tick(dt))
                    _active.RemoveAt(i);
            }
        }
    }
}
