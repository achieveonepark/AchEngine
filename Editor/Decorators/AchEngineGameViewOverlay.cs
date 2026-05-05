#if UNITY_6000_3_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor.Decorators
{
    /// <summary>
    /// Injects a small UI Toolkit HUD on top of every GameView root.
    /// Shows FPS, resolution, and time scale. Updates ~4Hz.
    /// </summary>
    [InitializeOnLoad]
    internal static class AchEngineGameViewOverlay
    {
        private const string OverlayName = "ach-game-overlay";
        private const string OverlayBodyName = "ach-game-overlay-body";
        private const string OverlayToggleName = "ach-game-overlay-toggle";
        private const string OverlayToggleCollapsedClass = "ach-game-toggle--collapsed";
        private const string CollapsedPrefKey = "AchEngine.Decorators.Game.OverlayCollapsed";
        private const double TickInterval = 0.5;

        private static StyleSheet _theme;
        private static Type _gameViewType;
        private static double _nextTickTime;

        // FPS smoothing
        private static double _lastFrameTime;
        private static float _fpsSmoothed;

        static AchEngineGameViewOverlay()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            AchEngineDecoratorSettings.Changed -= OnSettingsChanged;
            AchEngineDecoratorSettings.Changed += OnSettingsChanged;
        }

        private static void OnSettingsChanged()
        {
            _nextTickTime = 0;
        }

        private static void OnEditorUpdate()
        {
            UpdateFps();

            if (EditorApplication.timeSinceStartup < _nextTickTime)
            {
                return;
            }
            _nextTickTime = EditorApplication.timeSinceStartup + TickInterval;

            ResolveGameViewType();
            if (_gameViewType == null)
            {
                return;
            }

            var enabled = AchEngineDecoratorSettings.GameOverlayEnabled;
            if (enabled && _theme == null)
            {
                _theme = LoadTheme();
            }

            foreach (var window in EnumerateGameViews())
            {
                var root = window.rootVisualElement;
                if (root == null)
                {
                    continue;
                }

                var overlay = root.Q<VisualElement>(OverlayName);
                if (!enabled)
                {
                    overlay?.RemoveFromHierarchy();
                    continue;
                }

                if (_theme != null && !root.styleSheets.Contains(_theme))
                {
                    root.styleSheets.Add(_theme);
                }

                if (overlay == null)
                {
                    overlay = BuildOverlay();
                    root.Add(overlay);
                }

                RefreshOverlay(overlay, window);
            }
        }

        private static void UpdateFps()
        {
            var now = EditorApplication.timeSinceStartup;
            var dt = now - _lastFrameTime;
            _lastFrameTime = now;
            if (dt > 0 && dt < 1)
            {
                var fps = (float)(1.0 / dt);
                _fpsSmoothed = _fpsSmoothed <= 0 ? fps : Mathf.Lerp(_fpsSmoothed, fps, 0.1f);
            }
        }

        private static VisualElement BuildOverlay()
        {
            var root = new VisualElement { name = OverlayName };
            root.AddToClassList("ach-game-overlay");

            var header = new VisualElement();
            header.AddToClassList("ach-game-header");

            var title = new Label("Game HUD");
            title.AddToClassList("ach-game-title");
            header.Add(title);

            var toggle = new Button(ToggleOverlayCollapsed)
            {
                name = OverlayToggleName,
                text = GetToggleLabel(IsOverlayCollapsed),
                tooltip = "Collapse or expand the Game HUD.",
            };
            toggle.AddToClassList("ach-game-toggle");
            header.Add(toggle);

            root.Add(header);

            var body = new VisualElement { name = OverlayBodyName };
            body.AddToClassList("ach-game-body");
            body.Add(MakeRow("fps", "ach-game-fps"));
            body.Add(MakeRow("res", "ach-game-res"));
            body.Add(MakeRow("ts",  "ach-game-ts"));
            root.Add(body);

            ApplyCollapsedState(root);
            return root;
        }

        private static VisualElement MakeRow(string label, string valueName)
        {
            var row = new VisualElement();
            row.AddToClassList("ach-game-row");

            var k = new Label(label);
            k.AddToClassList("ach-game-key");
            row.Add(k);

            var v = new Label("—") { name = valueName };
            v.AddToClassList("ach-game-value");
            row.Add(v);
            return row;
        }

        private static void RefreshOverlay(VisualElement overlay, EditorWindow window)
        {
            ApplyCollapsedState(overlay);

            var fps = overlay.Q<Label>("ach-game-fps");
            var res = overlay.Q<Label>("ach-game-res");
            var ts  = overlay.Q<Label>("ach-game-ts");

            if (fps != null)
            {
                fps.text = $"{_fpsSmoothed:0.0}";
            }
            if (res != null)
            {
                var p = window.position;
                res.text = $"{(int)p.width}x{(int)p.height}";
            }
            if (ts != null)
            {
                ts.text = $"{Time.timeScale:0.##}x";
            }
        }

        private static bool IsOverlayCollapsed
        {
            get => EditorPrefs.GetBool(CollapsedPrefKey, false);
            set => EditorPrefs.SetBool(CollapsedPrefKey, value);
        }

        private static void ToggleOverlayCollapsed()
        {
            IsOverlayCollapsed = !IsOverlayCollapsed;
            _nextTickTime = 0;
        }

        private static void ApplyCollapsedState(VisualElement overlay)
        {
            if (overlay == null)
            {
                return;
            }

            var body = overlay.Q<VisualElement>(OverlayBodyName);
            if (body != null)
            {
                body.style.display = IsOverlayCollapsed ? DisplayStyle.None : DisplayStyle.Flex;
            }

            var toggle = overlay.Q<Button>(OverlayToggleName);
            if (toggle != null)
            {
                toggle.text = GetToggleLabel(IsOverlayCollapsed);
                toggle.EnableInClassList(OverlayToggleCollapsedClass, IsOverlayCollapsed);
            }
        }

        private static string GetToggleLabel(bool collapsed)
        {
            return collapsed ? "> Show" : "v Hide";
        }

        private static void ResolveGameViewType()
        {
            if (_gameViewType != null)
            {
                return;
            }
            _gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        }

        private static IEnumerable<EditorWindow> EnumerateGameViews()
        {
            if (_gameViewType == null)
            {
                yield break;
            }
            var windows = Resources.FindObjectsOfTypeAll(_gameViewType);
            foreach (var w in windows)
            {
                if (w is EditorWindow ew)
                {
                    yield return ew;
                }
            }
        }

        private static StyleSheet LoadTheme()
        {
            var guids = AssetDatabase.FindAssets("AchEngineInspectorTheme t:StyleSheet");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }
                var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (sheet != null)
                {
                    return sheet;
                }
            }
            return null;
        }
    }
}
#endif
