using System.IO;
using UnityEditor;
using UnityEngine;

namespace AchEngine.Localization.Editor
{
    /// <summary>
    /// 에디터 유틸리티 함수 모음
    /// </summary>
    public static class LocalizationEditorUtility
    {
        private const string UpmPackagePath = "Packages/com.engine.achieve";

        /// <summary>
        /// 패키지 루트 경로를 반환. UPM 패키지이면 Packages/ 경로, 로컬이면 Assets/ 하위 경로.
        /// </summary>
        public static string GetPackageRootPath()
        {
            // UPM 설치 확인
            string packageJsonPath = Path.Combine(UpmPackagePath, "package.json");
            if (File.Exists(Path.GetFullPath(packageJsonPath)))
                return UpmPackagePath;

            // 로컬 설치 — asmdef 위치를 기준으로 패키지 루트를 역추적
            string[] guids = AssetDatabase.FindAssets("AchEngine.Localization.Editor t:DefaultAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("AchEngine.Localization.Editor.asmdef"))
                    continue;

                // path 예: {root}/Editor/Localization/AchEngine.Localization.Editor.asmdef
                int idx = path.IndexOf("/Editor/Localization/");
                if (idx >= 0)
                    return path.Substring(0, idx);
            }

            return UpmPackagePath;
        }

        /// <summary>
        /// UXML 파일 경로를 반환
        /// </summary>
        public static string GetUXMLPath(string fileName)
        {
            return $"{GetPackageRootPath()}/Editor/Localization/UXML/{fileName}";
        }

        /// <summary>
        /// USS 파일 경로를 반환
        /// </summary>
        public static string GetUSSPath(string fileName)
        {
            return $"{GetPackageRootPath()}/Editor/Localization/USS/{fileName}";
        }

        /// <summary>
        /// LocalizationSettings 에셋을 찾거나 생성
        /// </summary>
        public static LocalizationSettings GetOrCreateSettings()
        {
            var settings = LocalizationSettings.Instance;

            // Resources에서 로드 시도
            if (settings != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(settings)))
                return settings;

            // 기존 에셋 검색
            string[] guids = AssetDatabase.FindAssets("t:LocalizationSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(path);
                if (settings != null)
                {
                    LocalizationSettings.SetInstance(settings);
                    return settings;
                }
            }

            // 새로 생성
            settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            EnsureDirectoryExists("Assets/Resources");
            AssetDatabase.CreateAsset(settings, "Assets/Resources/LocalizationSettings.asset");
            AssetDatabase.SaveAssets();
            LocalizationSettings.SetInstance(settings);
            return settings;
        }

        /// <summary>
        /// LocaleDatabase 에셋 생성
        /// </summary>
        public static LocaleDatabase CreateDatabase(string path = "Assets/Resources/LocaleDatabase.asset")
        {
            var database = ScriptableObject.CreateInstance<LocaleDatabase>();
            EnsureDirectoryExists(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(database, path);
            AssetDatabase.SaveAssets();
            return database;
        }

        /// <summary>
        /// JSON TextAsset 파일 생성
        /// </summary>
        public static TextAsset CreateLocaleJsonAsset(string localeCode, string directory = "Assets/Resources/Locales")
        {
            EnsureDirectoryExists(directory);
            string filePath = $"{directory}/{localeCode}.json";

            File.WriteAllText(
                Path.GetFullPath(filePath),
                "{\n}"
            );

            AssetDatabase.ImportAsset(filePath);
            return AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
        }

        /// <summary>
        /// JSON TextAsset 파일에 데이터 저장
        /// </summary>
        public static void SaveLocaleJson(TextAsset textAsset, System.Collections.Generic.Dictionary<string, string> data)
        {
            if (textAsset == null) return;

            string assetPath = AssetDatabase.GetAssetPath(textAsset);
            if (string.IsNullOrEmpty(assetPath)) return;

            string json = SimpleJsonParser.Serialize(data);
            string fullPath = Path.GetFullPath(assetPath);
            File.WriteAllText(fullPath, json);
            AssetDatabase.ImportAsset(assetPath);
        }

        /// <summary>
        /// 디렉토리가 없으면 생성
        /// </summary>
        public static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            string fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 누락된 번역 수 계산
        /// </summary>
        public static int CountMissingTranslations(LocaleDatabase database)
        {
            if (database == null) return 0;

            var allKeys = database.GetAllKeys();
            var localeCodes = database.GetAllLocaleCodes();
            int missing = 0;

            foreach (var key in allKeys)
            {
                foreach (var code in localeCodes)
                {
                    if (!database.TryGetValue(code, key, out var value) || string.IsNullOrEmpty(value))
                        missing++;
                }
            }

            return missing;
        }
    }
}
