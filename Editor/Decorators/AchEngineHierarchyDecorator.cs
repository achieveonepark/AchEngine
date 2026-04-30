#if UNITY_6000_3_OR_NEWER
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AchEngine.Editor.Decorators
{
    /// <summary>
    /// Draws extra decorations on top of the standard Hierarchy rows:
    /// alternating stripes, section headers, component icons,
    /// active toggle, and tag/layer/static badges.
    /// </summary>
    [InitializeOnLoad]
    internal static class AchEngineHierarchyDecorator
    {
        private const int RowHeight = 16;
        private const int IconSize = 14;
        private const int IconSpacing = 1;
        private const int MaxComponentIcons = 5;

        private static readonly Color StripeColor = new Color(1f, 1f, 1f, 0.025f);
        private static readonly Color HeaderBg = new Color(0.06f, 0.20f, 0.38f, 0.55f);
        private static readonly Color HeaderAccent = new Color(0.36f, 0.63f, 0.83f, 1f);
        private static readonly Color BadgeStaticBg = new Color(0.62f, 0.42f, 0.13f, 0.85f);
        private static readonly Color BadgeLayerBg = new Color(0.18f, 0.36f, 0.55f, 0.85f);
        private static readonly Color BadgeTagBg = new Color(0.18f, 0.50f, 0.32f, 0.85f);
        private static readonly Color BadgeText = new Color(0.94f, 0.94f, 0.94f, 1f);

        private static GUIStyle _headerStyle;
        private static GUIStyle _badgeStyle;
        private static readonly List<Component> _componentBuffer = new List<Component>(16);

        static AchEngineHierarchyDecorator()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            AchEngineDecoratorSettings.Changed -= EditorApplication.RepaintHierarchyWindow;
            AchEngineDecoratorSettings.Changed += EditorApplication.RepaintHierarchyWindow;
        }

        private static void OnHierarchyGUI(int instanceID, Rect rect)
        {
            if (!AchEngineDecoratorSettings.HierarchyEnabled)
            {
                return;
            }

            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null)
            {
                return;
            }

            EnsureStyles();

            // ----- Stripes -----
            if (AchEngineDecoratorSettings.HierarchyStripes)
            {
                var row = Mathf.FloorToInt(rect.y / RowHeight);
                if ((row & 1) == 1)
                {
                    var stripeRect = new Rect(0, rect.y, rect.xMax + 200, rect.height);
                    EditorGUI.DrawRect(stripeRect, StripeColor);
                }
            }

            // ----- Section header rows -----
            if (AchEngineDecoratorSettings.HierarchySectionHeaders &&
                TryGetSectionLabel(go.name, out var sectionLabel))
            {
                var headerRect = new Rect(32, rect.y, rect.xMax - 32 + 200, rect.height);
                EditorGUI.DrawRect(headerRect, HeaderBg);
                var accent = new Rect(headerRect.x, headerRect.yMax - 1, headerRect.width, 1);
                EditorGUI.DrawRect(accent, HeaderAccent);
                GUI.Label(headerRect, sectionLabel, _headerStyle);
                return;
            }

            var cursor = rect.xMax;

            // ----- Active toggle (rightmost) -----
            if (AchEngineDecoratorSettings.HierarchyActiveToggle)
            {
                var toggleRect = new Rect(cursor - IconSize, rect.y + (rect.height - IconSize) * 0.5f, IconSize, IconSize);
                EditorGUI.BeginChangeCheck();
                var nextActive = GUI.Toggle(toggleRect, go.activeSelf, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(go, nextActive ? "Activate GameObject" : "Deactivate GameObject");
                    go.SetActive(nextActive);
                    EditorUtility.SetDirty(go);
                }
                cursor -= IconSize + 4;
            }

            // ----- Tag / Layer / Static badges -----
            if (AchEngineDecoratorSettings.HierarchyBadges)
            {
                if (go.isStatic)
                {
                    cursor = DrawBadge(cursor, rect, "S", BadgeStaticBg);
                }
                if (go.layer != 0)
                {
                    var layerName = LayerMask.LayerToName(go.layer);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        cursor = DrawBadge(cursor, rect, layerName, BadgeLayerBg);
                    }
                }
                if (!go.CompareTag("Untagged"))
                {
                    cursor = DrawBadge(cursor, rect, go.tag, BadgeTagBg);
                }
            }

            // ----- Component icons -----
            if (AchEngineDecoratorSettings.HierarchyComponentIcons)
            {
                _componentBuffer.Clear();
                go.GetComponents(_componentBuffer);

                var drawn = 0;
                for (var i = _componentBuffer.Count - 1; i >= 0 && drawn < MaxComponentIcons; i--)
                {
                    var component = _componentBuffer[i];
                    if (component == null || component is Transform)
                    {
                        continue;
                    }

                    var icon = AssetPreview.GetMiniThumbnail(component);
                    if (icon == null)
                    {
                        continue;
                    }

                    var iconRect = new Rect(cursor - IconSize, rect.y + (rect.height - IconSize) * 0.5f, IconSize, IconSize);
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                    if (Event.current.type == EventType.MouseDown &&
                        Event.current.button == 0 &&
                        iconRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUIUtility.PingObject(component);
                        Event.current.Use();
                    }

                    cursor -= IconSize + IconSpacing;
                    drawn++;
                }
            }
        }

        private static float DrawBadge(float right, Rect row, string text, Color bg)
        {
            var content = new GUIContent(text);
            var size = _badgeStyle.CalcSize(content);
            var width = Mathf.Min(size.x + 8, 90);
            var badgeRect = new Rect(right - width, row.y + (row.height - 12) * 0.5f, width, 12);
            EditorGUI.DrawRect(badgeRect, bg);
            GUI.Label(badgeRect, content, _badgeStyle);
            return right - width - 2;
        }

        private static bool TryGetSectionLabel(string name, out string label)
        {
            label = null;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var trimmed = name.Trim();
            if (trimmed.StartsWith("---") || trimmed.StartsWith("###") || trimmed.StartsWith(">>>"))
            {
                label = trimmed.Trim('-', '#', '>', ' ', '\t').ToUpperInvariant();
                if (string.IsNullOrEmpty(label))
                {
                    label = trimmed;
                }
                return true;
            }

            return false;
        }

        private static void EnsureStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                    normal = { textColor = new Color(0.92f, 0.97f, 1f, 1f) },
                };
            }

            if (_badgeStyle == null)
            {
                _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 9,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(4, 4, 0, 0),
                    normal = { textColor = BadgeText },
                };
            }
        }
    }
}
#endif
