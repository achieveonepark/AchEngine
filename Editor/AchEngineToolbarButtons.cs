using System;
#if UNITY_6000_3_OR_NEWER
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
#else
using System.Linq;
using AchEngine.Editor.Table;
using AchEngine.Localization.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace AchEngine.Editor
{
    [InitializeOnLoad]
    internal static class AchEngineToolbarButtons
    {
        private const string UIToolbarPath = "AchEngine/UI Workspace";
        private const string UIToolbarInitializedPrefKey = "AchEngine.Toolbar.UIWorkspace.Initialized";
        private const float IconButtonSize = 24f;
#if !UNITY_6000_3_OR_NEWER
        private const string ContainerName = "achengine-toolbar-buttons";

        private static EditorWindow toolbarWindow;
        private static VisualElement container;
#endif
        private static Texture2D uiToolkitIcon;

        static AchEngineToolbarButtons()
        {
#if UNITY_6000_3_OR_NEWER
            EditorApplication.delayCall += EnsureMainToolbarButtonVisible;
#else
            EditorApplication.update += TryAttachToToolbar;
#endif
        }

#if UNITY_6000_3_OR_NEWER
        [MainToolbarElement(
            UIToolbarPath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = -10)]
        private static MainToolbarElement CreateUIToolkitQuickSettingsButton()
        {
            return new MainToolbarButton(
                new MainToolbarContent(GetUIToolkitIcon(), "Open AchEngine UI Toolkit quick settings"),
                OpenUIToolkitPopup);
        }

        private static void EnsureMainToolbarButtonVisible()
        {
            if (EditorPrefs.GetBool(UIToolbarInitializedPrefKey, false))
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                MainToolbar.Refresh(UIToolbarPath);
                EditorPrefs.SetBool(UIToolbarInitializedPrefKey, true);
            };
        }

        private static void OpenUIToolkitPopup()
        {
            var mainPos = EditorGUIUtility.GetMainWindowPosition();
            var screenRect = new Rect(
                mainPos.x + mainPos.width * 0.5f,
                mainPos.y + 28f,
                Mathf.Max(IconButtonSize, 1f),
                22f);
            AchEngineUIToolkitQuickSettingsWindow.ShowPopup(screenRect);
        }
#else
        private static void TryAttachToToolbar()
        {
            if (toolbarWindow == null || toolbarWindow.rootVisualElement == null || toolbarWindow.rootVisualElement.panel == null)
            {
                toolbarWindow = FindToolbarWindow();

                if (toolbarWindow == null)
                {
                    return;
                }
            }

            var root = toolbarWindow.rootVisualElement;
            if (root == null)
            {
                return;
            }

            if (container != null && container.panel != null)
            {
                return;
            }

            container = root.Q<VisualElement>(ContainerName);
            if (container != null)
            {
                return;
            }

            var playZone = FindPlayZone(root);
            if (playZone != null && playZone.parent != null)
            {
                container = CreateContainer();
                var parent = playZone.parent;
                parent.Insert(parent.IndexOf(playZone), container);
                return;
            }

            var leftZone = FindLeftZone(root);
            if (leftZone != null)
            {
                container = CreateContainer();
                leftZone.Add(container);
            }
        }

        private static EditorWindow FindToolbarWindow()
        {
            return Resources
                .FindObjectsOfTypeAll<EditorWindow>()
                .FirstOrDefault(LooksLikeToolbarWindow);
        }

        private static bool LooksLikeToolbarWindow(EditorWindow window)
        {
            if (window == null)
            {
                return false;
            }

            var type = window.GetType();
            var fullName = type.FullName ?? string.Empty;
            if (string.Equals(fullName, "UnityEditor.Toolbar", StringComparison.Ordinal) ||
                string.Equals(fullName, "UnityEditor.MainToolbarWindow", StringComparison.Ordinal))
            {
                return true;
            }

            if (type.Name.IndexOf("Toolbar", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            return HasToolbarZones(window.rootVisualElement);
        }

        private static bool HasToolbarZones(VisualElement root)
        {
            if (root == null)
            {
                return false;
            }

            if (FindZone(root, "ToolbarZonePlayModes", "ToolbarZonePlayMode", "ToolbarZonePlay", "ToolbarZoneLeftAlign", "ToolbarZoneLeft") != null)
            {
                return true;
            }

            return root.Query<VisualElement>().ToList().Any(element =>
            {
                var name = element.name;
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }

                var lowered = name.ToLowerInvariant();
                return lowered.Contains("toolbarzone") || (lowered.Contains("toolbar") && lowered.Contains("zone"));
            });
        }

        private static VisualElement FindPlayZone(VisualElement root)
        {
            var zone = FindZone(root, "ToolbarZonePlayModes", "ToolbarZonePlayMode", "ToolbarZonePlay");
            if (zone != null)
            {
                return zone;
            }

            return root.Query<VisualElement>().ToList().FirstOrDefault(element =>
            {
                var name = element.name;
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }

                var lowered = name.ToLowerInvariant();
                return lowered.Contains("toolbarzone") && (lowered.Contains("play") || lowered.Contains("playmode"));
            });
        }

        private static VisualElement FindLeftZone(VisualElement root)
        {
            var zone = FindZone(root, "ToolbarZoneLeftAlign", "ToolbarZoneLeft");
            if (zone != null)
            {
                return zone;
            }

            return root.Query<VisualElement>().ToList().FirstOrDefault(element =>
            {
                var name = element.name;
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }

                var lowered = name.ToLowerInvariant();
                return lowered.Contains("toolbarzone") && lowered.Contains("left");
            });
        }

        private static VisualElement FindZone(VisualElement root, params string[] zoneNames)
        {
            foreach (var zoneName in zoneNames)
            {
                var zone = root.Q<VisualElement>(zoneName);
                if (zone != null)
                {
                    return zone;
                }
            }

            var allZones = root.Query<VisualElement>().ToList();
            foreach (var zoneName in zoneNames)
            {
                var partialMatch = allZones.FirstOrDefault(element =>
                    !string.IsNullOrEmpty(element.name) &&
                    element.name.IndexOf(zoneName, StringComparison.OrdinalIgnoreCase) >= 0);

                if (partialMatch != null)
                {
                    return partialMatch;
                }
            }

            return null;
        }

        private static VisualElement CreateContainer()
        {
            var row = new VisualElement
            {
                name = ContainerName
            };

            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginLeft = 6f;
            row.style.marginRight = 6f;
            row.style.flexShrink = 0f;

            row.Add(CreateUIToolkitPopupButton());
            row.Add(CreateButton("\uD14C\uC774\uBE14 \uB85C\uB4DC", TableLoaderWindow.ShowWindow, "Open the Table Loader window"));
            row.Add(CreateButton("Localization \uB85C\uB4DC", LocalizationEditorWindow.Open, "Open the Localization window"));

            return row;
        }

        private static VisualElement CreateUIToolkitPopupButton()
        {
            ToolbarButton button = null;
            button = new ToolbarButton(() => OpenUIToolkitPopup(button))
            {
                tooltip = "Open AchEngine UI Toolkit quick settings",
            };

            button.style.width = IconButtonSize;
            button.style.height = 22f;
            button.style.minWidth = IconButtonSize;
            button.style.marginRight = 6f;
            button.style.paddingLeft = 0f;
            button.style.paddingRight = 0f;
            button.style.paddingTop = 0f;
            button.style.paddingBottom = 0f;
            button.style.alignItems = Align.Center;
            button.style.justifyContent = Justify.Center;
            button.style.backgroundColor = new StyleColor(new Color(0.17f, 0.22f, 0.28f, 0.92f));
            button.style.borderTopLeftRadius = 5f;
            button.style.borderTopRightRadius = 5f;
            button.style.borderBottomLeftRadius = 5f;
            button.style.borderBottomRightRadius = 5f;

            var icon = new Image
            {
                image = GetUIToolkitIcon(),
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore,
            };
            icon.style.width = 14f;
            icon.style.height = 14f;
            button.Add(icon);

            return button;
        }

        private static VisualElement CreateButton(string label, Action onClick, string tooltip)
        {
            var button = new ToolbarButton(onClick)
            {
                text = label,
                tooltip = tooltip
            };

            button.style.marginRight = 4f;
            button.style.flexShrink = 0f;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.whiteSpace = WhiteSpace.NoWrap;
            return button;
        }

        private static void OpenUIToolkitPopup(VisualElement anchor)
        {
            if (anchor == null || toolbarWindow == null)
            {
                return;
            }

            var rect = anchor.worldBound;
            var toolbarRect = toolbarWindow.position;
            var screenRect = new Rect(
                toolbarRect.x + rect.x,
                toolbarRect.y + rect.yMax,
                Mathf.Max(rect.width, IconButtonSize),
                rect.height);

            AchEngineUIToolkitQuickSettingsWindow.ShowPopup(screenRect);
        }
#endif

        private static Texture2D GetUIToolkitIcon()
        {
            if (uiToolkitIcon != null)
            {
                return uiToolkitIcon;
            }

            const int size = 16;
            uiToolkitIcon = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "AchEngineUIToolkitToolbarIcon",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            var clear = new Color(0f, 0f, 0f, 0f);
            var surface = new Color(0.14f, 0.19f, 0.25f, 1f);
            var accent = new Color(0.36f, 0.63f, 0.83f, 1f);
            var accentSoft = new Color(0.78f, 0.90f, 0.98f, 1f);

            var pixels = new Color[size * size];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = clear;
            }

            SetRect(pixels, size, 1, 2, 14, 12, surface);
            SetOutline(pixels, size, 1, 2, 14, 12, accent);
            SetRect(pixels, size, 3, 4, 4, 3, accentSoft);
            SetRect(pixels, size, 8, 4, 4, 3, accent);
            SetRect(pixels, size, 3, 9, 9, 2, accentSoft);
            SetRect(pixels, size, 6, 0, 4, 2, accent);

            uiToolkitIcon.SetPixels(pixels);
            uiToolkitIcon.Apply();
            return uiToolkitIcon;
        }

        private static void SetRect(Color[] pixels, int size, int x, int y, int width, int height, Color color)
        {
            for (var yy = y; yy < y + height; yy++)
            {
                for (var xx = x; xx < x + width; xx++)
                {
                    if (xx < 0 || yy < 0 || xx >= size || yy >= size)
                    {
                        continue;
                    }

                    pixels[(yy * size) + xx] = color;
                }
            }
        }

        private static void SetOutline(Color[] pixels, int size, int x, int y, int width, int height, Color color)
        {
            SetRect(pixels, size, x, y, width, 1, color);
            SetRect(pixels, size, x, y + height - 1, width, 1, color);
            SetRect(pixels, size, x, y, 1, height, color);
            SetRect(pixels, size, x + width - 1, y, 1, height, color);
        }
    }

    internal sealed class AchEngineUIToolkitQuickSettingsWindow : EditorWindow
    {
        private static readonly Vector2 WindowSize = new Vector2(460f, 520f);

        public static void ShowPopup(Rect anchorRect)
        {
            var window = CreateInstance<AchEngineUIToolkitQuickSettingsWindow>();
            window.titleContent = new GUIContent("AchEngine UI");
            window.ShowAsDropDown(anchorRect, WindowSize);
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.minWidth = WindowSize.x;
            rootVisualElement.style.minHeight = WindowSize.y;

            AchEngineSettingsProvider.BuildUIWorkspaceSettingsPanel(rootVisualElement);
            rootVisualElement.Add(BuildFooter());
        }

        private VisualElement BuildFooter()
        {
            var footer = new VisualElement();
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.justifyContent = Justify.FlexEnd;
            footer.style.paddingLeft = 16f;
            footer.style.paddingRight = 16f;
            footer.style.paddingTop = 10f;
            footer.style.paddingBottom = 12f;
            footer.style.borderTopWidth = 1f;
            footer.style.borderTopColor = new StyleColor(AchEngineEditorUI.ColorDivider);
            footer.style.backgroundColor = new StyleColor(new Color(0.11f, 0.11f, 0.11f, 0.96f));

            var workspaceButton = new Button(() => AchEngine.Editor.UI.AchEngineUIWorkspaceWindow.Open(Selection.activeObject as AchEngine.UI.UIViewCatalog))
            {
                text = "Open Workspace",
                tooltip = "Open the full UI Workspace window.",
            };
            workspaceButton.AddToClassList("btn-secondary");
            workspaceButton.style.marginRight = 6f;

            var settingsButton = new Button(() => SettingsService.OpenProjectSettings("Project/AchEngine/UI Workspace"))
            {
                text = "Project Settings",
                tooltip = "Open the full Project Settings page for AchEngine UI Workspace.",
            };
            settingsButton.AddToClassList("btn-primary");

            footer.Add(workspaceButton);
            footer.Add(settingsButton);
            return footer;
        }
    }
}
