using System;
using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.Localization
{
    /// <summary>
    /// Localization 시스템의 핵심 API.
    /// Static facade 패턴으로 어디서든 접근 가능.
    /// </summary>
    public static class LocalizationManager
    {
        private static LocalizationSettings _settings;
        private static LocaleDatabase _database;
        private static Dictionary<string, string> _currentData;
        private static Dictionary<string, string> _fallbackData;
        private static List<Locale> _availableLocales;

        /// <summary>초기화 완료 여부</summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>현재 선택된 locale</summary>
        public static Locale CurrentLocale { get; private set; }

        /// <summary>폴백 locale</summary>
        public static Locale FallbackLocale { get; private set; }

        /// <summary>사용 가능한 모든 locale 목록</summary>
        public static IReadOnlyList<Locale> AvailableLocales => _availableLocales;

        /// <summary>locale 변경 시 발생하는 이벤트</summary>
        public static event Action<LocaleChangedEventArgs> LocaleChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            var settings = LocalizationSettings.Instance;
            if (settings != null && settings.autoInitialize && settings.database != null)
            {
                Initialize(settings);
            }
        }

        /// <summary>
        /// 기본 설정으로 초기화
        /// </summary>
        public static void Initialize()
        {
            Initialize(LocalizationSettings.Instance);
        }

        /// <summary>
        /// 지정된 설정으로 초기화
        /// </summary>
        public static void Initialize(LocalizationSettings settings)
        {
            if (settings == null)
            {
                Debug.LogWarning("[Localization] LocalizationSettings를 찾을 수 없습니다. Resources 폴더에 'LocalizationSettings'를 생성하세요.");
                return;
            }

            _settings = settings;
            _database = settings.database;

            if (_database == null)
            {
                Debug.LogWarning("[Localization] LocaleDatabase가 설정되지 않았습니다.");
                return;
            }

            _database.InvalidateCache();
            _database.ParseJsonAssets();

            _availableLocales = _database.GetAllLocales();

            // 폴백 locale 설정
            FallbackLocale = FindLocale(settings.fallbackLocaleCode);
            _fallbackData = _database.GetLocaleData(settings.fallbackLocaleCode);

            // 초기 locale 결정
            string targetCode = settings.defaultLocaleCode;

            if (settings.autoDetectSystemLanguage)
            {
                string systemCode = SystemLanguageMapper.GetLocaleCode(Application.systemLanguage);
                if (systemCode != null && _database.HasLocale(systemCode))
                {
                    targetCode = systemCode;
                }
            }

            // PlayerPrefs에 저장된 사용자 선택 우선
            string savedLocale = PlayerPrefs.GetString("achieve_localization_locale", null);
            if (!string.IsNullOrEmpty(savedLocale) && _database.HasLocale(savedLocale))
            {
                targetCode = savedLocale;
            }

            IsInitialized = true;

            // 이벤트 없이 초기 locale 설정
            CurrentLocale = FindLocale(targetCode);
            _currentData = _database.GetLocaleData(targetCode);

            if (_currentData == null)
            {
                Debug.LogWarning($"[Localization] locale '{targetCode}'의 데이터를 로드할 수 없습니다. 폴백 사용.");
                CurrentLocale = FallbackLocale;
                _currentData = _fallbackData;
            }
        }

        /// <summary>
        /// locale 전환
        /// </summary>
        public static void SetLocale(string localeCode)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Localization] 초기화되지 않았습니다. Initialize()를 먼저 호출하세요.");
                return;
            }

            if (string.Equals(CurrentLocale.Code, localeCode, StringComparison.OrdinalIgnoreCase))
                return;

            var newData = _database.GetLocaleData(localeCode);
            if (newData == null)
            {
                Debug.LogWarning($"[Localization] locale '{localeCode}'을 찾을 수 없습니다.");
                return;
            }

            var previous = CurrentLocale;
            CurrentLocale = FindLocale(localeCode);
            _currentData = newData;

            // 사용자 선택 저장
            PlayerPrefs.SetString("achieve_localization_locale", localeCode);
            PlayerPrefs.Save();

            LocaleChanged?.Invoke(new LocaleChangedEventArgs(previous, CurrentLocale));
        }

        /// <summary>
        /// locale 전환
        /// </summary>
        public static void SetLocale(Locale locale)
        {
            SetLocale(locale.Code);
        }

        /// <summary>
        /// 키에 해당하는 번역 문자열 반환
        /// </summary>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            if (!IsInitialized)
                return key;

            // 현재 locale에서 검색
            if (_currentData != null && _currentData.TryGetValue(key, out var value))
                return value;

            // 폴백 locale에서 검색
            if (_fallbackData != null && _fallbackData.TryGetValue(key, out var fallbackValue))
                return fallbackValue;

            // 찾지 못함
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return $"[missing:{key}]";
#else
            return key;
#endif
        }

        /// <summary>
        /// 위치 기반 인자를 사용하여 번역 문자열 반환.
        /// 예: Get("dialog.welcome", playerName, count)
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            string template = Get(key);
            return StringFormatter.Format(template, args);
        }

        /// <summary>
        /// 이름 기반 인자를 사용하여 번역 문자열 반환.
        /// 예: Get("dialog.welcome", new Dictionary{{"playerName", "홍길동"}, {"count", 5}})
        /// </summary>
        public static string Get(string key, Dictionary<string, object> namedArgs)
        {
            string template = Get(key);
            return StringFormatter.Format(template, namedArgs);
        }

        /// <summary>
        /// 키에 해당하는 번역 문자열을 시도적으로 반환
        /// </summary>
        public static bool TryGet(string key, out string value)
        {
            value = null;

            if (string.IsNullOrEmpty(key) || !IsInitialized)
                return false;

            if (_currentData != null && _currentData.TryGetValue(key, out value))
                return true;

            if (_fallbackData != null && _fallbackData.TryGetValue(key, out value))
                return true;

            return false;
        }

        /// <summary>
        /// 키 존재 여부 확인
        /// </summary>
        public static bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key) || !IsInitialized)
                return false;

            if (_currentData != null && _currentData.ContainsKey(key))
                return true;

            if (_fallbackData != null && _fallbackData.ContainsKey(key))
                return true;

            return false;
        }

        /// <summary>
        /// locale 존재 여부 확인
        /// </summary>
        public static bool HasLocale(string localeCode)
        {
            return _database != null && _database.HasLocale(localeCode);
        }

        /// <summary>
        /// 시스템 리셋 (테스트용)
        /// </summary>
        internal static void Reset()
        {
            IsInitialized = false;
            CurrentLocale = default;
            FallbackLocale = default;
            _settings = null;
            _database = null;
            _currentData = null;
            _fallbackData = null;
            _availableLocales = null;
            LocaleChanged = null;
        }

        private static Locale FindLocale(string code)
        {
            if (_availableLocales == null) return new Locale(code, code);

            foreach (var locale in _availableLocales)
            {
                if (string.Equals(locale.Code, code, StringComparison.OrdinalIgnoreCase))
                    return locale;
            }

            return new Locale(code, code);
        }
    }
}
