using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AchEngine.Editor.Table
{
    public static class TableCodeGenerator
    {
        private static readonly Dictionary<string, string> TypeMap = new()
        {
            { "int", "int" },
            { "float", "float" },
            { "string", "string" },
            { "bool", "bool" },
            { "long", "long" },
            { "double", "double" },
            { "int[]", "int[]" },
            { "float[]", "float[]" },
            { "string[]", "string[]" },
            { "bool[]", "bool[]" },
        };

        private static readonly HashSet<string> CSharpKeywords = new(StringComparer.Ordinal)
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
            "void", "volatile", "while", "add", "alias", "ascending", "async", "await", "by", "descending",
            "dynamic", "equals", "from", "get", "global", "group", "init", "into", "join", "let", "managed",
            "nameof", "nint", "not", "notnull", "nuint", "on", "or", "orderby", "partial", "record", "remove",
            "required", "scoped", "select", "set", "unmanaged", "value", "var", "when", "where", "with", "yield"
        };

        public struct ColumnInfo
        {
            public string Name;
            public string Type;
            public int Index;
        }

        public static List<ColumnInfo> ParseSchema(List<string[]> csvRows)
        {
            if (csvRows.Count < 2)
                return new List<ColumnInfo>();

            var names = csvRows[0];
            var types = csvRows[1];
            var columns = new List<ColumnInfo>();

            for (int i = 0; i < names.Length && i < types.Length; i++)
            {
                var name = names[i].Trim().TrimStart('\uFEFF');
                var type = types[i].Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(name) || name.StartsWith("#"))
                    continue;

                if (!TypeMap.ContainsKey(type))
                    throw new InvalidDataException(
                        $"지원하지 않는 타입 '{type}'입니다. 컬럼: {name}");

                columns.Add(new ColumnInfo
                {
                    Name = name,
                    Type = TypeMap[type],
                    Index = i
                });
            }

            return columns;
        }

        internal static void ValidateSchema(string className, IReadOnlyList<ColumnInfo> columns)
        {
            ValidateClassName(className);

            if (columns == null || columns.Count == 0)
                throw new InvalidDataException($"'{className}' 스키마가 비어 있습니다.");

            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var column in columns)
            {
                ValidateIdentifier(column.Name, "컬럼 이름");
                if (!names.Add(column.Name))
                    throw new InvalidDataException($"'{className}'에 중복 컬럼 '{column.Name}'이(가) 있습니다.");
            }

            var idColumn = columns.FirstOrDefault(column => column.Name == "Id");
            if (string.IsNullOrEmpty(idColumn.Name))
                throw new InvalidDataException($"'{className}'에는 int 타입의 'Id' 컬럼이 필요합니다.");
            if (idColumn.Type != "int")
                throw new InvalidDataException($"'{className}.Id' 컬럼 타입은 int여야 합니다.");
        }

        internal static void ValidateClassName(string className)
            => ValidateIdentifier(className, "클래스 이름");

        private static void ValidateIdentifier(string value, string label)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidDataException($"{label}이(가) 비어 있습니다.");

            if (CSharpKeywords.Contains(value))
                throw new InvalidDataException($"{label} '{value}'은(는) C# 예약어이므로 사용할 수 없습니다.");

            if (!(value[0] == '_' || char.IsLetter(value[0])))
                throw new InvalidDataException($"{label} '{value}'은(는) 문자 또는 밑줄로 시작해야 합니다.");

            for (var index = 1; index < value.Length; index++)
            {
                if (value[index] != '_' && !char.IsLetterOrDigit(value[index]))
                    throw new InvalidDataException(
                        $"{label} '{value}'에 사용할 수 없는 문자 '{value[index]}'이(가) 있습니다.");
            }
        }

        public static string GenerateDataClass(string className, List<ColumnInfo> columns)
        {
            ValidateSchema(className, columns);

            var sb = new StringBuilder();
            sb.AppendLine("// AchEngine TableLoader가 자동 생성한 파일입니다. 직접 수정하지 마세요.");
            sb.AppendLine("using System;");
            sb.AppendLine("using AchEngine.Table;");
            sb.AppendLine("#if ACHENGINE_MEMORYPACK");
            sb.AppendLine("using MemoryPack;");
            sb.AppendLine("#endif");
            sb.AppendLine();
            sb.AppendLine("[Serializable]");
            sb.AppendLine("#if ACHENGINE_MEMORYPACK");
            sb.AppendLine("[MemoryPackable]");
            sb.AppendLine("#endif");
            sb.AppendLine($"public partial class {className} : ITableData");
            sb.AppendLine("{");

            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                sb.AppendLine("#if ACHENGINE_MEMORYPACK");
                sb.AppendLine($"    [MemoryPackOrder({i})]");
                sb.AppendLine("#endif");
                sb.AppendLine($"    public {col.Type} {col.Name} {{ get; set; }}");
                if (i < columns.Count - 1)
                    sb.AppendLine();
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string GenerateLoaderClass(List<(string className, string sheetName)> tables)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// AchEngine TableLoader가 자동 생성한 파일입니다. 직접 수정하지 마세요.");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using AchEngine.DI;");
            sb.AppendLine("using AchEngine.Table;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("#if ACHENGINE_MEMORYPACK");
            sb.AppendLine("using MemoryPack;");
            sb.AppendLine("#endif");
            sb.AppendLine();
            sb.AppendLine("public static class TableLoaderGenerated");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Resources 폴더에서 모든 테이블을 로드합니다.");
            sb.AppendLine("    /// TableManager 정적 파사드 또는 ITableService를 통해 사용할 수 있습니다.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static void LoadAll(string resourceFolder = \"Tables\")");
            sb.AppendLine("    {");

            foreach (var (className, _) in tables)
            {
                sb.AppendLine($"        {{");
                sb.AppendLine($"            var asset = Resources.Load<TextAsset>($\"{{resourceFolder}}/{className}\");");
                sb.AppendLine($"            if (asset != null)");
                sb.AppendLine($"                TableManager.Load<{className}>(asset);");
                sb.AppendLine($"            else");
                sb.AppendLine($"                Debug.LogWarning(\"[TableLoader] '{className}' 데이터 파일을 찾을 수 없습니다.\");");
                sb.AppendLine($"        }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 지정한 ITableService에 모든 테이블을 로드합니다.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static void LoadAll(ITableService service, string resourceFolder = \"Tables\")");
            sb.AppendLine("    {");

            foreach (var (className, _) in tables)
            {
                sb.AppendLine($"        {{");
                sb.AppendLine($"            var asset = Resources.Load<TextAsset>($\"{{resourceFolder}}/{className}\");");
                sb.AppendLine($"            if (asset != null)");
                sb.AppendLine($"                service.Load<{className}>(asset);");
                sb.AppendLine($"            else");
                sb.AppendLine($"                Debug.LogWarning(\"[TableLoader] '{className}' 데이터 파일을 찾을 수 없습니다.\");");
                sb.AppendLine($"        }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static void GenerateAll(TableLoaderSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (!Directory.Exists(settings.codeOutputPath))
                Directory.CreateDirectory(settings.codeOutputPath);

            var tableInfos = new List<(string className, string sheetName)>();
            var classNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var sheet in settings.sheets)
            {
                if (sheet == null || !sheet.enabled) continue;

                var className = sheet.GetClassName();
                ValidateIdentifier(className, "클래스 이름");
                if (!classNames.Add(className))
                    throw new InvalidDataException($"중복 테이블 클래스 이름 '{className}'이(가) 있습니다.");

                var csvPath = Path.Combine(settings.csvOutputPath, $"{className}.csv");
                if (!File.Exists(csvPath))
                    throw new FileNotFoundException("CSV 파일을 찾을 수 없습니다. 먼저 다운로드하세요.", csvPath);

                var csv = File.ReadAllText(csvPath);
                var rows = CsvParser.Parse(csv);
                var columns = ParseSchema(rows);

                ValidateSchema(className, columns);

                var code = GenerateDataClass(className, columns);
                var codePath = Path.Combine(settings.codeOutputPath, $"{className}.cs");
                File.WriteAllText(codePath, code);
                Debug.Log($"[TableLoader] 코드 생성 완료: {codePath}");

                tableInfos.Add((className, sheet.sheetName));
            }

            if (tableInfos.Count > 0)
            {
                var loaderCode = GenerateLoaderClass(tableInfos);
                var loaderPath = Path.Combine(settings.codeOutputPath, "TableLoaderGenerated.cs");
                File.WriteAllText(loaderPath, loaderCode);
                Debug.Log($"[TableLoader] 로더 코드 생성 완료: {loaderPath}");
            }

            AssetDatabase.Refresh();
        }
    }
}
