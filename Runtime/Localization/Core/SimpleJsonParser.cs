using System;
using System.Collections.Generic;
using System.Text;

namespace AchEngine.Localization
{
    /// <summary>
    /// 외부 의존성 없이 flat JSON (string→string) 파싱/직렬화를 처리하는 내부 유틸리티
    /// </summary>
    public static class SimpleJsonParser
    {
        /// <summary>
        /// flat JSON 문자열을 Dictionary로 파싱
        /// </summary>
        public static Dictionary<string, string> Parse(string json)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(json))
                return result;

            int i = 0;
            SkipWhitespace(json, ref i);

            if (i >= json.Length || json[i] != '{')
                return result;
            i++; // skip '{'

            while (i < json.Length)
            {
                SkipWhitespace(json, ref i);

                if (i >= json.Length || json[i] == '}')
                    break;

                if (json[i] == ',')
                {
                    i++;
                    continue;
                }

                string key = ParseString(json, ref i);
                SkipWhitespace(json, ref i);

                if (i >= json.Length || json[i] != ':')
                    break;
                i++; // skip ':'

                SkipWhitespace(json, ref i);
                string value = ParseString(json, ref i);

                result[key] = value;
            }

            return result;
        }

        /// <summary>
        /// Dictionary를 flat JSON 문자열로 직렬화
        /// </summary>
        public static string Serialize(Dictionary<string, string> data, bool prettyPrint = true)
        {
            if (data == null || data.Count == 0)
                return "{}";

            var sb = new StringBuilder();
            sb.Append('{');

            if (prettyPrint)
                sb.AppendLine();

            int count = 0;
            // 키 정렬로 일관된 출력 보장
            var sortedKeys = new List<string>(data.Keys);
            sortedKeys.Sort(StringComparer.Ordinal);

            foreach (var key in sortedKeys)
            {
                if (count > 0)
                {
                    sb.Append(',');
                    if (prettyPrint)
                        sb.AppendLine();
                }

                string escapedKey = EscapeString(key);
                string escapedValue = EscapeString(data[key] ?? "");

                if (prettyPrint)
                    sb.Append($"  \"{escapedKey}\": \"{escapedValue}\"");
                else
                    sb.Append($"\"{escapedKey}\":\"{escapedValue}\"");

                count++;
            }

            if (prettyPrint)
            {
                sb.AppendLine();
                sb.Append('}');
            }
            else
            {
                sb.Append('}');
            }

            return sb.ToString();
        }

        private static string ParseString(string json, ref int i)
        {
            if (i >= json.Length || json[i] != '"')
                return string.Empty;

            i++; // skip opening quote
            var sb = new StringBuilder();

            while (i < json.Length)
            {
                char c = json[i];

                if (c == '\\' && i + 1 < json.Length)
                {
                    i++;
                    char escaped = json[i];
                    switch (escaped)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (i + 4 < json.Length)
                            {
                                string hex = json.Substring(i + 1, 4);
                                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                                {
                                    sb.Append((char)code);
                                    i += 4;
                                }
                            }
                            break;
                        default:
                            sb.Append('\\');
                            sb.Append(escaped);
                            break;
                    }
                }
                else if (c == '"')
                {
                    i++; // skip closing quote
                    return sb.ToString();
                }
                else
                {
                    sb.Append(c);
                }

                i++;
            }

            return sb.ToString();
        }

        private static string EscapeString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        private static void SkipWhitespace(string json, ref int i)
        {
            while (i < json.Length && char.IsWhiteSpace(json[i]))
                i++;
        }
    }
}
