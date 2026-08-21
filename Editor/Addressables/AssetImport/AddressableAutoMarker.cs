#if ACHENGINE_ADDRESSABLES
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AchEngine.Assets.Editor
{
    public class AddressableAutoMarker : AssetPostprocessor
    {
        private const string GuidSessionPrefix = "AchEngine.AddressableAutoMarker.Guid.";
        private static bool _isProcessing;

        internal static int RescanWatchedFolders()
        {
            if (_isProcessing)
            {
                return 0;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return 0;
            }

            var editorSettings = AddressableManagerEditorSettings.instance;
            if (editorSettings.watchedFolders == null || editorSettings.watchedFolders.Count == 0)
            {
                return 0;
            }

            var seenAssets = new HashSet<string>();
            var processedCount = 0;
            _isProcessing = true;

            try
            {
                foreach (var folder in editorSettings.watchedFolders)
                {
                    if (folder == null || string.IsNullOrWhiteSpace(folder.folderPath) ||
                        !AssetDatabase.IsValidFolder(folder.folderPath))
                    {
                        continue;
                    }

                    foreach (var guid in AssetDatabase.FindAssets(string.Empty, new[] { folder.folderPath }))
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (!seenAssets.Add(assetPath))
                        {
                            continue;
                        }

                        if (TryMarkAsAddressable(assetPath, settings, editorSettings))
                        {
                            processedCount++;
                        }
                    }
                }
            }
            finally
            {
                _isProcessing = false;
            }

            return processedCount;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (_isProcessing)
            {
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return;
            }

            var editorSettings = AddressableManagerEditorSettings.instance;
            if (editorSettings.watchedFolders == null || editorSettings.watchedFolders.Count == 0)
            {
                return;
            }

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

                int movedCount = System.Math.Min(movedFromAssetPaths.Length, movedAssets.Length);
                for (int i = 0; i < movedCount; i++)
                {
                    var oldPath = movedFromAssetPaths[i];
                    var newPath = movedAssets[i];

                    if (editorSettings.IsWatchedPath(oldPath) && !editorSettings.IsWatchedPath(newPath))
                    {
                        RemoveAddressableEntry(
                            AssetDatabase.AssetPathToGUID(newPath),
                            oldPath,
                            settings);
                    }

                    SessionState.EraseString(GetGuidSessionKey(oldPath));
                }

                foreach (var assetPath in deletedAssets)
                {
                    if (editorSettings.IsWatchedPath(assetPath))
                    {
                        RemoveAddressableEntry(GetKnownGuid(assetPath), assetPath, settings);
                    }
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private static bool TryMarkAsAddressable(
            string assetPath,
            AddressableAssetSettings settings,
            AddressableManagerEditorSettings editorSettings)
        {
            if (!editorSettings.IsWatchedPath(assetPath))
            {
                return false;
            }

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return false;
            }

            var folderConfig = editorSettings.GetWatchedFolderFor(assetPath);
            if (folderConfig == null)
            {
                return false;
            }

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            SessionState.SetString(GetGuidSessionKey(assetPath), guid);

            var address = editorSettings.GenerateAddress(assetPath, folderConfig);
            if (string.IsNullOrWhiteSpace(address))
            {
                Debug.LogError($"[AddressableAutoMarker] 주소를 생성할 수 없습니다: {assetPath}");
                return false;
            }

            var duplicate = FindEntryByAddress(settings, address, guid);
            if (duplicate != null)
            {
                Debug.LogError(
                    $"[AddressableAutoMarker] 중복 주소 '{address}'을(를) 사용할 수 없습니다: " +
                    $"{duplicate.AssetPath}, {assetPath}");
                return false;
            }

            var group = FindOrCreateGroup(settings, folderConfig.groupName);
            var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
            if (entry == null)
            {
                return false;
            }

            entry.address = address;

            if (folderConfig.labels != null)
            foreach (var label in folderConfig.labels)
            {
                if (string.IsNullOrEmpty(label))
                {
                    continue;
                }

                var trimmedLabel = label.Trim();
                settings.AddLabel(trimmedLabel);
                entry.SetLabel(trimmedLabel, true, false);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, postEvent: true);
            Debug.Log($"[AddressableAutoMarker] Marked as Addressable: {assetPath} -> {entry.address}");
            return true;
        }

        private static void RemoveAddressableEntry(
            string guid,
            string assetPath,
            AddressableAssetSettings settings)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                return;
            }

            settings.RemoveAssetEntry(guid);
            SessionState.EraseString(GetGuidSessionKey(assetPath));
            Debug.Log($"[AddressableAutoMarker] Removed Addressable entry: {assetPath}");
        }

        private static AddressableAssetGroup FindOrCreateGroup(
            AddressableAssetSettings settings,
            string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                return settings.DefaultGroup;

            var group = settings.FindGroup(groupName);
            if (group != null)
            {
                return group;
            }

            group = settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas);
            Debug.Log($"[AddressableAutoMarker] Created Addressable group: {groupName}");
            return group;
        }

        private static AddressableAssetEntry FindEntryByAddress(
            AddressableAssetSettings settings,
            string address,
            string excludedGuid)
        {
            foreach (var group in settings.groups)
            {
                if (group == null) continue;
                foreach (var candidate in group.entries)
                {
                    if (candidate.guid != excludedGuid && candidate.address == address)
                        return candidate;
                }
            }

            return null;
        }

        private static string GetKnownGuid(string assetPath)
            => SessionState.GetString(GetGuidSessionKey(assetPath), string.Empty);

        private static string GetGuidSessionKey(string assetPath)
            => GuidSessionPrefix + assetPath;
    }
}
#endif
