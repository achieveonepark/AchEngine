using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor.Table
{
    public class TableLoaderWindow : EditorWindow
    {
        private TableLoaderSettings _settings;
        private VisualElement _root;
        private Label _statusLabel;
        private string _jsonFilePath;
        private string _jsonFileCsvPath;
        private string _jsonFolderPath;
        private string _jsonFolderCsvPath;
        private bool _openJsonCsvTab;

        [MenuItem("AchEngine/Table Loader")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<TableLoaderWindow>();
            wnd.titleContent = new GUIContent("Table Loader");
            wnd.minSize = new Vector2(480, 600);
        }

        [MenuItem("AchEngine/Table JSON to CSV")]
        public static void ShowJsonCsvWindow()
        {
            var wnd = GetWindow<TableLoaderWindow>();
            wnd.titleContent = new GUIContent("Table Loader");
            wnd.minSize = new Vector2(560, 640);
            wnd._openJsonCsvTab = true;
            wnd.Show();

            if (wnd._root != null)
            {
                wnd.SwitchTab("tab-json-csv", "panel-json-csv");
                wnd._openJsonCsvTab = false;
            }
        }

        public void CreateGUI()
        {
            _settings = TableLoaderSettings.GetOrCreate();
            _jsonFolderPath = _settings.binaryOutputPath;
            _jsonFolderCsvPath = _settings.csvOutputPath;

            var uxmlPath = FindAssetPath<VisualTreeAsset>("TableLoaderWindow");
            var ussPath = FindAssetPath<StyleSheet>("TableLoaderWindow");

            if (uxmlPath != null)
            {
                var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                uxml.CloneTree(rootVisualElement);
            }

            if (ussPath != null)
            {
                var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
                rootVisualElement.styleSheets.Add(uss);
            }

            _root = rootVisualElement;
            _statusLabel = _root.Q<Label>("label-status");

            SetupTabs();
            SetupSettings();
            SetupTables();
            SetupJsonCsv();

            RefreshTableStatus();

            if (_openJsonCsvTab)
            {
                SwitchTab("tab-json-csv", "panel-json-csv");
                _openJsonCsvTab = false;
            }
        }

        #region Tabs

        private void SetupTabs()
        {
            SetupTabButton("tab-guide", "panel-guide");
            SetupTabButton("tab-settings", "panel-settings");
            SetupTabButton("tab-tables", "panel-tables");
            SetupTabButton("tab-json-csv", "panel-json-csv");
        }

        private void SetupTabButton(string tabName, string panelName)
        {
            var btn = _root.Q<Button>(tabName);
            btn?.RegisterCallback<ClickEvent>(_ => SwitchTab(tabName, panelName));
        }

        private void SwitchTab(string activeTab, string activePanel)
        {
            string[] tabs = { "tab-guide", "tab-settings", "tab-tables", "tab-json-csv" };
            string[] panels = { "panel-guide", "panel-settings", "panel-tables", "panel-json-csv" };

            for (int i = 0; i < tabs.Length; i++)
            {
                var tabBtn = _root.Q<Button>(tabs[i]);
                var panel = _root.Q(panels[i]);

                if (tabs[i] == activeTab)
                {
                    tabBtn.AddToClassList("tab-active");
                    panel.RemoveFromClassList("panel-hidden");
                }
                else
                {
                    tabBtn.RemoveFromClassList("tab-active");
                    panel.AddToClassList("panel-hidden");
                }
            }
        }

        #endregion

        #region Settings

        private void SetupSettings()
        {
            BindTextField("field-spreadsheet-id", _settings.spreadsheetId, v => _settings.spreadsheetId = v);
            BindTextField("field-csv-path", _settings.csvOutputPath, v => _settings.csvOutputPath = v);
            BindTextField("field-code-path", _settings.codeOutputPath, v => _settings.codeOutputPath = v);
            BindTextField("field-binary-path", _settings.binaryOutputPath, v => _settings.binaryOutputPath = v);

            BindToggle("toggle-auto-generate", _settings.autoGenerateOnDownload, v => _settings.autoGenerateOnDownload = v);
            BindToggle("toggle-auto-bake", _settings.autoBakeOnGenerate, v => _settings.autoBakeOnGenerate = v);

            _root.Q<Button>("btn-open-sheet")?.RegisterCallback<ClickEvent>(_ =>
            {
                if (!string.IsNullOrEmpty(_settings.spreadsheetId))
                    Application.OpenURL(_settings.GetSpreadsheetUrl());
            });

            _root.Q<Button>("btn-add-sheet")?.RegisterCallback<ClickEvent>(_ => AddSheetEntry());
            _root.Q<Button>("btn-save-settings")?.RegisterCallback<ClickEvent>(_ => SaveSettings());

            RefreshSheetList();
        }

        private void BindTextField(string name, string value, System.Action<string> setter)
        {
            var field = _root.Q<TextField>(name);
            if (field == null) return;
            field.value = value;
            field.RegisterValueChangedCallback(evt => setter(evt.newValue));
        }

        private void BindToggle(string name, bool value, System.Action<bool> setter)
        {
            var toggle = _root.Q<Toggle>(name);
            if (toggle == null) return;
            toggle.value = value;
            toggle.RegisterValueChangedCallback(evt => setter(evt.newValue));
        }

        private void RefreshSheetList()
        {
            var container = _root.Q("sheet-list-container");
            if (container == null) return;
            container.Clear();

            for (int i = 0; i < _settings.sheets.Count; i++)
            {
                var sheet = _settings.sheets[i];
                var idx = i;
                container.Add(CreateSheetEntry(sheet, idx));
            }
        }

        private VisualElement CreateSheetEntry(SheetInfo sheet, int index)
        {
            var row = new VisualElement();
            row.AddToClassList("sheet-entry");

            var toggle = new Toggle { value = sheet.enabled };
            toggle.RegisterValueChangedCallback(evt => sheet.enabled = evt.newValue);

            var nameField = new TextField("시트명") { value = sheet.sheetName };
            nameField.RegisterValueChangedCallback(evt => sheet.sheetName = evt.newValue);

            var gidField = new TextField("GID") { value = sheet.sheetGid };
            gidField.RegisterValueChangedCallback(evt => sheet.sheetGid = evt.newValue);
            gidField.style.maxWidth = 80;

            var classField = new TextField("클래스명") { value = sheet.className };
            classField.RegisterValueChangedCallback(evt => sheet.className = evt.newValue);

            var removeBtn = new Button(() =>
            {
                _settings.sheets.RemoveAt(index);
                RefreshSheetList();
            }) { text = "X" };
            removeBtn.AddToClassList("btn-danger");

            row.Add(toggle);
            row.Add(nameField);
            row.Add(gidField);
            row.Add(classField);
            row.Add(removeBtn);

            return row;
        }

        private void AddSheetEntry()
        {
            _settings.sheets.Add(new SheetInfo());
            RefreshSheetList();
        }

        private void SaveSettings()
        {
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            SetStatus("설정을 저장했습니다.");
            RefreshTableStatus();
        }

        #endregion

        #region Tables

        private void SetupTables()
        {
            _root.Q<Button>("btn-download-all")?.RegisterCallback<ClickEvent>(_ => DownloadAll());
            _root.Q<Button>("btn-generate-all")?.RegisterCallback<ClickEvent>(_ => GenerateAll());
            _root.Q<Button>("btn-bake-all")?.RegisterCallback<ClickEvent>(_ => BakeAll());
            _root.Q<Button>("btn-all-in-one")?.RegisterCallback<ClickEvent>(_ => AllInOne());
        }

        private async void DownloadAll()
        {
            SetStatus("다운로드 중...");
            await GoogleSheetDownloader.DownloadAllAsync(_settings, (current, total, name) =>
            {
                SetStatus($"다운로드 중 ({current}/{total}): {name}");
            });

            SetStatus("다운로드가 완료되었습니다.");
            RefreshTableStatus();

            if (_settings.autoGenerateOnDownload)
                GenerateAll();
        }

        private void GenerateAll()
        {
            SetStatus("코드를 생성하는 중...");
            TableCodeGenerator.GenerateAll(_settings);
            SetStatus("코드 생성이 완료되었습니다. 컴파일을 기다리는 중...");
            RefreshTableStatus();

            if (_settings.autoBakeOnGenerate)
            {
                EditorApplication.delayCall += () =>
                {
                    if (!EditorApplication.isCompiling)
                        BakeAll();
                };
            }
        }

        private void BakeAll()
        {
            SetStatus("베이킹 중...");
            TableBaker.BakeAll(_settings);
            SetStatus("베이킹이 완료되었습니다.");
            RefreshTableStatus();
        }

        private async void AllInOne()
        {
            SetStatus("일괄 실행: 다운로드 중...");
            await GoogleSheetDownloader.DownloadAllAsync(_settings, (current, total, name) =>
            {
                SetStatus($"다운로드 중 ({current}/{total}): {name}");
            });

            SetStatus("일괄 실행: 코드 생성 중...");
            TableCodeGenerator.GenerateAll(_settings);

            SetStatus("일괄 실행: 컴파일 완료 후 베이킹을 진행합니다...");
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isCompiling)
                {
                    SetStatus("일괄 실행: 베이킹 중...");
                    TableBaker.BakeAll(_settings);
                    SetStatus("일괄 실행이 완료되었습니다.");
                    RefreshTableStatus();
                }
            };
        }

        private void RefreshTableStatus()
        {
            var container = _root.Q("table-status-container");
            if (container == null) return;
            container.Clear();

            foreach (var sheet in _settings.sheets)
            {
                if (!sheet.enabled) continue;

                var className = sheet.GetClassName();
                var row = new VisualElement();
                row.AddToClassList("table-row");

                var nameLabel = new Label(className);
                nameLabel.AddToClassList("table-row-name");

                var csvExists = File.Exists(Path.Combine(_settings.csvOutputPath, $"{className}.csv"));
                var codeExists = File.Exists(Path.Combine(_settings.codeOutputPath, $"{className}.cs"));
#if ACHENGINE_MEMORYPACK
                var binaryExt = ".bytes";
#else
                var binaryExt = ".json";
#endif
                var binaryExists = File.Exists(Path.Combine(_settings.binaryOutputPath, $"{className}{binaryExt}"));

                var statusText = "";
                var statusClass = "status-ok";

                if (!csvExists)
                {
                    statusText = "CSV 없음";
                    statusClass = "status-missing";
                }
                else if (!codeExists)
                {
                    statusText = "코드 없음";
                    statusClass = "status-outdated";
                }
                else if (!binaryExists)
                {
                    statusText = "베이킹 필요";
                    statusClass = "status-outdated";
                }
                else
                {
                    statusText = "준비됨";
                }

                var statusLabel = new Label(statusText);
                statusLabel.AddToClassList("table-row-status");
                statusLabel.AddToClassList(statusClass);

                row.Add(nameLabel);
                row.Add(statusLabel);
                container.Add(row);
            }
        }

        #endregion

        #region JSON to CSV

        private void SetupJsonCsv()
        {
            BindPathField("field-json-file", _jsonFilePath, v => _jsonFilePath = v);
            BindPathField("field-json-file-csv", _jsonFileCsvPath, v => _jsonFileCsvPath = v);
            BindPathField("field-json-folder", _jsonFolderPath, v => _jsonFolderPath = v);
            BindPathField("field-json-folder-csv", _jsonFolderCsvPath, v => _jsonFolderCsvPath = v);

            _root.Q<Button>("btn-pick-json-file")?.RegisterCallback<ClickEvent>(_ =>
            {
                var path = EditorUtility.OpenFilePanel("JSON 파일 선택", ToAbsolutePath(_settings.binaryOutputPath), "json");
                if (string.IsNullOrEmpty(path)) return;

                _jsonFilePath = ToProjectRelativePath(path);
                if (string.IsNullOrEmpty(_jsonFileCsvPath))
                    _jsonFileCsvPath = Path.Combine(_settings.csvOutputPath, $"{Path.GetFileNameWithoutExtension(path)}.csv");
                RefreshJsonCsvFields();
            });

            _root.Q<Button>("btn-pick-json-file-csv")?.RegisterCallback<ClickEvent>(_ =>
            {
                var defaultName = string.IsNullOrEmpty(_jsonFilePath)
                    ? "table"
                    : Path.GetFileNameWithoutExtension(_jsonFilePath);
                var path = EditorUtility.SaveFilePanel("CSV 저장 위치 선택", ToAbsolutePath(_settings.csvOutputPath), defaultName, "csv");
                if (string.IsNullOrEmpty(path)) return;

                _jsonFileCsvPath = ToProjectRelativePath(path);
                RefreshJsonCsvFields();
            });

            _root.Q<Button>("btn-convert-json-file")?.RegisterCallback<ClickEvent>(_ => ConvertJsonFile());

            _root.Q<Button>("btn-pick-json-folder")?.RegisterCallback<ClickEvent>(_ =>
            {
                var path = EditorUtility.OpenFolderPanel("JSON 폴더 선택", ToAbsolutePath(_settings.binaryOutputPath), "");
                if (string.IsNullOrEmpty(path)) return;

                _jsonFolderPath = ToProjectRelativePath(path);
                RefreshJsonCsvFields();
            });

            _root.Q<Button>("btn-pick-json-folder-csv")?.RegisterCallback<ClickEvent>(_ =>
            {
                var path = EditorUtility.OpenFolderPanel("CSV 출력 폴더 선택", ToAbsolutePath(_settings.csvOutputPath), "");
                if (string.IsNullOrEmpty(path)) return;

                _jsonFolderCsvPath = ToProjectRelativePath(path);
                RefreshJsonCsvFields();
            });

            _root.Q<Button>("btn-convert-json-folder")?.RegisterCallback<ClickEvent>(_ => ConvertJsonFolder());
            RefreshJsonCsvFields();
        }

        private void BindPathField(string name, string value, System.Action<string> setter)
        {
            var field = _root.Q<TextField>(name);
            if (field == null) return;
            field.value = value ?? "";
            field.RegisterValueChangedCallback(evt => setter(evt.newValue));
        }

        private void RefreshJsonCsvFields()
        {
            SetFieldValue("field-json-file", _jsonFilePath);
            SetFieldValue("field-json-file-csv", _jsonFileCsvPath);
            SetFieldValue("field-json-folder", _jsonFolderPath);
            SetFieldValue("field-json-folder-csv", _jsonFolderCsvPath);
        }

        private void SetFieldValue(string name, string value)
        {
            var field = _root.Q<TextField>(name);
            if (field != null && field.value != (value ?? ""))
                field.SetValueWithoutNotify(value ?? "");
        }

        private void ConvertJsonFile()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_jsonFilePath) || string.IsNullOrWhiteSpace(_jsonFileCsvPath))
                {
                    SetStatus("JSON 파일과 CSV 저장 위치를 모두 지정하세요.");
                    return;
                }

                var result = TableJsonCsvExporter.ExportFile(ToAbsolutePath(_jsonFilePath), ToAbsolutePath(_jsonFileCsvPath));
                AssetDatabase.Refresh();
                SetStatus($"JSON → CSV 완료: {Path.GetFileName(result.CsvPath)} ({result.RowCount}행, {result.ColumnCount}열)");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TableLoader] JSON → CSV 실패: {e.Message}\n{e.StackTrace}");
                SetStatus($"JSON → CSV 실패: {e.Message}");
            }
        }

        private void ConvertJsonFolder()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_jsonFolderPath) || string.IsNullOrWhiteSpace(_jsonFolderCsvPath))
                {
                    SetStatus("JSON 폴더와 CSV 출력 폴더를 모두 지정하세요.");
                    return;
                }

                var recursive = _root.Q<Toggle>("toggle-json-folder-recursive")?.value ?? false;
                var results = TableJsonCsvExporter.ExportFolder(ToAbsolutePath(_jsonFolderPath), ToAbsolutePath(_jsonFolderCsvPath), recursive);
                AssetDatabase.Refresh();
                SetStatus($"폴더 JSON → CSV 완료: {results.Count}개 파일 변환");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TableLoader] 폴더 JSON → CSV 실패: {e.Message}\n{e.StackTrace}");
                SetStatus($"폴더 JSON → CSV 실패: {e.Message}");
            }
        }

        private static string ToAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Application.dataPath;
            if (Path.IsPathRooted(path))
                return path;

            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        }

        private static string ToProjectRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            var projectRoot = Directory.GetCurrentDirectory().Replace('\\', '/');
            var normalized = Path.GetFullPath(path).Replace('\\', '/');
            if (normalized.StartsWith(projectRoot + "/", System.StringComparison.OrdinalIgnoreCase))
                return normalized.Substring(projectRoot.Length + 1);

            return path;
        }

        #endregion

        private void SetStatus(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message;

            var jsonStatusLabel = _root?.Q<Label>("label-json-csv-status");
            if (jsonStatusLabel != null)
                jsonStatusLabel.text = message;
        }

        private static string FindAssetPath<T>(string name) where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name} {name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == name)
                    return path;
            }
            return null;
        }
    }
}
