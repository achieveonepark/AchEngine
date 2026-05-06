using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AchEngine.Editor.Table
{
    public static class TableJsonCsvExporter
    {
        public readonly struct ExportResult
        {
            public ExportResult(string jsonPath, string csvPath, int rowCount, int columnCount)
            {
                JsonPath = jsonPath;
                CsvPath = csvPath;
                RowCount = rowCount;
                ColumnCount = columnCount;
            }

            public string JsonPath { get; }
            public string CsvPath { get; }
            public int RowCount { get; }
            public int ColumnCount { get; }
        }

        public static ExportResult ExportFile(string jsonPath, string csvPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
                throw new ArgumentException("JSON path is empty.", nameof(jsonPath));
            if (string.IsNullOrWhiteSpace(csvPath))
                throw new ArgumentException("CSV path is empty.", nameof(csvPath));
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("JSON file not found.", jsonPath);

            var rows = ReadRows(jsonPath);
            var csv = BuildCsv(rows);
            var directory = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(csvPath, csv, new UTF8Encoding(true));
            return new ExportResult(jsonPath, csvPath, rows.Count, CountColumns(rows));
        }

        public static List<ExportResult> ExportFolder(string jsonFolderPath, string csvFolderPath, bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(jsonFolderPath))
                throw new ArgumentException("JSON folder path is empty.", nameof(jsonFolderPath));
            if (string.IsNullOrWhiteSpace(csvFolderPath))
                throw new ArgumentException("CSV folder path is empty.", nameof(csvFolderPath));
            if (!Directory.Exists(jsonFolderPath))
                throw new DirectoryNotFoundException(jsonFolderPath);

            if (!Directory.Exists(csvFolderPath))
                Directory.CreateDirectory(csvFolderPath);

            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json", option)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var results = new List<ExportResult>();
            foreach (var jsonPath in jsonFiles)
            {
                var relativePath = GetRelativePath(jsonFolderPath, jsonPath);
                var relativeCsvPath = Path.ChangeExtension(relativePath, ".csv");
                var csvPath = Path.Combine(csvFolderPath, relativeCsvPath);
                results.Add(ExportFile(jsonPath, csvPath));
            }

            return results;
        }

        private static List<OrderedObject> ReadRows(string jsonPath)
        {
            var json = File.ReadAllText(jsonPath);
            var root = JsonReader.Parse(json);

            if (root is JsonArray array)
                return ArrayToRows(array, jsonPath);

            if (root is OrderedObject obj)
            {
                var items = obj.Get("Items") ?? obj.Get("items");
                if (items is JsonArray wrappedArray)
                    return ArrayToRows(wrappedArray, jsonPath);

                return new List<OrderedObject> { obj };
            }

            throw new FormatException($"Root JSON must be an object or array: {jsonPath}");
        }

        private static List<OrderedObject> ArrayToRows(JsonArray array, string jsonPath)
        {
            var rows = new List<OrderedObject>();
            foreach (var item in array.Items)
            {
                if (item is OrderedObject obj)
                    rows.Add(obj);
                else
                    throw new FormatException($"JSON array must contain objects only: {jsonPath}");
            }

            return rows;
        }

        private static string BuildCsv(List<OrderedObject> rows)
        {
            var columns = CollectColumns(rows);
            var types = columns.Select(column => InferType(rows, column)).ToList();
            var sb = new StringBuilder();

            AppendRow(sb, columns);
            AppendRow(sb, types);

            foreach (var row in rows)
            {
                var values = columns.Select(column => ToCellValue(row.Get(column)));
                AppendRow(sb, values);
            }

            return sb.ToString();
        }

        private static List<string> CollectColumns(List<OrderedObject> rows)
        {
            var columns = new List<string>();
            var seen = new HashSet<string>();

            foreach (var row in rows)
            {
                foreach (var pair in row.Properties)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key) || seen.Contains(pair.Key))
                        continue;

                    seen.Add(pair.Key);
                    columns.Add(pair.Key);
                }
            }

            var idIndex = columns.FindIndex(c => string.Equals(c, "Id", StringComparison.Ordinal));
            if (idIndex > 0)
            {
                var id = columns[idIndex];
                columns.RemoveAt(idIndex);
                columns.Insert(0, id);
            }

            return columns;
        }

        private static int CountColumns(List<OrderedObject> rows) => CollectColumns(rows).Count;

        private static string InferType(List<OrderedObject> rows, string column)
        {
            var values = rows.Select(row => row.Get(column)).Where(value => value != null && value is not JsonNull).ToList();
            if (values.Count == 0)
                return "string";

            if (values.All(value => value is JsonArray))
                return InferArrayType(values.Cast<JsonArray>());

            if (values.All(value => value is JsonBoolean))
                return "bool";
            if (values.All(value => value is JsonNumber number && number.IsInteger))
                return values.All(value => IsInt((JsonNumber)value)) ? "int" : "long";
            if (values.All(value => value is JsonNumber))
                return "double";

            return "string";
        }

        private static string InferArrayType(IEnumerable<JsonArray> arrays)
        {
            var items = arrays.SelectMany(array => array.Items).Where(value => value != null && value is not JsonNull).ToList();
            if (items.Count == 0)
                return "string[]";

            if (items.All(value => value is JsonBoolean))
                return "bool[]";
            if (items.All(value => value is JsonNumber number && number.IsInteger && IsInt(number)))
                return "int[]";
            if (items.All(value => value is JsonNumber))
                return "float[]";

            return "string[]";
        }

        private static bool IsInt(JsonNumber number)
        {
            return number.DoubleValue >= int.MinValue && number.DoubleValue <= int.MaxValue;
        }

        private static string ToCellValue(JsonValue value)
        {
            return value switch
            {
                null => "",
                JsonNull => "",
                JsonString text => text.Value ?? "",
                JsonNumber number => number.Raw,
                JsonBoolean boolean => boolean.Value ? "true" : "false",
                JsonArray array => string.Join("|", array.Items.Select(ToCellValue)),
                OrderedObject obj => ToCompactJson(obj),
                _ => ""
            };
        }

        private static string ToCompactJson(JsonValue value)
        {
            return value switch
            {
                null => "null",
                JsonNull => "null",
                JsonString text => $"\"{EscapeJsonString(text.Value ?? "")}\"",
                JsonNumber number => number.Raw,
                JsonBoolean boolean => boolean.Value ? "true" : "false",
                JsonArray array => $"[{string.Join(",", array.Items.Select(ToCompactJson))}]",
                OrderedObject obj => $"{{{string.Join(",", obj.Properties.Select(pair => $"\"{EscapeJsonString(pair.Key)}\":{ToCompactJson(pair.Value)}"))}}}",
                _ => "null"
            };
        }

        private static string EscapeJsonString(string value)
        {
            var sb = new StringBuilder();
            foreach (var c in value)
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
                    default:
                        if (char.IsControl(c))
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        private static void AppendRow(StringBuilder sb, IEnumerable<string> fields)
        {
            bool first = true;
            foreach (var field in fields)
            {
                if (!first)
                    sb.Append(',');

                sb.Append(EscapeCsv(field));
                first = false;
            }

            sb.AppendLine();
        }

        private static string EscapeCsv(string value)
        {
            value ??= "";
            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
                return value;

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static string GetRelativePath(string basePath, string path)
        {
            var baseUri = new Uri(AppendDirectorySeparatorChar(Path.GetFullPath(basePath)));
            var pathUri = new Uri(Path.GetFullPath(path));
            return Uri.UnescapeDataString(baseUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? path
                : path + Path.DirectorySeparatorChar;
        }

        private abstract class JsonValue
        {
        }

        private sealed class JsonNull : JsonValue
        {
        }

        private sealed class JsonString : JsonValue
        {
            public JsonString(string value) => Value = value;
            public string Value { get; }
        }

        private sealed class JsonNumber : JsonValue
        {
            public JsonNumber(string raw)
            {
                Raw = raw;
                IsInteger = raw.IndexOfAny(new[] { '.', 'e', 'E' }) < 0;
                double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue);
                DoubleValue = doubleValue;
            }

            public string Raw { get; }
            public bool IsInteger { get; }
            public double DoubleValue { get; }
        }

        private sealed class JsonBoolean : JsonValue
        {
            public JsonBoolean(bool value) => Value = value;
            public bool Value { get; }
        }

        private sealed class JsonArray : JsonValue
        {
            public List<JsonValue> Items { get; } = new();
        }

        private sealed class OrderedObject : JsonValue
        {
            public List<KeyValuePair<string, JsonValue>> Properties { get; } = new();

            public JsonValue Get(string key)
            {
                for (int i = Properties.Count - 1; i >= 0; i--)
                {
                    if (Properties[i].Key == key)
                        return Properties[i].Value;
                }

                return null;
            }
        }

        private sealed class JsonReader
        {
            private readonly string _json;
            private int _index;

            private JsonReader(string json) => _json = json;

            public static JsonValue Parse(string json)
            {
                var reader = new JsonReader(json);
                var value = reader.ParseValue();
                reader.SkipWhitespace();
                if (!reader.IsEnd)
                    throw new FormatException($"Unexpected JSON token at index {reader._index}.");
                return value;
            }

            private bool IsEnd => _index >= _json.Length;

            private JsonValue ParseValue()
            {
                SkipWhitespace();
                if (IsEnd)
                    throw new FormatException("Unexpected end of JSON.");

                return _json[_index] switch
                {
                    '{' => ParseObject(),
                    '[' => ParseArray(),
                    '"' => new JsonString(ParseString()),
                    't' => ParseLiteral("true", new JsonBoolean(true)),
                    'f' => ParseLiteral("false", new JsonBoolean(false)),
                    'n' => ParseLiteral("null", new JsonNull()),
                    '-' => ParseNumber(),
                    _ when char.IsDigit(_json[_index]) => ParseNumber(),
                    _ => throw new FormatException($"Unexpected JSON token at index {_index}.")
                };
            }

            private OrderedObject ParseObject()
            {
                Expect('{');
                var obj = new OrderedObject();
                SkipWhitespace();

                if (TryConsume('}'))
                    return obj;

                while (true)
                {
                    SkipWhitespace();
                    var key = ParseString();
                    SkipWhitespace();
                    Expect(':');
                    var value = ParseValue();
                    obj.Properties.Add(new KeyValuePair<string, JsonValue>(key, value));
                    SkipWhitespace();

                    if (TryConsume('}'))
                        return obj;

                    Expect(',');
                }
            }

            private JsonArray ParseArray()
            {
                Expect('[');
                var array = new JsonArray();
                SkipWhitespace();

                if (TryConsume(']'))
                    return array;

                while (true)
                {
                    array.Items.Add(ParseValue());
                    SkipWhitespace();

                    if (TryConsume(']'))
                        return array;

                    Expect(',');
                }
            }

            private string ParseString()
            {
                Expect('"');
                var sb = new StringBuilder();

                while (!IsEnd)
                {
                    var c = _json[_index++];
                    if (c == '"')
                        return sb.ToString();

                    if (c != '\\')
                    {
                        sb.Append(c);
                        continue;
                    }

                    if (IsEnd)
                        throw new FormatException("Unexpected end of JSON string escape.");

                    var escaped = _json[_index++];
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
                            if (_index + 4 > _json.Length)
                                throw new FormatException("Invalid unicode escape sequence.");

                            var hex = _json.Substring(_index, 4);
                            sb.Append((char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            _index += 4;
                            break;
                        default:
                            throw new FormatException($"Invalid JSON string escape '\\{escaped}'.");
                    }
                }

                throw new FormatException("Unterminated JSON string.");
            }

            private JsonValue ParseLiteral(string literal, JsonValue value)
            {
                if (_index + literal.Length > _json.Length ||
                    string.Compare(_json, _index, literal, 0, literal.Length, StringComparison.Ordinal) != 0)
                    throw new FormatException($"Invalid JSON literal at index {_index}.");

                _index += literal.Length;
                return value;
            }

            private JsonNumber ParseNumber()
            {
                var start = _index;
                if (_json[_index] == '-')
                    _index++;

                ReadDigits();

                if (!IsEnd && _json[_index] == '.')
                {
                    _index++;
                    ReadDigits();
                }

                if (!IsEnd && (_json[_index] == 'e' || _json[_index] == 'E'))
                {
                    _index++;
                    if (!IsEnd && (_json[_index] == '+' || _json[_index] == '-'))
                        _index++;
                    ReadDigits();
                }

                return new JsonNumber(_json.Substring(start, _index - start));
            }

            private void ReadDigits()
            {
                var start = _index;
                while (!IsEnd && char.IsDigit(_json[_index]))
                    _index++;

                if (start == _index)
                    throw new FormatException($"Expected digit at index {_index}.");
            }

            private void SkipWhitespace()
            {
                while (!IsEnd && char.IsWhiteSpace(_json[_index]))
                    _index++;
            }

            private void Expect(char expected)
            {
                SkipWhitespace();
                if (IsEnd || _json[_index] != expected)
                    throw new FormatException($"Expected '{expected}' at index {_index}.");

                _index++;
            }

            private bool TryConsume(char expected)
            {
                SkipWhitespace();
                if (IsEnd || _json[_index] != expected)
                    return false;

                _index++;
                return true;
            }
        }
    }
}
