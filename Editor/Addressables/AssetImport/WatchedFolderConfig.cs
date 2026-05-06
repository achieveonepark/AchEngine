#if ACHENGINE_ADDRESSABLES
using System;
using System.Collections.Generic;

namespace AchEngine.Assets.Editor
{
    public enum AddressNamingMode
    {
        FullPath,
        FilenameOnly,
        RelativeToFolder
    }

    [Serializable]
    public class WatchedFolderConfig
    {
        public string folderPath = "Assets/";
        public string groupName = "Default Local Group";
        public AddressNamingMode namingMode = AddressNamingMode.RelativeToFolder;
        public List<string> labels = new();
        public bool recursive = true;
    }
}
#endif
