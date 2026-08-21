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

        public IReadOnlyList<LocaleEntry> Entries => entries ?? (IReadOnlyList<LocaleEntry>)Array.Empty<LocaleEntry>();

        /// <summary>
        /// 지정된 locale의 번역 데이터를 반환
        /// </summary>
        public Dictionary<string, string> GetLocaleData(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode)) return null;
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
            if (entries == null) return new List<Locale>();
            return entries
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.locale.Code))
                .Select(entry => entry.locale)
                .ToList();
        }

        /// <summary>
        /// locale code 목록 반환
        /// </summary>
        public List<string> GetAllLocaleCodes()
        {
            if (entries == null) return new List<string>();
            return entries
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.locale.Code))
                .Select(entry => entry.locale.Code)
                .ToList();
        }

        /// <summary>
        /// 특정 locale에서 특정 키의 값을 가져옴
        /// </summary>
        public bool TryGetValue(string localeCode, string key, out string value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(localeCode) || string.IsNullOrEmpty(key)) return false;
            EnsureParsed();

            if (_cache != null && _cache.TryGetValue(localeCode, out var data))
                return data.TryGetValue(key, out value);

            return false;
        }

        /// <summary>
        /// 캐시에 값 설정 (에디터에서 사용)
        /// </summary>
        public void SetEntry(string localeCode, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(localeCode))
                throw new ArgumentException("Locale 코드는 비어 있을 수 없습니다.", nameof(localeCode));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("번역 키는 비어 있을 수 없습니다.", nameof(key));
            EnsureParsed();

            if (_cache == null)
                _cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            if (!_cache.TryGetValue(localeCode, out var data))
            {
                data = new Dictionary<string, string>(StringComparer.Ordinal);
                _cache[localeCode] = data;
            }

            data[key] = value;
        }

        /// <summary>
        /// 캐시에서 키 삭제
        /// </summary>
        public void RemoveKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
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
            if (string.IsNullOrWhiteSpace(locale.Code))
                throw new ArgumentException("Locale 코드는 비어 있을 수 없습니다.", nameof(locale));
            if (HasLocale(locale.Code))
                throw new InvalidOperationException($"Locale '{locale.Code}'이(가) 이미 등록되어 있습니다.");

            entries ??= new List<LocaleEntry>();
            entries.Add(new LocaleEntry { locale = locale, jsonAsset = jsonAsset });
            InvalidateCache();
        }

        /// <summary>
        /// locale entry 제거
        /// </summary>
        public void RemoveLocaleEntry(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode) || entries == null) return;
            entries.RemoveAll(entry => entry != null &&
                string.Equals(entry.locale.Code, localeCode, StringComparison.OrdinalIgnoreCase));
            InvalidateCache();
        }

        /// <summary>
        /// locale 존재 여부 확인
        /// </summary>
        public bool HasLocale(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode) || entries == null) return false;
            return entries.Any(entry => entry != null &&
                string.Equals(entry.locale.Code, localeCode, StringComparison.OrdinalIgnoreCase));
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
            _cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            if (entries == null)
            {
                _parsed = true;
                return;
            }

            var localeCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                if (entry == null) continue;
                if (string.IsNullOrWhiteSpace(entry.locale.Code))
                    throw new InvalidOperationException("LocaleDatabase에 코드가 비어 있는 항목이 있습니다.");
                if (!localeCodes.Add(entry.locale.Code))
                    throw new InvalidOperationException(
                        $"LocaleDatabase에 중복 locale 코드 '{entry.locale.Code}'이(가) 있습니다.");
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
