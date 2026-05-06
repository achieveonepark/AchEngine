#if ACHENGINE_ADDRESSABLES
using NUnit.Framework;

namespace AchEngine.Assets.Editor.Tests
{
    public class AddressableAutoMarkerTests
    {
        [Test]
        public void EditorSettings_IsWatchedPath_ReturnsTrueForWatchedFolder()
        {
            var settings = AddressableManagerEditorSettings.instance;
            settings.watchedFolders.Clear();
            settings.watchedFolders.Add(new WatchedFolderConfig
            {
                folderPath = "Assets/Addressables",
                recursive = true
            });

            Assert.IsTrue(settings.IsWatchedPath("Assets/Addressables/Prefab.prefab"));
            Assert.IsTrue(settings.IsWatchedPath("Assets/Addressables/Sub/Prefab.prefab"));
            Assert.IsFalse(settings.IsWatchedPath("Assets/Other/Prefab.prefab"));

            settings.watchedFolders.Clear();
        }

        [Test]
        public void EditorSettings_IsWatchedPath_NonRecursive()
        {
            var settings = AddressableManagerEditorSettings.instance;
            settings.watchedFolders.Clear();
            settings.watchedFolders.Add(new WatchedFolderConfig
            {
                folderPath = "Assets/Addressables",
                recursive = false
            });

            Assert.IsTrue(settings.IsWatchedPath("Assets/Addressables/Prefab.prefab"));
            Assert.IsFalse(settings.IsWatchedPath("Assets/Addressables/Sub/Prefab.prefab"));

            settings.watchedFolders.Clear();
        }

        [Test]
        public void EditorSettings_GenerateAddress_FilenameOnly()
        {
            var settings = AddressableManagerEditorSettings.instance;
            var config = new WatchedFolderConfig
            {
                folderPath = "Assets/Addressables",
                namingMode = AddressNamingMode.FilenameOnly
            };

            var address = settings.GenerateAddress("Assets/Addressables/Prefabs/Player.prefab", config);
            Assert.AreEqual("Player", address);
        }

        [Test]
        public void EditorSettings_GenerateAddress_RelativeToFolder()
        {
            var settings = AddressableManagerEditorSettings.instance;
            var config = new WatchedFolderConfig
            {
                folderPath = "Assets/Addressables",
                namingMode = AddressNamingMode.RelativeToFolder
            };

            var address = settings.GenerateAddress("Assets/Addressables/Prefabs/Player.prefab", config);
            Assert.AreEqual("Prefabs/Player", address);
        }

        [Test]
        public void EditorSettings_GenerateAddress_FullPath()
        {
            var settings = AddressableManagerEditorSettings.instance;
            var config = new WatchedFolderConfig
            {
                folderPath = "Assets/Addressables",
                namingMode = AddressNamingMode.FullPath
            };

            var address = settings.GenerateAddress("Assets/Addressables/Prefabs/Player.prefab", config);
            Assert.AreEqual("Assets/Addressables/Prefabs/Player", address);
        }
    }
}
#endif
