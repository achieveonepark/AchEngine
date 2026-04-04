using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AchEngine.Localization.Editor
{
    /// <summary>
    /// JSON 형식의 번역 데이터 가져오기/내보내기
    /// </summary>
    public static class JSONImporter
    {
        /// <summary>
        /// 단일 JSON 파일을 특정 locale로 가져오기
        /// </summary>
        public static void ImportLocale(string jsonPath, string localeCode, LocaleDatabase database)
        {
            if (database == null || !File.Exists(jsonPath))
                return;

            string json = File.ReadAllText(jsonPath);
            var data = SimpleJsonParser.Parse(json);

            database.InvalidateCache();
            database.ParseJsonAssets();

            foreach (var kvp in data)
                database.SetEntry(localeCode, kvp.Key, kvp.Value);
        }

        /// <summary>
        /// 특정 locale의 데이터를 JSON 파일로 내보내기
        /// </summary>
        public static void ExportLocale(string outputPath, string localeCode, LocaleDatabase database)
        {
            if (database == null) return;

            database.InvalidateCache();
            database.ParseJsonAssets();

            var data = database.GetLocaleData(localeCode);
            if (data == null) return;

            string json = SimpleJsonParser.Serialize(data);
            File.WriteAllText(outputPath, json);
        }

        /// <summary>
        /// 디렉토리 내의 모든 JSON 파일을 가져오기.
        /// 파일명이 locale 코드로 사용됨 (예: en.json → "en")
        /// </summary>
        public static void ImportDirectory(string directoryPath, LocaleDatabase database)
        {
            if (database == null || !Directory.Exists(directoryPath))
                return;

            database.InvalidateCache();
            database.ParseJsonAssets();

            var jsonFiles = Directory.GetFiles(directoryPath, "*.json");
            foreach (var filePath in jsonFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string json = File.ReadAllText(filePath);
                var data = SimpleJsonParser.Parse(json);

                foreach (var kvp in data)
                    database.SetEntry(fileName, kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 모든 locale 데이터를 개별 JSON 파일로 내보내기
        /// </summary>
        public static void ExportAll(string directoryPath, LocaleDatabase database)
        {
            if (database == null) return;

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            database.InvalidateCache();
            database.ParseJsonAssets();

            foreach (var locale in database.GetAllLocales())
            {
                var data = database.GetLocaleData(locale.Code);
                if (data == null) continue;

                string json = SimpleJsonParser.Serialize(data);
                string filePath = Path.Combine(directoryPath, $"{locale.Code}.json");
                File.WriteAllText(filePath, json);
            }
        }
    }
}
