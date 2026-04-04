using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.Localization
{
    /// <summary>
    /// Unity SystemLanguage를 ISO 639-1 locale 코드로 매핑
    /// </summary>
    internal static class SystemLanguageMapper
    {
        private static readonly Dictionary<SystemLanguage, string> Map = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.Afrikaans, "af" },
            { SystemLanguage.Arabic, "ar" },
            { SystemLanguage.Basque, "eu" },
            { SystemLanguage.Belarusian, "be" },
            { SystemLanguage.Bulgarian, "bg" },
            { SystemLanguage.Catalan, "ca" },
            { SystemLanguage.Chinese, "zh" },
            { SystemLanguage.ChineseSimplified, "zh-CN" },
            { SystemLanguage.ChineseTraditional, "zh-TW" },
            { SystemLanguage.Czech, "cs" },
            { SystemLanguage.Danish, "da" },
            { SystemLanguage.Dutch, "nl" },
            { SystemLanguage.English, "en" },
            { SystemLanguage.Estonian, "et" },
            { SystemLanguage.Faroese, "fo" },
            { SystemLanguage.Finnish, "fi" },
            { SystemLanguage.French, "fr" },
            { SystemLanguage.German, "de" },
            { SystemLanguage.Greek, "el" },
            { SystemLanguage.Hebrew, "he" },
            { SystemLanguage.Hungarian, "hu" },
            { SystemLanguage.Icelandic, "is" },
            { SystemLanguage.Indonesian, "id" },
            { SystemLanguage.Italian, "it" },
            { SystemLanguage.Japanese, "ja" },
            { SystemLanguage.Korean, "ko" },
            { SystemLanguage.Latvian, "lv" },
            { SystemLanguage.Lithuanian, "lt" },
            { SystemLanguage.Norwegian, "no" },
            { SystemLanguage.Polish, "pl" },
            { SystemLanguage.Portuguese, "pt" },
            { SystemLanguage.Romanian, "ro" },
            { SystemLanguage.Russian, "ru" },
            { SystemLanguage.SerboCroatian, "sr" },
            { SystemLanguage.Slovak, "sk" },
            { SystemLanguage.Slovenian, "sl" },
            { SystemLanguage.Spanish, "es" },
            { SystemLanguage.Swedish, "sv" },
            { SystemLanguage.Thai, "th" },
            { SystemLanguage.Turkish, "tr" },
            { SystemLanguage.Ukrainian, "uk" },
            { SystemLanguage.Vietnamese, "vi" },
        };

        /// <summary>
        /// SystemLanguage를 locale 코드로 변환. 매핑이 없으면 null 반환.
        /// </summary>
        public static string GetLocaleCode(SystemLanguage language)
        {
            return Map.TryGetValue(language, out var code) ? code : null;
        }
    }
}
