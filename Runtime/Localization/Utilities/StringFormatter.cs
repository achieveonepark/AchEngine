using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AchEngine.Localization
{
    /// <summary>
    /// 문자열 보간 유틸리티. 위치 기반({0}) 및 이름 기반({name}) 인자를 지원.
    /// 포맷 지정자도 사용 가능 (예: {price:C2}, {count:N0})
    /// </summary>
    public static class StringFormatter
    {
        // {name} 또는 {name:format} 패턴 매칭
        private static readonly Regex NamedPattern = new Regex(
            @"\{(\w+)(?::([^}]+))?\}",
            RegexOptions.Compiled
        );

        /// <summary>
        /// 위치 기반 인자로 문자열 포맷팅.
        /// 템플릿의 {0}, {1} 등을 args 배열 값으로 치환.
        /// </summary>
        public static string Format(string template, params object[] args)
        {
            if (string.IsNullOrEmpty(template) || args == null || args.Length == 0)
                return template;

            try
            {
                return string.Format(template, args);
            }
            catch
            {
                // string.Format 실패 시 이름 기반 시도
                return FormatNamed(template, args);
            }
        }

        /// <summary>
        /// 이름 기반 인자로 문자열 포맷팅.
        /// 템플릿의 {playerName}, {count} 등을 딕셔너리 값으로 치환.
        /// </summary>
        public static string Format(string template, Dictionary<string, object> namedArgs)
        {
            if (string.IsNullOrEmpty(template) || namedArgs == null || namedArgs.Count == 0)
                return template;

            return NamedPattern.Replace(template, match =>
            {
                string name = match.Groups[1].Value;
                string format = match.Groups[2].Success ? match.Groups[2].Value : null;

                if (!namedArgs.TryGetValue(name, out var value))
                    return match.Value; // 매칭되는 인자가 없으면 원본 유지

                if (value == null)
                    return string.Empty;

                if (!string.IsNullOrEmpty(format) && value is System.IFormattable formattable)
                    return formattable.ToString(format, System.Globalization.CultureInfo.CurrentCulture);

                return value.ToString();
            });
        }

        /// <summary>
        /// params object[]를 이름 기반으로 처리하는 내부 헬퍼.
        /// 인자를 순서대로 이름 매칭 시도.
        /// </summary>
        private static string FormatNamed(string template, object[] args)
        {
            int argIndex = 0;
            return NamedPattern.Replace(template, match =>
            {
                if (argIndex >= args.Length)
                    return match.Value;

                string format = match.Groups[2].Success ? match.Groups[2].Value : null;
                var value = args[argIndex++];

                if (value == null)
                    return string.Empty;

                if (!string.IsNullOrEmpty(format) && value is System.IFormattable formattable)
                    return formattable.ToString(format, System.Globalization.CultureInfo.CurrentCulture);

                return value.ToString();
            });
        }
    }
}
