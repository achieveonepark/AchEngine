using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AchEngine.Editor.Table
{
    public class TableLoaderSettings : ScriptableObject
    {
        private const string SettingsPath = "Assets/TableLoaderSettings.asset";

        [Header("Google Spreadsheet")]
        public string spreadsheetId = "";

        [Header("Sheets")]
        public List<SheetInfo> sheets = new();

        [Header("Output Paths")]
        [Tooltip("Folder used for downloaded CSV files.")]
        public string csvOutputPath = "Assets/TableLoader/CSV";

        [Tooltip("Folder used for generated C# files.")]
        public string codeOutputPath = "Assets/TableLoader/Generated";

        [Tooltip("Resources output path for baked table data assets.")]
        public string binaryOutputPath = "Assets/Resources/Tables";

        [Header("Automation")]
        public bool autoGenerateOnDownload = true;
        public bool autoBakeOnGenerate = true;

        public string GetSpreadsheetUrl()
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}";
        }

        public string GetCsvDownloadUrl(SheetInfo sheet)
        {
            return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv&gid={sheet.sheetGid}";
        }

        public static TableLoaderSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TableLoaderSettings>(SettingsPath);
            if (settings != null)
                return settings;

            settings = CreateInstance<TableLoaderSettings>();

            var dir = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreate());
        }
    }
}