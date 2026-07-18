#if ACHENGINE_ADDRESSABLES
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AchEngine.Assets.Editor
{
    public static class BuildMenuItems
    {
        [MenuItem("AchEngine/Addressables/Build Content", priority = 100)]
        public static void BuildAddressableContent()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[AddressableManager] AddressableAssetSettings를 생성하지 못했습니다.");
                EditorUtility.DisplayDialog("Addressables 빌드",
                    "AddressableAssetSettings를 생성하지 못했습니다.", "확인");
                return;
            }

            Debug.Log("[AddressableManager] Building Addressable content...");

            AddressableAssetSettings.BuildPlayerContent(out var result);

            if (string.IsNullOrEmpty(result.Error))
            {
                Debug.Log($"[AddressableManager] Build completed successfully. " +
                           $"Duration: {result.Duration:F1}s");
                EditorUtility.DisplayDialog("Addressables 빌드",
                    $"빌드가 완료되었습니다.\n소요 시간: {result.Duration:F1}초", "확인");
            }
            else
            {
                Debug.LogError($"[AddressableManager] Build failed: {result.Error}");
                EditorUtility.DisplayDialog("Addressables 빌드",
                    $"빌드 실패:\n{result.Error}", "확인");
            }
        }

        [MenuItem("AchEngine/Addressables/Clean Build", priority = 101)]
        public static void CleanBuildAddressableContent()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[AddressableManager] AddressableAssetSettings를 생성하지 못했습니다.");
                EditorUtility.DisplayDialog("Addressables 빌드",
                    "AddressableAssetSettings를 생성하지 못했습니다.", "확인");
                return;
            }

            Debug.Log("[AddressableManager] Cleaning and rebuilding Addressable content...");

            AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);

            BuildAddressableContent();
        }

        [MenuItem("AchEngine/Addressables/Settings", priority = 200)]
        public static void OpenSettings()
        {
            SettingsService.OpenProjectSettings("Project/AchEngine/Addressables");
        }

        [MenuItem("AchEngine/Addressables/Re-scan Watched Folders", priority = 300)]
        public static void RescanWatchedFolders()
        {
            var editorSettings = AddressableManagerEditorSettings.instance;
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);

            if (settings == null)
            {
                Debug.LogError("[AddressableManager] AddressableAssetSettings를 생성하지 못했습니다.");
                return;
            }

            int count = 0;
            foreach (var folder in editorSettings.watchedFolders)
            {
                if (string.IsNullOrEmpty(folder.folderPath))
                    continue;

                var guids = AssetDatabase.FindAssets("", new[] { folder.folderPath });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(path))
                        continue;

                    if (!folder.recursive)
                    {
                        var normalizedFolder = folder.folderPath.TrimEnd('/') + "/";
                        var relative = path.Substring(normalizedFolder.Length);
                        if (relative.Contains("/"))
                            continue;
                    }

                    var group = settings.FindGroup(folder.groupName)
                                ?? settings.CreateGroup(folder.groupName, false, false, true,
                                    settings.DefaultGroup.Schemas);

                    var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
                    if (entry != null)
                    {
                        entry.address = editorSettings.GenerateAddress(path, folder);
                        foreach (var label in folder.labels)
                        {
                            if (!string.IsNullOrEmpty(label))
                            {
                                settings.AddLabel(label);
                                entry.SetLabel(label, true, false);
                            }
                        }
                        count++;
                    }
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
            Debug.Log($"[AddressableManager] Re-scan complete. {count} assets marked as Addressable.");
            EditorUtility.DisplayDialog("다시 스캔 완료",
                $"{count}개 에셋을 Addressable로 마킹했습니다.", "확인");
        }
    }
}
#endif
