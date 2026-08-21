using System;
using System.Collections.Generic;
using UnityEngine;

namespace AchEngine
{
    /// <summary>
    /// 네이티브 디버그 콘솔 브릿지.
    /// 플랫폼별로 Android Java 플러그인, 에디터 IMGUI 폴백을 제공한다.
    /// </summary>
    public static class AchDebugConsole
    {
        /// <summary>최대 보관 로그 항목 수 (원형 버퍼).</summary>
        private const int MaxEntries = 500;

        /// <summary>에디터 / 폴백용 인메모리 로그 목록.</summary>
        private static readonly List<(string type, string msg, string stack)> _entries = new(MaxEntries);

        /// <summary>콘솔 표시 상태 플래그.</summary>
        private static bool _isVisible;

        /// <summary>에디터 IMGUI 스크롤 위치.</summary>
        private static Vector2 _scroll;

#if !UNITY_EDITOR && UNITY_ANDROID
        private static readonly Queue<(string type, string message)> _pendingAndroidLogs = new(MaxEntries);
        private static bool _androidPluginAvailable = true;
        private static AchDebugConsoleDispatcher _dispatcher;
#endif

        /// <summary>도메인 재로드 시 상태를 초기화하고 로그 콜백을 등록한다.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            _entries.Clear();
            _isVisible = false;
#if !UNITY_EDITOR && UNITY_ANDROID
            lock (_pendingAndroidLogs) _pendingAndroidLogs.Clear();
            _androidPluginAvailable = true;
            _dispatcher = null;
#endif
            // 도메인 재로드를 끈 Play Mode 재진입에서도 같은 콜백이 누적되지 않게 한다.
            Application.logMessageReceivedThreaded -= OnLog;
            Application.logMessageReceivedThreaded += OnLog;
        }

        /// <summary>Unity 로그 메시지를 수신하여 플랫폼별 처리를 한다.</summary>
        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            var typeStr = type.ToString();

#if UNITY_EDITOR
            // 에디터: 인메모리 버퍼에 추가 (스레드 안전)
            lock (_entries)
            {
                if (_entries.Count >= MaxEntries)
                    _entries.RemoveAt(0);
                _entries.Add((typeStr, condition, stackTrace));
            }
#elif UNITY_ANDROID
            // JNI 호출은 Unity 메인 스레드에서 처리하도록 큐에 보관합니다.
            lock (_pendingAndroidLogs)
            {
                if (_pendingAndroidLogs.Count >= MaxEntries)
                    _pendingAndroidLogs.Dequeue();
                _pendingAndroidLogs.Enqueue((typeStr, condition));
            }
#endif
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateAndroidDispatcher()
        {
            if (_dispatcher != null) return;
            var go = new GameObject("[AchEngine] DebugConsoleDispatcher")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            UnityEngine.Object.DontDestroyOnLoad(go);
            _dispatcher = go.AddComponent<AchDebugConsoleDispatcher>();
        }

        internal static void FlushAndroidLogs()
        {
            if (!_androidPluginAvailable) return;

            try
            {
                using var plugin = new AndroidJavaClass("com.achengine.debugconsole.AchDebugConsolePlugin");
                const int maxLogsPerFrame = 50;
                for (var index = 0; index < maxLogsPerFrame; index++)
                {
                    (string type, string message) entry;
                    lock (_pendingAndroidLogs)
                    {
                        if (_pendingAndroidLogs.Count == 0) break;
                        entry = _pendingAndroidLogs.Dequeue();
                    }
                    plugin.CallStatic("addLog", entry.type, entry.message);
                }
            }
            catch
            {
                // 플러그인 오류가 다시 Unity 로그를 발생시키는 재귀를 막습니다.
                _androidPluginAvailable = false;
                lock (_pendingAndroidLogs) _pendingAndroidLogs.Clear();
            }
        }
#endif

        /// <summary>콘솔이 현재 표시 중인지 여부.</summary>
        public static bool IsVisible => _isVisible;

        /// <summary>콘솔을 표시한다.</summary>
        public static void Show()
        {
            _isVisible = true;

#if !UNITY_EDITOR && UNITY_ANDROID
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var plugin = new AndroidJavaClass("com.achengine.debugconsole.AchDebugConsolePlugin");
                plugin.CallStatic("show", activity);
            }
            catch
            {
                _androidPluginAvailable = false;
                _isVisible = false;
            }
#endif
        }

        /// <summary>콘솔을 숨긴다.</summary>
        public static void Hide()
        {
            _isVisible = false;

#if !UNITY_EDITOR && UNITY_ANDROID
            if (_androidPluginAvailable)
            {
                try
                {
                    using var plugin = new AndroidJavaClass("com.achengine.debugconsole.AchDebugConsolePlugin");
                    plugin.CallStatic("hide");
                }
                catch
                {
                    _androidPluginAvailable = false;
                }
            }
#endif
        }

        /// <summary>콘솔 표시/숨김을 토글한다.</summary>
        public static void Toggle()
        {
            if (_isVisible) Hide();
            else Show();
        }

        /// <summary>로그 항목을 모두 지운다.</summary>
        public static void Clear()
        {
            lock (_entries) _entries.Clear();

#if !UNITY_EDITOR && UNITY_ANDROID
            lock (_pendingAndroidLogs) _pendingAndroidLogs.Clear();
            if (_androidPluginAvailable)
            {
                try
                {
                    using var plugin = new AndroidJavaClass("com.achengine.debugconsole.AchDebugConsolePlugin");
                    plugin.CallStatic("clear");
                }
                catch
                {
                    _androidPluginAvailable = false;
                }
            }
#endif
        }

#if UNITY_EDITOR
        // ──────────────────────────────────────────────────────────────
        // 에디터 전용 IMGUI 폴백
        // Unity 에디터에는 이미 Console 창이 있으므로 Show/Hide는 플래그만 토글한다.
        // OnGUI는 AchDebugConsoleEditorOverlay 헬퍼 MonoBehaviour에서 호출한다.
        // ──────────────────────────────────────────────────────────────

        /// <summary>에디터 IMGUI 오버레이를 그린다. 게임 뷰에서만 호출할 것.</summary>
        public static void DrawEditorGUI()
        {
            if (!_isVisible) return;

            // 반투명 배경 박스
            var windowRect = new Rect(10, 10, Screen.width - 20, Screen.height * 0.5f);
            GUI.Box(windowRect, GUIContent.none);

            GUILayout.BeginArea(windowRect);

            // 툴바 (제목 + Clear + Close)
            GUILayout.BeginHorizontal();
            GUILayout.Label($"AchDebugConsole  [{_entries.Count}/{MaxEntries}]");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", GUILayout.Width(60))) Clear();
            if (GUILayout.Button("X", GUILayout.Width(30))) Hide();
            GUILayout.EndHorizontal();

            // 로그 스크롤 뷰
            _scroll = GUILayout.BeginScrollView(_scroll);
            List<(string type, string msg, string stack)> snapshot;
            lock (_entries)
                snapshot = new List<(string, string, string)>(_entries);

            foreach (var (t, m, _) in snapshot)
            {
                // 타입에 따른 색상 지정
                var color = t switch
                {
                    "Error" or "Exception" or "Assert" => Color.red,
                    "Warning" => Color.yellow,
                    _ => Color.white,
                };
                var prev = GUI.contentColor;
                GUI.contentColor = color;
                GUILayout.Label($"[{t}] {m}");
                GUI.contentColor = prev;
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
#endif
    }

#if !UNITY_EDITOR && UNITY_ANDROID
    internal sealed class AchDebugConsoleDispatcher : MonoBehaviour
    {
        private void Update() => AchDebugConsole.FlushAndroidLogs();
    }
#endif
}
