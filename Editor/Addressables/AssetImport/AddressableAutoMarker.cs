using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AchEngine.Assets.Editor
{
    /// <summary>
    /// 감시 폴더로 임포트된 에셋을 자동으로 Addressable로 마킹합니다.
    /// </summary>
    public class AddressableAutoMarker : AssetPostprocessor
    {
        private static bool _isProcessing;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (_isProcessing)
                return;

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            var editorSettings = AddressableManagerEditorSettings.instance;
            if (editorSettings.watchedFolders == null || editorSettings.watchedFolders.Count == 0)
                return;

            _isProcessing = true;

            try
            {
                foreach (var assetPath in importedAssets)
                {
                    TryMarkAsAddressable(assetPath, settings, editorSettings);
                }

                foreach (var assetPath in movedAssets)
                {
                    TryMarkAsAddressable(assetPath, settings, editorSettings);
                }

                for (int i = 0; i < movedFromAssetPaths.Length; i++)
                {
                    var oldPath = movedFromAssetPaths[i];
                    var newPath = movedAssets[i];

                    if (editorSettings.IsWatchedPath(oldPath) && !editorSettings.IsWatchedPath(newPath))
                    {
                        RemoveAddressableEntry(oldPath, settings);
                    }
                }

                foreach (var assetPath in deletedAssets)
                {
                    if (editorSettings.IsWatchedPath(assetPath))
                    {
                        RemoveAddressableEntry(assetPath, settings);
                    }
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private static void TryMarkAsAddressable(
            string assetPath,
            AddressableAssetSettings settings,
            AddressableManagerEditorSettings editorSettings)
        {
            if (!editorSettings.IsWatchedPath(assetPath))
                return;

            if (AssetDatabase.IsValidFolder(assetPath))
                return;

            var folderConfig = editorSettings.GetWatchedFolderFor(assetPath);
            if (folderConfig == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            var group = FindOrCreateGroup(settings, folderConfig.groupName);
            var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);

            if (entry != null)
            {
                entry.address = editorSettings.GenerateAddress(assetPath, folderConfig);

                foreach (var label in folderConfig.labels)
                {
                    if (!string.IsNullOrEmpty(label))
                    {
                        settings.AddLabel(label);
                        entry.SetLabel(label, true, false);
                    }
                }

                Debug.Log($"[AddressableAutoMarker] Marked as Addressable: {assetPath} -> {entry.address}");
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, postEvent: true);
        }

        private static void RemoveAddressableEntry(string assetPath, AddressableAssetSettings settings)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            var entry = settings.FindAssetEntry(guid);
            if (entry != null)
            {
                settings.RemoveAssetEntry(guid);
                Debug.Log($"[AddressableAutoMarker] Removed Addressable entry: {assetPath}");
            }
        }

        private static AddressableAssetGroup FindOrCreateGroup(
            AddressableAssetSettings settings, string groupName)
        {
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, true,
                    settings.DefaultGroup.Schemas);
                Debug.Log($"[AddressableAutoMarker] Created Addressable group: {groupName}");
            }
            return group;
        }
    }
}
