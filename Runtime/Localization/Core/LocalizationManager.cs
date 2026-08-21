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
        public static IReadOnlyList<Locale> AvailableLocales
            => _availableLocales ?? (IReadOnlyList<Locale>)Array.Empty<Locale>();

        /// <summary>locale 변경 시 발생하는 이벤트</summary>
        public static event Action<LocaleChangedEventArgs> LocaleChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain() => Reset();

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
            IsInitialized = false;
            CurrentLocale = default;
            FallbackLocale = default;
            _database = null;
            _currentData = null;
            _fallbackData = null;
            _availableLocales = new List<Locale>();

            if (settings == null)
            {
                Debug.LogWarning("[Localization] LocalizationSettings를 찾을 수 없습니다. Resources 폴더에 'LocalizationSettings'를 생성하세요.");
                return;
            }

            var database = settings.database;
            if (database == null)
            {
                Debug.LogWarning("[Localization] LocaleDatabase가 설정되지 않았습니다.");
                return;
            }

            try
            {
                database.InvalidateCache();
                database.ParseJsonAssets();

                var availableLocales = database.GetAllLocales();
                if (availableLocales.Count == 0)
                {
                    Debug.LogWarning("[Localization] 사용할 수 있는 locale이 없습니다.");
                    return;
                }

                string fallbackCode = database.HasLocale(settings.fallbackLocaleCode)
                    ? settings.fallbackLocaleCode
                    : availableLocales[0].Code;
                var fallbackData = database.GetLocaleData(fallbackCode);

                if (fallbackData == null)
                {
                    foreach (var locale in availableLocales)
                    {
                        fallbackData = database.GetLocaleData(locale.Code);
                        if (fallbackData == null) continue;
                        fallbackCode = locale.Code;
                        break;
                    }
                }

                if (fallbackData == null)
                {
                    Debug.LogWarning("[Localization] 번역 데이터가 할당된 locale이 없습니다.");
                    return;
                }

                string targetCode = database.HasLocale(settings.defaultLocaleCode)
                    ? settings.defaultLocaleCode
                    : fallbackCode;

                if (settings.autoDetectSystemLanguage)
                {
                    string systemCode = SystemLanguageMapper.GetLocaleCode(Application.systemLanguage);
                    if (database.HasLocale(systemCode))
                        targetCode = systemCode;
                }

                string savedLocale = PlayerPrefs.GetString("achieve_localization_locale", null);
                if (database.HasLocale(savedLocale))
                    targetCode = savedLocale;

                var currentData = database.GetLocaleData(targetCode);
                if (currentData == null)
                {
                    Debug.LogWarning($"[Localization] locale '{targetCode}'의 데이터가 없어 폴백을 사용합니다.");
                    targetCode = fallbackCode;
                    currentData = fallbackData;
                }

                _database = database;
                _availableLocales = availableLocales;
                FallbackLocale = FindLocale(availableLocales, fallbackCode);
                _fallbackData = fallbackData;
                CurrentLocale = FindLocale(availableLocales, targetCode);
                _currentData = currentData;
                IsInitialized = true;
            }
            catch (Exception e)
            {
                IsInitialized = false;
                Debug.LogError($"[Localization] 초기화에 실패했습니다: {e.Message}");
                Debug.LogException(e);
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

            if (string.IsNullOrWhiteSpace(localeCode))
            {
                Debug.LogWarning("[Localization] locale 코드는 비어 있을 수 없습니다.");
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
            CurrentLocale = FindLocale(_availableLocales, localeCode);
            _currentData = newData;

            // 사용자 선택 저장
            PlayerPrefs.SetString("achieve_localization_locale", CurrentLocale.Code);
            PlayerPrefs.Save();

            InvokeLocaleChangedSafely(new LocaleChangedEventArgs(previous, CurrentLocale));
        }

        /// <summary>
        /// locale 전환
        /// </summary>
        public static void SetLocale(Locale locale)
        {
            if (string.IsNullOrWhiteSpace(locale.Code))
            {
                Debug.LogWarning("[Localization] locale 코드는 비어 있을 수 없습니다.");
                return;
            }
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
            _database = null;
            _currentData = null;
            _fallbackData = null;
            _availableLocales = new List<Locale>();
            LocaleChanged = null;
        }

        private static Locale FindLocale(IReadOnlyList<Locale> locales, string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return default;

            if (locales != null)
            {
                foreach (var locale in locales)
                {
                    if (string.Equals(locale.Code, code, StringComparison.OrdinalIgnoreCase))
                        return locale;
                }
            }

            return new Locale(code, code);
        }

        private static void InvokeLocaleChangedSafely(LocaleChangedEventArgs args)
        {
            var handlers = LocaleChanged;
            if (handlers == null) return;

            foreach (Action<LocaleChangedEventArgs> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(args);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Localization] LocaleChanged 구독자 실행 중 예외가 발생했습니다.");
                    Debug.LogException(e);
                }
            }
        }
    }
}
