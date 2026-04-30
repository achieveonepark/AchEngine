#if UNITY_6000_3_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor.Decorators
{
    /// <summary>
    /// EditorPrefs-backed toggles for the AchEngine hierarchy / project / overlay decorators.
    /// Exposed via Preferences > AchEngine > Decorators.
    /// </summary>
    internal static class AchEngineDecoratorSettings
    {
        private const string Prefix = "AchEngine.Decorators.";

        public static bool HierarchyEnabled
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.Enabled", true);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.Enabled", value);
        }

        public static bool HierarchyStripes
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.Stripes", true);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.Stripes", value);
        }

        public static bool HierarchySectionHeaders
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.Sections", true);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.Sections", value);
        }

        public static bool HierarchyComponentIcons
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.ComponentIcons", true);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.ComponentIcons", value);
        }

        public static bool HierarchyActiveToggle
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.ActiveToggle", true);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.ActiveToggle", value);
        }

        public static bool HierarchyBadges
        {
            get => EditorPrefs.GetBool(Prefix + "Hierarchy.Badges", true);
            set => EditorPrefs.SetBool(Prefix + "Hierarchy.Badges", value);
        }

        public static bool ProjectEnabled
        {
            get => EditorPrefs.GetBool(Prefix + "Project.Enabled", true);
            set => EditorPrefs.SetBool(Prefix + "Project.Enabled", value);
        }

        public static bool ProjectStripes
        {
            get => EditorPrefs.GetBool(Prefix + "Project.Stripes", true);
            set => EditorPrefs.SetBool(Prefix + "Project.Stripes", value);
        }

        public static bool ProjectFileSizeBadge
        {
            get => EditorPrefs.GetBool(Prefix + "Project.FileSize", true);
            set => EditorPrefs.SetBool(Prefix + "Project.FileSize", value);
        }

        public static bool ProjectFolderItemCount
        {
            get => EditorPrefs.GetBool(Prefix + "Project.FolderCount", true);
            set => EditorPrefs.SetBool(Prefix + "Project.FolderCount", value);
        }

        public static bool SceneOverlayEnabled
        {
            get => EditorPrefs.GetBool(Prefix + "Scene.Overlay", true);
            set => EditorPrefs.SetBool(Prefix + "Scene.Overlay", value);
        }

        public static bool GameOverlayEnabled
        {
            get => EditorPrefs.GetBool(Prefix + "Game.Overlay", true);
            set => EditorPrefs.SetBool(Prefix + "Game.Overlay", value);
        }

        public static event System.Action Changed;

        public static void RaiseChanged()
        {
            Changed?.Invoke();
            EditorApplication.RepaintHierarchyWindow();
            InternalEditorUtility_RepaintAllViews();
        }

        private static void InternalEditorUtility_RepaintAllViews()
        {
            // Use reflection to avoid hard dep on internal API surface.
            var type = typeof(UnityEditor.EditorApplication).Assembly
                .GetType("UnityEditorInternal.InternalEditorUtility");
            var method = type?.GetMethod("RepaintAllViews",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            method?.Invoke(null, null);
        }
    }

    internal static class AchEngineDecoratorSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider("Preferences/AchEngine/Decorators", SettingsScope.User)
            {
                label = "AchEngine Decorators",
                keywords = new[] { "achengine", "hierarchy", "project", "scene", "game", "decorator", "theme" },
                guiHandler = _ => DrawGUI(),
            };
        }

        private static void DrawGUI()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Hierarchy", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                DrawToggle("Enabled",
                    AchEngineDecoratorSettings.HierarchyEnabled,
                    v => AchEngineDecoratorSettings.HierarchyEnabled = v);

                using (new EditorGUI.DisabledScope(!AchEngineDecoratorSettings.HierarchyEnabled))
                {
                    DrawToggle("Row stripes",
                        AchEngineDecoratorSettings.HierarchyStripes,
                        v => AchEngineDecoratorSettings.HierarchyStripes = v);
                    DrawToggle("Section header rows  (---, ###, >>>)",
                        AchEngineDecoratorSettings.HierarchySectionHeaders,
                        v => AchEngineDecoratorSettings.HierarchySectionHeaders = v);
                    DrawToggle("Component icons",
                        AchEngineDecoratorSettings.HierarchyComponentIcons,
                        v => AchEngineDecoratorSettings.HierarchyComponentIcons = v);
                    DrawToggle("Active toggle",
                        AchEngineDecoratorSettings.HierarchyActiveToggle,
                        v => AchEngineDecoratorSettings.HierarchyActiveToggle = v);
                    DrawToggle("Tag / Layer / Static badges",
                        AchEngineDecoratorSettings.HierarchyBadges,
                        v => AchEngineDecoratorSettings.HierarchyBadges = v);
                }
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Project", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                DrawToggle("Enabled",
                    AchEngineDecoratorSettings.ProjectEnabled,
                    v => AchEngineDecoratorSettings.ProjectEnabled = v);

                using (new EditorGUI.DisabledScope(!AchEngineDecoratorSettings.ProjectEnabled))
                {
                    DrawToggle("Row stripes",
                        AchEngineDecoratorSettings.ProjectStripes,
                        v => AchEngineDecoratorSettings.ProjectStripes = v);
                    DrawToggle("File size badge",
                        AchEngineDecoratorSettings.ProjectFileSizeBadge,
                        v => AchEngineDecoratorSettings.ProjectFileSizeBadge = v);
                    DrawToggle("Folder item count",
                        AchEngineDecoratorSettings.ProjectFolderItemCount,
                        v => AchEngineDecoratorSettings.ProjectFolderItemCount = v);
                }
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Scene & Game View", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                DrawToggle("Scene HUD overlay",
                    AchEngineDecoratorSettings.SceneOverlayEnabled,
                    v => AchEngineDecoratorSettings.SceneOverlayEnabled = v);
                DrawToggle("Game HUD overlay",
                    AchEngineDecoratorSettings.GameOverlayEnabled,
                    v => AchEngineDecoratorSettings.GameOverlayEnabled = v);
            }
        }

        private static void DrawToggle(string label, bool current, System.Action<bool> setter)
        {
            EditorGUI.BeginChangeCheck();
            var next = EditorGUILayout.Toggle(label, current);
            if (EditorGUI.EndChangeCheck())
            {
                setter(next);
                AchEngineDecoratorSettings.RaiseChanged();
            }
        }
    }
}
#endif
