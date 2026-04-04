using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AchEngine.Assets.Editor
{
    [FilePath("ProjectSettings/AddressablesManagerSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AddressableManagerEditorSettings : ScriptableSingleton<AddressableManagerEditorSettings>
    {
        [Header("감시 폴더")]
        public List<WatchedFolderConfig> watchedFolders = new();

        [Header("빌드 설정")]
        public bool autoBuildBeforePlayerBuild = true;
        public bool enforceUseExistingBuild = true;

        [Header("원격 설정")]
        public CloudProvider cloudProvider = CloudProvider.None;
        public string bucketName;
        public string bucketRegion;
        public string remoteCatalogUrl;
        public string remoteBundleUrl;

        public void SaveSettings()
        {
            Save(true);
        }

        public bool IsWatchedPath(string assetPath)
        {
            foreach (var folder in watchedFolders)
            {
                if (string.IsNullOrEmpty(folder.folderPath))
                    continue;

                var normalizedFolder = folder.folderPath.TrimEnd('/') + "/";
                if (assetPath.StartsWith(normalizedFolder))
                {
                    if (folder.recursive)
                        return true;

                    var relativePath = assetPath.Substring(normalizedFolder.Length);
                    if (!relativePath.Contains("/"))
                        return true;
                }
            }
            return false;
        }

        public WatchedFolderConfig GetWatchedFolderFor(string assetPath)
        {
            WatchedFolderConfig bestMatch = null;
            int bestLength = 0;

            foreach (var folder in watchedFolders)
            {
                if (string.IsNullOrEmpty(folder.folderPath))
                    continue;

                var normalizedFolder = folder.folderPath.TrimEnd('/') + "/";
                if (assetPath.StartsWith(normalizedFolder) && normalizedFolder.Length > bestLength)
                {
                    if (folder.recursive || !assetPath.Substring(normalizedFolder.Length).Contains("/"))
                    {
                        bestMatch = folder;
                        bestLength = normalizedFolder.Length;
                    }
                }
            }

            return bestMatch;
        }

        public string GenerateAddress(string assetPath, WatchedFolderConfig config)
        {
            switch (config.namingMode)
            {
                case AddressNamingMode.FilenameOnly:
                    return Path.GetFileNameWithoutExtension(assetPath);

                case AddressNamingMode.RelativeToFolder:
                    var normalizedFolder = config.folderPath.TrimEnd('/') + "/";
                    var relative = assetPath.Substring(normalizedFolder.Length);
                    return Path.ChangeExtension(relative, null);

                case AddressNamingMode.FullPath:
                default:
                    return Path.ChangeExtension(assetPath, null);
            }
        }
    }
}
