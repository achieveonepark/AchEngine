using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AchEngine.Localization;

namespace AchEngine.Localization.Editor
{
    public sealed class LocalizedTMPCharacterSetOptions
    {
        public string LocaleCode;
        /// <summary>
        /// 설정 시 일치하는 prefix로 시작하는 locale만 포함. LocaleCode보다 우선.
        /// 예: new[] { "ko" } → "ko", "ko-KR" 포함
        /// </summary>
        public string[] LocaleCodePrefixes;
        public string ExtraCharacters;
        public IEnumerable<string> AdditionalTexts;
        public bool IncludeCommonAscii = true;
        public bool IncludeKorean;
        public bool IncludeJapanese;
        public bool IncludeLocalizationKeys;
        public bool StripRichTextTags = true;
        public bool StripFormatPlaceholders = true;
    }

    public readonly struct LocalizedTMPCharacterSetResult
    {
        public readonly string CharacterSet;
        public readonly int CharacterCount;
        public readonly int SourceStringCount;

        public LocalizedTMPCharacterSetResult(string characterSet, int characterCount, int sourceStringCount)
        {
            CharacterSet = characterSet;
            CharacterCount = characterCount;
            SourceStringCount = sourceStringCount;
        }
    }

    public static class LocalizedTMPCharacterSetBuilder
    {
        private const string CommonAscii =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" +
            " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~\n\t";

        private static readonly Regex FormatPlaceholderRegex =
            new Regex(@"\{[^{}\r\n]*\}", RegexOptions.Compiled);

        public static LocalizedTMPCharacterSetResult Build(LocaleDatabase database, LocalizedTMPCharacterSetOptions options)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            options ??= new LocalizedTMPCharacterSetOptions();

            var codepoints = new HashSet<int>();
            int sourceStringCount = 0;

            sourceStringCount += AddPresetCharacters(codepoints, options);

            database.InvalidateCache();
            database.ParseJsonAssets();

            if (options.LocaleCodePrefixes != null && options.LocaleCodePrefixes.Length > 0)
            {
                var localeCodes = database.GetAllLocaleCodes();
                for (int i = 0; i < localeCodes.Count; i++)
                {
                    if (MatchesAnyPrefix(localeCodes[i], options.LocaleCodePrefixes))
                        sourceStringCount += AddLocale(database, localeCodes[i], options, codepoints);
                }
            }
            else if (string.IsNullOrWhiteSpace(options.LocaleCode))
            {
                var localeCodes = database.GetAllLocaleCodes();
                for (int i = 0; i < localeCodes.Count; i++)
                    sourceStringCount += AddLocale(database, localeCodes[i], options, codepoints);
            }
            else
            {
                sourceStringCount += AddLocale(database, options.LocaleCode, options, codepoints);
            }

            string characterSet = BuildSortedString(codepoints);
            return new LocalizedTMPCharacterSetResult(characterSet, codepoints.Count, sourceStringCount);
        }

        public static LocalizedTMPCharacterSetResult BuildFallback(LocalizedTMPCharacterSetOptions options)
        {
            options ??= new LocalizedTMPCharacterSetOptions();

            var codepoints = new HashSet<int>();
            int sourceStringCount = AddPresetCharacters(codepoints, options);

            string characterSet = BuildSortedString(codepoints);
            return new LocalizedTMPCharacterSetResult(characterSet, codepoints.Count, sourceStringCount);
        }

        private static int AddPresetCharacters(HashSet<int> codepoints, LocalizedTMPCharacterSetOptions options)
        {
            int sourceStringCount = 0;

            if (options.IncludeCommonAscii)
                AddString(codepoints, CommonAscii);

            if (!string.IsNullOrEmpty(options.ExtraCharacters))
                AddString(codepoints, options.ExtraCharacters);

            if (options.AdditionalTexts != null)
            {
                foreach (string text in options.AdditionalTexts)
                {
                    string value = PrepareAdditionalText(text, options);
                    if (string.IsNullOrEmpty(value))
                        continue;

                    AddString(codepoints, value);
                    sourceStringCount++;
                }
            }

            if (options.IncludeKorean)
                AddKoreanRanges(codepoints);

            if (options.IncludeJapanese)
                AddJapaneseRanges(codepoints);

            return sourceStringCount;
        }

        private static int AddLocale(
            LocaleDatabase database,
            string localeCode,
            LocalizedTMPCharacterSetOptions options,
            HashSet<int> codepoints)
        {
            var localeData = database.GetLocaleData(localeCode);
            if (localeData == null)
                return 0;

            int sourceStringCount = 0;
            foreach (var kvp in localeData)
            {
                if (options.IncludeLocalizationKeys)
                    AddString(codepoints, kvp.Key);

                string value = PrepareText(kvp.Value, options);
                AddString(codepoints, value);
                sourceStringCount++;
            }

            return sourceStringCount;
        }

        private static string PrepareText(string text, LocalizedTMPCharacterSetOptions options)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (options.StripRichTextTags)
                text = StripRichTextTags(text);

            if (options.StripFormatPlaceholders)
                text = FormatPlaceholderRegex.Replace(text, string.Empty);

            return text;
        }

        private static string PrepareAdditionalText(string text, LocalizedTMPCharacterSetOptions options)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return options.StripRichTextTags ? StripRichTextTags(text) : text;
        }

        private static string StripRichTextTags(string text)
        {
            var sb = new StringBuilder(text.Length);
            bool inTag = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (!inTag && c == '<' && LooksLikeRichTextTagStart(text, i))
                {
                    inTag = true;
                    continue;
                }

                if (inTag)
                {
                    if (c == '>')
                        inTag = false;
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        private static bool LooksLikeRichTextTagStart(string text, int index)
        {
            if (index + 1 >= text.Length)
                return false;

            char next = text[index + 1];
            return char.IsLetter(next) || next == '/' || next == '#' || next == '!';
        }

        private static void AddString(HashSet<int> codepoints, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            for (int i = 0; i < text.Length; i++)
            {
                int codepoint;
                char c = text[i];

                if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    codepoint = char.ConvertToUtf32(c, text[i + 1]);
                    i++;
                }
                else
                {
                    codepoint = c;
                }

                if (ShouldIncludeCodepoint(codepoint))
                    codepoints.Add(codepoint);
            }
        }

        private static bool ShouldIncludeCodepoint(int codepoint)
        {
            if (codepoint == '\n' || codepoint == '\t' || codepoint == '\r')
                return true;

            return !char.IsControl((char)Math.Min(codepoint, char.MaxValue));
        }

        private static bool MatchesAnyPrefix(string localeCode, string[] prefixes)
        {
            for (int i = 0; i < prefixes.Length; i++)
            {
                string prefix = prefixes[i];
                if (localeCode.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                    localeCode.StartsWith(prefix + "-", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static void AddKoreanRanges(HashSet<int> codepoints)
        {
            AddRange(codepoints, 0x1100, 0x11FF); // 한글 자모
            AddRange(codepoints, 0x3130, 0x318F); // 한글 호환 자모
            AddRange(codepoints, 0xA960, 0xA97F); // 한글 자모 확장-A
            AddRange(codepoints, 0xAC00, 0xD7A3); // 한글 음절
            AddRange(codepoints, 0xD7B0, 0xD7FF); // 한글 자모 확장-B
        }

        private static void AddJapaneseRanges(HashSet<int> codepoints)
        {
            AddRange(codepoints, 0x3000, 0x303F); // CJK 기호 및 구두점
            AddRange(codepoints, 0x3040, 0x309F); // 히라가나
            AddRange(codepoints, 0x30A0, 0x30FF); // 가타카나
            AddRange(codepoints, 0x4E00, 0x9FFF); // CJK 통합 한자 (상용 한자)
            AddRange(codepoints, 0xFF65, 0xFF9F); // 반각 가타카나
        }

        private static void AddRange(HashSet<int> codepoints, int start, int end)
        {
            for (int codepoint = start; codepoint <= end; codepoint++)
                codepoints.Add(codepoint);
        }

        private static string BuildSortedString(HashSet<int> codepoints)
        {
            var sorted = new List<int>(codepoints);
            sorted.Sort();

            var sb = new StringBuilder(sorted.Count);
            for (int i = 0; i < sorted.Count; i++)
                sb.Append(char.ConvertFromUtf32(sorted[i]));

            return sb.ToString();
        }
    }
}
