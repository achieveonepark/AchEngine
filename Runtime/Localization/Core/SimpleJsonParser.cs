using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AchEngine.Localization
{
    /// <summary>
    /// 문자열 키와 문자열 값으로 구성된 JSON 객체를 파싱하고 직렬화합니다.
    /// </summary>
    public static class SimpleJsonParser
    {
        /// <summary>JSON 문자열을 Dictionary로 파싱합니다.</summary>
        public static Dictionary<string, string> Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>(StringComparer.Ordinal);

            try
            {
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return parsed == null
                    ? new Dictionary<string, string>(StringComparer.Ordinal)
                    : new Dictionary<string, string>(parsed, StringComparer.Ordinal);
            }
            catch (JsonException e)
            {
                throw new FormatException("로컬라이제이션 JSON 형식이 올바르지 않습니다.", e);
            }
        }

        /// <summary>Dictionary를 키 순서가 일정한 JSON 문자열로 직렬화합니다.</summary>
        public static string Serialize(Dictionary<string, string> data, bool prettyPrint = true)
        {
            if (data == null || data.Count == 0)
                return "{}";

            var sorted = new SortedDictionary<string, string>(data, StringComparer.Ordinal);
            return JsonConvert.SerializeObject(
                sorted,
                prettyPrint ? Formatting.Indented : Formatting.None);
        }
    }
}
