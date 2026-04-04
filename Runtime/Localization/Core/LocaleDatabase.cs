using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AchEngine.Localization
{
    /// <summary>
    /// 모든 locale 데이터를 보유하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "LocaleDatabase", menuName = "Achieve/Localization/Locale Database")]
    public class LocaleDatabase : ScriptableObject
    {
        [SerializeField] private List<LocaleEntry> entries = new List<LocaleEntry>();

        [NonSerialized] private Dictionary<string, Dictionary<string, string>> _cache;
        [NonSerialized] private bool _parsed;

        public IReadOnlyList<LocaleEntry> Entries => entries;

        /// <summary>
        /// 지정된 locale의 번역 데이터를 반환
        /// </summary>
        public Dictionary<string, string> GetLocaleData(string localeCode)
        {
            EnsureParsed();

            if (_cache != null && _cache.TryGetValue(localeCode, out var data))
                return data;

            return null;
        }

        /// <summary>
        /// 모든 locale에 존재하는 키의 합집합 반환
        /// </summary>
        public List<string> GetAllKeys()
        {
            EnsureParsed();

            var keys = new HashSet<string>();
            if (_cache != null)
            {
                foreach (var localeData in _cache.Values)
                {
                    foreach (var key in localeData.Keys)
                        keys.Add(key);
                }
            }

            var sorted = keys.ToList();
            sorted.Sort(StringComparer.Ordinal);
            return sorted;
        }

        /// <summary>
        /// 등록된 모든 Locale 목록 반환
        /// </summary>
        public List<Locale> GetAllLocales()
        {
            return entries.Select(e => e.locale).ToList();
        }

        /// <summary>
        /// locale code 목록 반환
        /// </summary>
        public List<string> GetAllLocaleCodes()
        {
            return entries.Select(e => e.locale.Code).ToList();
        }

        /// <summary>
        /// 특정 locale에서 특정 키의 값을 가져옴
        /// </summary>
        public bool TryGetValue(string localeCode, string key, out string value)
        {
            EnsureParsed();
            value = null;

            if (_cache != null && _cache.TryGetValue(localeCode, out var data))
                return data.TryGetValue(key, out value);

            return false;
        }

        /// <summary>
        /// 캐시에 값 설정 (에디터에서 사용)
        /// </summary>
        public void SetEntry(string localeCode, string key, string value)
        {
            EnsureParsed();

            if (_cache == null)
                _cache = new Dictionary<string, Dictionary<string, string>>();

            if (!_cache.TryGetValue(localeCode, out var data))
            {
                data = new Dictionary<string, string>();
                _cache[localeCode] = data;
            }

            data[key] = value;
        }

        /// <summary>
        /// 캐시에서 키 삭제
        /// </summary>
        public void RemoveKey(string key)
        {
            EnsureParsed();

            if (_cache == null) return;

            foreach (var data in _cache.Values)
                data.Remove(key);
        }

        /// <summary>
        /// locale entry 추가
        /// </summary>
        public void AddLocaleEntry(Locale locale, TextAsset jsonAsset)
        {
            entries.Add(new LocaleEntry { locale = locale, jsonAsset = jsonAsset });
            InvalidateCache();
        }

        /// <summary>
        /// locale entry 제거
        /// </summary>
        public void RemoveLocaleEntry(string localeCode)
        {
            entries.RemoveAll(e => string.Equals(e.locale.Code, localeCode, StringComparison.OrdinalIgnoreCase));
            InvalidateCache();
        }

        /// <summary>
        /// locale 존재 여부 확인
        /// </summary>
        public bool HasLocale(string localeCode)
        {
            return entries.Any(e => string.Equals(e.locale.Code, localeCode, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 캐시 무효화
        /// </summary>
        public void InvalidateCache()
        {
            _cache = null;
            _parsed = false;
        }

        private void EnsureParsed()
        {
            if (_parsed) return;
            ParseJsonAssets();
        }

        /// <summary>
        /// TextAsset에서 JSON을 파싱하여 캐시 구축
        /// </summary>
        public void ParseJsonAssets()
        {
            _cache = new Dictionary<string, Dictionary<string, string>>();

            foreach (var entry in entries)
            {
                if (entry.jsonAsset == null) continue;

                var data = SimpleJsonParser.Parse(entry.jsonAsset.text);
                _cache[entry.locale.Code] = data;
            }

            _parsed = true;
        }
    }

    /// <summary>
    /// locale과 해당 JSON TextAsset의 쌍
    /// </summary>
    [Serializable]
    public class LocaleEntry
    {
        public Locale locale;
        public TextAsset jsonAsset;
    }
}
