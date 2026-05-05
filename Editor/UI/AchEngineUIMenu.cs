using AchEngine.UI;
using UnityEditor;
using UnityEngine;

namespace AchEngine.Editor.UI
{
    public static class AchEngineUIMenu
    {
        [MenuItem("AchEngine/UI Workspace", priority = 1)]
        public static void OpenWorkspace()
        {
            AchEngineUIWorkspaceWindow.Open(Selection.activeObject as UIViewCatalog);
        }

        [MenuItem("Assets/Create/AchEngine/View Catalog", priority = 320)]
        public static void CreateViewCatalog()
        {
            AchEngineUIEditorUtility.CreateViewCatalogAsset();
        }

        [MenuItem("GameObject/AchEngine/UI Root", false, 10)]
        public static void CreateUIRoot(MenuCommand command)
        {
            AchEngineUIEditorUtility.CreateUIRoot(command.context as GameObject);
        }

        [MenuItem("GameObject/AchEngine/Bootstrapper", false, 11)]
        public static void CreateBootstrapper(MenuCommand command)
        {
            var selectedCatalog = Selection.activeObject as UIViewCatalog;
            var existingRoot = AchEngineUIEditorUtility.FindUIRootInOpenScenes();
            AchEngineUIEditorUtility.CreateBootstrapper(selectedCatalog, existingRoot, command.context as GameObject);
        }
    }
}
