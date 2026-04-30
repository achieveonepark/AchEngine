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

        [MenuItem("AchEngine/Table Loader")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<TableLoaderWindow>();
            wnd.titleContent = new GUIContent("Table Loader");
            wnd.minSize = new Vector2(480, 600);
        }

        public void CreateGUI()
        {
            _settings = TableLoaderSettings.GetOrCreate();

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

            RefreshTableStatus();
        }

        #region Tabs

        private void SetupTabs()
        {
            SetupTabButton("tab-guide", "panel-guide");
            SetupTabButton("tab-settings", "panel-settings");
            SetupTabButton("tab-tables", "panel-tables");
        }

        private void SetupTabButton(string tabName, string panelName)
        {
            var btn = _root.Q<Button>(tabName);
            btn?.RegisterCallback<ClickEvent>(_ => SwitchTab(tabName, panelName));
        }

        private void SwitchTab(string activeTab, string activePanel)
        {
            string[] tabs = { "tab-guide", "tab-settings", "tab-tables" };
            string[] panels = { "panel-guide", "panel-settings", "panel-tables" };

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

        private void SetStatus(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message;
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
