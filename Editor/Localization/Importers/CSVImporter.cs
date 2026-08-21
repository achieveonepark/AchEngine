using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AchEngine.Localization.Editor
{
    /// <summary>
    /// CSV 형식의 번역 데이터 가져오기/내보내기.
    /// 형식: key,en,ko,ja,...
    /// RFC 4180 호환 (따옴표, 이스케이프, 줄바꿈 포함 값 지원)
    /// </summary>
    public static class CSVImporter
    {
        /// <summary>
        /// CSV 파일에서 번역 데이터를 가져와 LocaleDatabase에 병합
        /// </summary>
        public static void Import(string csvPath, LocaleDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (!File.Exists(csvPath)) throw new FileNotFoundException("CSV 파일을 찾을 수 없습니다.", csvPath);

            string content = File.ReadAllText(csvPath, Encoding.UTF8);
            var rows = ParseCSV(content);

            if (rows.Count < 2)
                throw new InvalidDataException("CSV에는 헤더와 최소 한 개의 데이터 행이 필요합니다.");

            // 헤더에서 locale 코드 추출 (첫 컬럼은 "key")
            var header = rows[0];
            if (header.Count < 2 || !string.Equals(header[0].Trim().TrimStart('\uFEFF'), "key", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("CSV 첫 번째 컬럼 이름은 'key'여야 합니다.");

            var localeCodes = new List<string>();
            var uniqueLocaleCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 1; i < header.Count; i++)
            {
                var localeCode = header[i].Trim();
                if (!database.HasLocale(localeCode))
                    throw new InvalidDataException($"LocaleDatabase에 locale '{localeCode}'이(가) 등록되어 있지 않습니다.");
                if (!uniqueLocaleCodes.Add(localeCode))
                    throw new InvalidDataException($"CSV 헤더에 locale '{localeCode}'이(가) 중복되어 있습니다.");
                localeCodes.Add(localeCode);
            }

            database.InvalidateCache();
            database.ParseJsonAssets();

            // 데이터 행 처리
            for (int row = 1; row < rows.Count; row++)
            {
                var cells = rows[row];
                if (cells.Count == 0) continue;

                string key = cells[0].Trim();
                if (string.IsNullOrEmpty(key)) continue;

                for (int col = 1; col < cells.Count && col - 1 < localeCodes.Count; col++)
                {
                    string localeCode = localeCodes[col - 1];
                    string value = cells[col];
                    database.SetEntry(localeCode, key, value);
                }
            }
        }

        /// <summary>
        /// LocaleDatabase의 데이터를 CSV 파일로 내보내기
        /// </summary>
        public static void Export(string outputPath, LocaleDatabase database)
        {
            if (database == null) return;

            database.InvalidateCache();
            database.ParseJsonAssets();

            var allKeys = database.GetAllKeys();
            var localeCodes = database.GetAllLocaleCodes();
            var locales = database.GetAllLocales();

            var sb = new StringBuilder();

            // 헤더
            sb.Append("key");
            for (int i = 0; i < locales.Count; i++)
                sb.Append($",{EscapeCSVField(locales[i].Code)}");
            sb.AppendLine();

            // 데이터
            foreach (var key in allKeys)
            {
                sb.Append(EscapeCSVField(key));

                foreach (var code in localeCodes)
                {
                    database.TryGetValue(code, key, out var value);
                    sb.Append($",{EscapeCSVField(value ?? "")}");
                }

                sb.AppendLine();
            }

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        #region CSV Parsing (RFC 4180)

        private static List<List<string>> ParseCSV(string content)
        {
            var result = new List<List<string>>();
            var currentRow = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;
            int i = 0;

            while (i < content.Length)
            {
                char c = content[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // 이스케이프된 따옴표 확인
                        if (i + 1 < content.Length && content[i + 1] == '"')
                        {
                            currentField.Append('"');
                            i += 2;
                            continue;
                        }
                        else
                        {
                            inQuotes = false;
                            i++;
                            continue;
                        }
                    }
                    else
                    {
                        currentField.Append(c);
                        i++;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                        i++;
                    }
                    else if (c == ',')
                    {
                        currentRow.Add(currentField.ToString());
                        currentField.Clear();
                        i++;
                    }
                    else if (c == '\r' || c == '\n')
                    {
                        currentRow.Add(currentField.ToString());
                        currentField.Clear();

                        if (currentRow.Count > 0 && !(currentRow.Count == 1 && string.IsNullOrEmpty(currentRow[0])))
                            result.Add(currentRow);

                        currentRow = new List<string>();

                        // \r\n 처리
                        if (c == '\r' && i + 1 < content.Length && content[i + 1] == '\n')
                            i++;

                        i++;
                    }
                    else
                    {
                        currentField.Append(c);
                        i++;
                    }
                }
            }

            // 마지막 필드/행 처리
            if (inQuotes)
                throw new InvalidDataException("CSV에 닫히지 않은 따옴표가 있습니다.");

            if (currentField.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentField.ToString());
                if (currentRow.Count > 0)
                    result.Add(currentRow);
            }

            return result;
        }

        private static string EscapeCSVField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            bool needsQuoting = field.Contains(",") || field.Contains("\"") ||
                                field.Contains("\n") || field.Contains("\r");

            if (needsQuoting)
                return $"\"{field.Replace("\"", "\"\"")}\"";

            return field;
        }

        #endregion
    }
}
