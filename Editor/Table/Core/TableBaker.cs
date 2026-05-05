using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
            if (!Directory.Exists(settings.binaryOutputPath))
                Directory.CreateDirectory(settings.binaryOutputPath);

            foreach (var sheet in settings.sheets)
            {
                if (!sheet.enabled) continue;

                try
                {
                    BakeSheet(settings, sheet);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[TableLoader] '{sheet.sheetName}' 踰좎씠???ㅽ뙣: {e.Message}\n{e.StackTrace}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("[TableLoader] 紐⑤뱺 ?뚯씠釉?踰좎씠???꾨즺.");
        }

        private static void BakeSheet(TableLoaderSettings settings, SheetInfo sheet)
        {
            var className = sheet.GetClassName();
            var csvPath = Path.Combine(settings.csvOutputPath, $"{className}.csv");

            if (!File.Exists(csvPath))
            {
                Debug.LogWarning($"[TableLoader] CSV ?뚯씪 ?놁쓬: {csvPath}");
                return;
            }

            var type = FindType(className);
            if (type == null)
            {
                Debug.LogError($"[TableLoader] ???'{className}'??李얠쓣 ???놁뒿?덈떎. 肄붾뱶 ?앹꽦 ??而댄뙆?쇱쓣 湲곕떎?ㅼ＜?몄슂.");
                return;
            }

            var csv = File.ReadAllText(csvPath);
            var rows = CsvParser.Parse(csv);
            var columns = TableCodeGenerator.ParseSchema(rows);

            if (columns.Count == 0 || rows.Count < 3)
            {
                Debug.LogWarning($"[TableLoader] '{className}' ?곗씠?곌? 鍮꾩뼱?덉뒿?덈떎.");
                return;
            }

            var listType = typeof(List<>).MakeGenericType(type);
            var list = (IList)Activator.CreateInstance(listType);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

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
                    if (prop == null) continue;

                    try
                    {
                        var value = ParseValue(rawValue, prop.PropertyType);
                        prop.SetValue(instance, value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(
                            $"[TableLoader] ?뚯떛 ?ㅻ쪟 ({className}, Row {rowIdx + 1}, {col.Name}): '{rawValue}' -> {e.Message}");
                    }
                }

                list.Add(instance);
            }

#if ACHENGINE_MEMORYPACK
            var serializeMethod = typeof(MemoryPackSerializer)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "Serialize" && m.IsGenericMethod && m.GetParameters().Length == 1);

            var genericSerialize = serializeMethod.MakeGenericMethod(listType);
            var bytes = (byte[])genericSerialize.Invoke(null, new object[] { list });

            var outputPath = Path.Combine(settings.binaryOutputPath, $"{className}.bytes");
            File.WriteAllBytes(outputPath, bytes);

            Debug.Log($"[TableLoader] 踰좎씠???꾨즺 (MemoryPack): {className} ({list.Count}?? {bytes.Length:N0} bytes)");
#else
            var jsonArray = JsonArrayFromList(list, type);
            var outputPath = Path.Combine(settings.binaryOutputPath, $"{className}.json");
            File.WriteAllText(outputPath, jsonArray);

            Debug.Log($"[TableLoader] 踰좎씠???꾨즺 (JSON): {className} ({list.Count}??");
#endif
        }

#if !ACHENGINE_MEMORYPACK
        private static string JsonArrayFromList(IList list, Type elementType)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(JsonUtility.ToJson(list[i]));
            }
            sb.Append("]");
            return sb.ToString();
        }
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
            var lower = raw.ToLower();
            return lower == "true" || lower == "1" || lower == "yes" || lower == "y";
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
