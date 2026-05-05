using System;
using UnityEngine;

namespace AchEngine.Editor.Table
{
    [Serializable]
    public class SheetInfo
    {
        public string sheetName = "";
        public string sheetGid = "0";
        public string className = "";
        public bool enabled = true;

        public string GetClassName()
        {
            if (!string.IsNullOrEmpty(className))
                return className;
            return sheetName.Replace(" ", "");
        }
    }
}
