using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
#if ACHENGINE_MEMORYPACK
using MemoryPack;
#endif

namespace AchEngine.Editor.Table
{
    public static class TableBaker
    {
        public static void BakeAll(TableLoaderSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (!Directory.Exists(settings.binaryOutputPath))
                Directory.CreateDirectory(settings.binaryOutputPath);

            var failures = new List<string>();
            foreach (var sheet in settings.sheets)
            {
                if (sheet == null || !sheet.enabled) continue;

                try
                {
                    BakeSheet(settings, sheet);
                }
                catch (Exception e)
                {
                    failures.Add($"{sheet.sheetName}: {e.Message}");
                    Debug.LogError($"[TableLoader] '{sheet.sheetName}' 베이크 실패: {e.Message}\n{e.StackTrace}");
                }
            }

            AssetDatabase.Refresh();
            if (failures.Count > 0)
                throw new InvalidOperationException(
                    $"테이블 {failures.Count}개를 베이크하지 못했습니다.\n- {string.Join("\n- ", failures)}");

            Debug.Log("[TableLoader] 모든 테이블 베이크 완료.");
        }

        private static void BakeSheet(TableLoaderSettings settings, SheetInfo sheet)
        {
            var className = sheet.GetClassName();
            var csvPath = Path.Combine(settings.csvOutputPath, $"{className}.csv");

            if (!File.Exists(csvPath))
                throw new FileNotFoundException("CSV 파일을 찾을 수 없습니다.", csvPath);

            var type = FindType(className);
            if (type == null)
                throw new TypeLoadException(
                    $"타입 '{className}'을 찾을 수 없습니다. 코드를 생성한 뒤 컴파일이 끝났는지 확인하세요.");

            var csv = File.ReadAllText(csvPath);
            var rows = CsvParser.Parse(csv);
            var columns = TableCodeGenerator.ParseSchema(rows);
            TableCodeGenerator.ValidateSchema(className, columns);

            if (columns.Count == 0 || rows.Count < 3)
                throw new InvalidDataException($"'{className}' 테이블에 베이크할 데이터가 없습니다.");

            var listType = typeof(List<>).MakeGenericType(type);
            var list = (IList)Activator.CreateInstance(listType);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var parseErrors = new List<string>();
            for (int rowIdx = 2; rowIdx < rows.Count; rowIdx++)
            {
                var row = rows[rowIdx];

                if (row.Length == 0 || string.IsNullOrWhiteSpace(row[0]))
                    continue;

                var instance = Activator.CreateInstance(type);

                foreach (var col in columns)
                {
                    if (col.Index >= row.Length) continue;

                    var rawValue = row[col.Index].Trim();
                    var prop = properties.FirstOrDefault(p => p.Name == col.Name);
                    if (prop == null)
                    {
                        parseErrors.Add($"행 {rowIdx + 1}, 열 {col.Name}: 생성된 프로퍼티를 찾을 수 없습니다.");
                        continue;
                    }

                    try
                    {
                        var value = ParseValue(rawValue, prop.PropertyType);
                        prop.SetValue(instance, value);
                    }
                    catch (Exception e)
                    {
                        parseErrors.Add(
                            $"행 {rowIdx + 1}, 열 {col.Name}: '{rawValue}'을(를) 변환할 수 없습니다. {e.Message}");
                    }
                }

                list.Add(instance);
            }

            if (parseErrors.Count > 0)
            {
                const int maxDisplayedErrors = 20;
                var displayed = parseErrors.Take(maxDisplayedErrors);
                var suffix = parseErrors.Count > maxDisplayedErrors
                    ? $"\n...외 {parseErrors.Count - maxDisplayedErrors}건"
                    : string.Empty;
                throw new InvalidDataException(
                    $"'{className}' 데이터 변환 오류 {parseErrors.Count}건:\n- {string.Join("\n- ", displayed)}{suffix}");
            }

#if ACHENGINE_MEMORYPACK
            var serializeMethod = typeof(MemoryPackSerializer)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "Serialize" && m.IsGenericMethod && m.GetParameters().Length == 1);

            var genericSerialize = serializeMethod.MakeGenericMethod(listType);
            var bytes = (byte[])genericSerialize.Invoke(null, new object[] { list });

            var outputPath = Path.Combine(settings.binaryOutputPath, $"{className}.bytes");
            File.WriteAllBytes(outputPath, bytes);

            Debug.Log($"[TableLoader] 베이크 완료 (MemoryPack): {className} ({list.Count}개, {bytes.Length:N0} bytes)");
#else
            var jsonArray = JsonArrayFromList(list);
            var outputPath = Path.Combine(settings.binaryOutputPath, $"{className}.json");
            File.WriteAllText(outputPath, jsonArray);

            Debug.Log($"[TableLoader] 베이크 완료 (JSON): {className} ({list.Count}개)");
#endif
        }

#if !ACHENGINE_MEMORYPACK
        private static string JsonArrayFromList(IList list)
            => JsonConvert.SerializeObject(list, Formatting.None);
#endif

        private static object ParseValue(string raw, Type targetType)
        {
            if (string.IsNullOrEmpty(raw))
                return GetDefault(targetType);

            if (targetType == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(float)) return float.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(double)) return double.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(long)) return long.Parse(raw, CultureInfo.InvariantCulture);
            if (targetType == typeof(bool)) return ParseBool(raw);
            if (targetType == typeof(string)) return raw;

            if (targetType == typeof(int[]))
                return ParseArray(raw, s => int.Parse(s.Trim(), CultureInfo.InvariantCulture));
            if (targetType == typeof(float[]))
                return ParseArray(raw, s => float.Parse(s.Trim(), CultureInfo.InvariantCulture));
            if (targetType == typeof(string[]))
                return ParseArray(raw, s => s.Trim());
            if (targetType == typeof(bool[]))
                return ParseArray(raw, s => ParseBool(s.Trim()));

            return GetDefault(targetType);
        }

        private static bool ParseBool(string raw)
        {
            switch (raw.Trim().ToLowerInvariant())
            {
                case "true":
                case "1":
                case "yes":
                case "y":
                    return true;
                case "false":
                case "0":
                case "no":
                case "n":
                    return false;
                default:
                    throw new FormatException("bool 값은 true/false, 1/0, yes/no 또는 y/n이어야 합니다.");
            }
        }

        private static T[] ParseArray<T>(string raw, Func<string, T> parser)
        {
            if (string.IsNullOrEmpty(raw)) return Array.Empty<T>();

            var separator = raw.Contains('|') ? '|' : ';';
            var parts = raw.Split(separator);
            var result = new T[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                result[i] = parser(parts[i]);

            return result;
        }

        private static object GetDefault(Type type)
        {
            if (type == typeof(string)) return "";
            if (type.IsArray) return Array.CreateInstance(type.GetElementType(), 0);
            if (type.IsValueType) return Activator.CreateInstance(type);
            return null;
        }

        private static Type FindType(string className)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(className);
                if (type != null) return type;
            }
            return null;
        }
    }
}
