#if UNITY_6000_3_OR_NEWER
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace AchEngine.Editor.Decorators
{
    /// <summary>
    /// SceneView HUD overlay built with UI Toolkit. Toggleable from
    /// the SceneView overlay menu and from the AchEngine decorator preferences.
    /// </summary>
    [Overlay(typeof(SceneView), OverlayId, "AchEngine HUD", true)]
    [Icon("d_UnityEditor.SceneView")]
    internal sealed class AchEngineSceneViewOverlay : Overlay
    {
        private const string OverlayId = "achengine-scene-hud";

        private Label _sceneLabel;
        private Label _selectionLabel;
        private Label _positionLabel;
        private Label _statsLabel;
        private VisualElement _root;

        public override VisualElement CreatePanelContent()
        {
            _root = new VisualElement();
            _root.AddToClassList("ach-scene-overlay");
            var theme = LoadTheme();
            if (theme != null)
            {
                _root.styleSheets.Add(theme);
            }

            _sceneLabel = AddRow("Scene", "—");
            _selectionLabel = AddRow("Selection", "—");
            _positionLabel = AddRow("Position", "—");
            _statsLabel = AddRow("Stats", "—");

            Selection.selectionChanged += Refresh;
            EditorApplication.hierarchyChanged += Refresh;
            _root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                Selection.selectionChanged -= Refresh;
                EditorApplication.hierarchyChanged -= Refresh;
            });

            _root.schedule.Execute(Refresh).Every(250);
            Refresh();
            return _root;
        }

        private Label AddRow(string key, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("ach-scene-row");
            var k = new Label(key);
            k.AddToClassList("ach-scene-key");
            var v = new Label(value);
            v.AddToClassList("ach-scene-value");
            row.Add(k);
            row.Add(v);
            _root.Add(row);
            return v;
        }

        private void Refresh()
        {
            if (!AchEngineDecoratorSettings.SceneOverlayEnabled)
            {
                if (_root != null)
                {
                    _root.style.display = DisplayStyle.None;
                }
                return;
            }
            if (_root == null)
            {
                return;
            }
            _root.style.display = DisplayStyle.Flex;

            var active = SceneManager.GetActiveScene();
            _sceneLabel.text = string.IsNullOrEmpty(active.name) ? "<untitled>" : active.name;

            var go = Selection.activeGameObject;
            if (go != null)
            {
                _selectionLabel.text = $"{go.name}  ({go.GetComponents<Component>().Length} comps)";
                var p = go.transform.position;
                _positionLabel.text = $"{p.x:0.##}, {p.y:0.##}, {p.z:0.##}";
            }
            else
            {
                _selectionLabel.text = "—";
                _positionLabel.text = "—";
            }

            var rootCount = active.rootCount;
            _statsLabel.text = $"{rootCount} root  ·  ts {Time.timeScale:0.##}";
        }

        private static StyleSheet LoadTheme()
        {
            var guids = AssetDatabase.FindAssets("AchEngineInspectorTheme t:StyleSheet");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                    if (sheet != null)
                    {
                        return sheet;
                    }
                }
            }
            return null;
        }
    }
}
#endif
