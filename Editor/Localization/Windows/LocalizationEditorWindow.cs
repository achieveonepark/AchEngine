using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Localization.Editor
{
    /// <summary>
    /// 메인 Localization 에디터 창.
    /// 키/언어 관리, 인라인 편집, CSV/JSON 가져오기/내보내기, 코드 생성 등을 제공.
    /// </summary>
    public class LocalizationEditorWindow : EditorWindow
    {
        private LocalizationSettings _settings;
        private LocaleDatabase _database;
        private LocalizationTableView _tableView;

        // UI elements
        private TextField _searchField;
        private DropdownField _languageFilter;
        private DropdownField _previewLocale;
        private Label _statsLabel;
        private VisualElement _tableContainer;
        private VisualElement _emptyState;
        private Button _undoButton;
        private Button _redoButton;

        private readonly Stack<LocalizationSnapshot> _undoStack = new Stack<LocalizationSnapshot>();
        private readonly Stack<LocalizationSnapshot> _redoStack = new Stack<LocalizationSnapshot>();
        private LocaleDatabase _historyDatabase;
        private bool _isApplyingHistory;

        private sealed class LocalizationSnapshot
        {
            public List<LocaleSnapshot> locales = new List<LocaleSnapshot>();
        }

        private sealed class LocaleSnapshot
        {
            public string code;
            public string displayName;
            public string assetPath;
            public Dictionary<string, string> data = new Dictionary<string, string>();
        }

        [MenuItem("AchEngine/Localization/String Table Editor")]
        public static void Open()
        {
            var window = GetWindow<LocalizationEditorWindow>();
            window.titleContent = new GUIContent("Localization");
            window.minSize = new Vector2(600, 400);
        }

        private void CreateGUI()
        {
            _settings = LocalizationEditorUtility.GetOrCreateSettings();
            _database = _settings.database;

            // UXML 로드 시도
            string uxmlPath = LocalizationEditorUtility.GetUXMLPath("LocalizationEditorWindow.uxml");
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (visualTree != null)
            {
                visualTree.CloneTree(rootVisualElement);
            }
            else
            {
                BuildFallbackUI();
            }

            // 스타일시트 로드
            string commonUssPath = LocalizationEditorUtility.GetUSSPath("Common.uss");
            var commonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(commonUssPath);
            if (commonStyle != null) rootVisualElement.styleSheets.Add(commonStyle);

            string editorUssPath = LocalizationEditorUtility.GetUSSPath("LocalizationEditor.uss");
            var editorStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(editorUssPath);
            if (editorStyle != null) rootVisualElement.styleSheets.Add(editorStyle);

            BindElements();
            RefreshAll();
        }

        private void BindElements()
        {
            rootVisualElement.UnregisterCallback<KeyDownEvent>(OnEditorKeyDown, TrickleDown.TrickleDown);
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnEditorKeyDown, TrickleDown.TrickleDown);

            // Search field
            _searchField = rootVisualElement.Q<TextField>("search-field");
            if (_searchField != null)
            {
                _searchField.RegisterValueChangedCallback(evt =>
                {
                    _tableView?.SetFilter(evt.newValue);
                    UpdateStats();
                });
            }

            // Language filter (not a strict filter, placeholder for future use)
            _languageFilter = rootVisualElement.Q<DropdownField>("language-filter");

            // Buttons
            BindButton("btn-add-key", OnAddKey);
            BindButton("btn-add-language", OnAddLanguage);
            BindButton("btn-import-csv", OnImportCSV);
            BindButton("btn-export-csv", OnExportCSV);
            BindButton("btn-import-json", OnImportJSON);
            BindButton("btn-export-json", OnExportJSON);
            BindButton("btn-undo", () => OnUndo());
            BindButton("btn-redo", () => OnRedo());
            BindButton("btn-generate-keys", OnGenerateKeys);

            // Stats label
            _statsLabel = rootVisualElement.Q<Label>("stats-label");

            // Table container
            _tableContainer = rootVisualElement.Q<VisualElement>("table-container");
            _emptyState = rootVisualElement.Q<VisualElement>("empty-state");

            // Preview locale
            _previewLocale = rootVisualElement.Q<DropdownField>("preview-locale");
            _undoButton = rootVisualElement.Q<Button>("btn-undo");
            _redoButton = rootVisualElement.Q<Button>("btn-redo");
            UpdateHistoryButtons();
        }

        private void BindButton(string name, System.Action action)
        {
            var btn = rootVisualElement.Q<Button>(name);
            if (btn != null) btn.clicked += action;
        }

        private void RefreshAll()
        {
            _settings = LocalizationEditorUtility.GetOrCreateSettings();
            _database = _settings.database;

            if (!ReferenceEquals(_database, _historyDatabase))
            {
                _undoStack.Clear();
                _redoStack.Clear();
                _historyDatabase = _database;
            }

            if (_database == null)
            {
                ShowEmptyState(true);
                UpdateHistoryButtons();
                return;
            }

            ShowEmptyState(false);

            // 테이블 뷰 구성
            if (_tableContainer != null)
            {
                _tableContainer.Clear();
                _tableView = new LocalizationTableView();
                _tableView.CellValueChanged += OnCellValueChanged;
                _tableView.KeyDeleteRequested += OnKeyDeleteRequested;
                _tableView.AddKeyRequested += OnAddKey;
                _tableView.AddLanguageRequested += OnAddLanguage;
                _tableView.SetData(_database);
                _tableContainer.Add(_tableView);
            }

            RefreshLanguageFilter();
            RefreshPreviewLocale();
            UpdateStats();
            UpdateHistoryButtons();
        }

        private void ShowEmptyState(bool show)
        {
            if (_tableContainer != null)
                _tableContainer.style.display = show ? DisplayStyle.None : DisplayStyle.Flex;
            if (_emptyState != null)
                _emptyState.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void RefreshLanguageFilter()
        {
            if (_languageFilter == null || _database == null) return;

            var choices = new List<string> { "전체 언어" };
            choices.AddRange(_database.GetAllLocales().Select(l => $"{l.DisplayName} ({l.Code})"));
            _languageFilter.choices = choices;
            _languageFilter.index = 0;
        }

        private void RefreshPreviewLocale()
        {
            if (_previewLocale == null || _database == null) return;

            var choices = _database.GetAllLocales().Select(l => $"{l.DisplayName} ({l.Code})").ToList();
            _previewLocale.choices = choices;
            if (choices.Count > 0) _previewLocale.index = 0;
        }

        private void UpdateStats()
        {
            if (_statsLabel == null) return;

            if (_tableView != null)
            {
                var (keyCount, localeCount, missingCount) = _tableView.GetStats();
                _statsLabel.text = $"키: {keyCount} | 언어: {localeCount} | 미번역: {missingCount}";
            }
            else
            {
                _statsLabel.text = "키: 0 | 언어: 0 | 미번역: 0";
            }
        }

        #region Event Handlers

        private void OnCellValueChanged(string key, string localeCode, string newValue)
        {
            if (_database == null || _isApplyingHistory) return;

            _database.TryGetValue(localeCode, key, out var currentValue);
            if (string.Equals(currentValue ?? "", newValue ?? "", System.StringComparison.Ordinal))
                return;

            RecordUndoSnapshot();
            _database.SetEntry(localeCode, key, newValue);
            SaveDatabaseToJson(localeCode);
            UpdateStats();
        }

        private void OnKeyDeleteRequested(string key)
        {
            if (!EditorUtility.DisplayDialog("키 삭제",
                $"'{key}' 키를 정말 삭제하시겠습니까?", "삭제", "취소"))
                return;

            RecordUndoSnapshot();
            _database.RemoveKey(key);
            SaveAllLocalesToJson();
            _tableView?.RefreshView();
            UpdateStats();
        }

        private void OnAddKey()
        {
            if (_database == null)
            {
                EditorUtility.DisplayDialog("오류", "먼저 Locale Database를 설정하세요.", "확인");
                return;
            }

            if (_database.GetAllLocaleCodes().Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "언어를 먼저 추가하세요",
                    "로컬라이제이션 키를 추가하기 전에 언어를 하나 이상 만들어 주세요.",
                    "확인");
                return;
            }

            var popup = new AddKeyPopup();
            popup.OnConfirm += newKey =>
            {
                if (string.IsNullOrWhiteSpace(newKey))
                    return;

                RecordUndoSnapshot();

                // 모든 locale에 빈 값으로 키 추가
                foreach (var code in _database.GetAllLocaleCodes())
                    _database.SetEntry(code, newKey, "");

                SaveAllLocalesToJson();
                _tableView?.RefreshView();
                UpdateStats();
            };

            UnityEditor.PopupWindow.Show(
                rootVisualElement.Q<Button>("btn-add-key").worldBound,
                popup
            );
        }

        private void OnAddLanguage()
        {
            if (_database == null)
            {
                EditorUtility.DisplayDialog("오류", "먼저 Locale Database를 설정하세요.", "확인");
                return;
            }

            var popup = new AddLanguagePopup();
            popup.OnConfirm += (code, displayName) =>
            {
                if (string.IsNullOrWhiteSpace(code)) return;

                if (_database.HasLocale(code))
                {
                    EditorUtility.DisplayDialog("오류", $"'{code}' 로케일이 이미 존재합니다.", "확인");
                    return;
                }

                RecordUndoSnapshot();

                // JSON 파일 생성
                var textAsset = LocalizationEditorUtility.CreateLocaleJsonAsset(code);
                var locale = new Locale(code, displayName);
                _database.AddLocaleEntry(locale, textAsset);

                // 기존 키들에 대해 빈 값 추가
                var allKeys = _database.GetAllKeys();
                foreach (var key in allKeys)
                    _database.SetEntry(code, key, "");

                SaveDatabaseToJson(code);
                EditorUtility.SetDirty(_database);
                AssetDatabase.SaveAssets();

                RefreshAll();
            };

            UnityEditor.PopupWindow.Show(
                rootVisualElement.Q<Button>("btn-add-language").worldBound,
                popup
            );
        }

        private void OnImportCSV()
        {
            string path = EditorUtility.OpenFilePanel("CSV 가져오기", "", "csv");
            if (string.IsNullOrEmpty(path)) return;

            RecordUndoSnapshot();
            CSVImporter.Import(path, _database);
            SaveAllLocalesToJson();
            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            RefreshAll();
        }

        private void OnExportCSV()
        {
            string path = EditorUtility.SaveFilePanel("CSV 내보내기", "", "localization", "csv");
            if (string.IsNullOrEmpty(path)) return;

            CSVImporter.Export(path, _database);
            EditorUtility.DisplayDialog("내보내기 완료", $"CSV를 다음 위치에 저장했습니다:\n{path}", "확인");
        }

        private void OnImportJSON()
        {
            string path = EditorUtility.OpenFolderPanel("JSON 폴더 가져오기", "", "");
            if (string.IsNullOrEmpty(path)) return;

            RecordUndoSnapshot();
            JSONImporter.ImportDirectory(path, _database);
            SaveAllLocalesToJson();
            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            RefreshAll();
        }

        private void OnExportJSON()
        {
            string path = EditorUtility.OpenFolderPanel("JSON 폴더 내보내기", "", "");
            if (string.IsNullOrEmpty(path)) return;

            JSONImporter.ExportAll(path, _database);
            EditorUtility.DisplayDialog("내보내기 완료", $"JSON 파일을 다음 위치에 저장했습니다:\n{path}", "확인");
        }

        private void OnGenerateKeys()
        {
            if (_settings.database == null)
            {
                EditorUtility.DisplayDialog("오류", "먼저 Locale Database를 설정하세요.", "확인");
                return;
            }

            LocalizationKeyGenerator.Generate(_settings);
            EditorUtility.DisplayDialog("완료", "키 상수 코드를 생성했습니다.", "확인");
        }

        #endregion

        #region Data Persistence

        private bool OnUndo()
        {
            if (_undoStack.Count == 0 || _database == null)
                return false;

            var current = CaptureSnapshot();
            var target = _undoStack.Pop();
            _redoStack.Push(current);
            ApplySnapshot(target);
            return true;
        }

        private bool OnRedo()
        {
            if (_redoStack.Count == 0 || _database == null)
                return false;

            var current = CaptureSnapshot();
            var target = _redoStack.Pop();
            _undoStack.Push(current);
            ApplySnapshot(target);
            return true;
        }

        private void OnEditorKeyDown(KeyDownEvent evt)
        {
            if (_database == null)
                return;

            if (evt.target is TextField)
                return;

            bool actionKey = evt.commandKey || evt.ctrlKey;
            if (!actionKey)
                return;

            if (evt.keyCode == KeyCode.Z)
            {
                bool handled;
                if (evt.shiftKey)
                    handled = OnRedo();
                else
                    handled = OnUndo();

                if (handled)
                {
                    evt.StopPropagation();
                    evt.PreventDefault();
                }
            }
            else if (evt.keyCode == KeyCode.Y)
            {
                if (OnRedo())
                {
                    evt.StopPropagation();
                    evt.PreventDefault();
                }
            }
        }

        private void RecordUndoSnapshot()
        {
            if (_database == null || _isApplyingHistory)
                return;

            _undoStack.Push(CaptureSnapshot());
            _redoStack.Clear();
            UpdateHistoryButtons();
        }

        private LocalizationSnapshot CaptureSnapshot()
        {
            var snapshot = new LocalizationSnapshot();
            if (_database == null)
                return snapshot;

            _database.InvalidateCache();
            _database.ParseJsonAssets();

            foreach (var entry in _database.Entries)
            {
                var localeSnapshot = new LocaleSnapshot
                {
                    code = entry.locale.Code,
                    displayName = entry.locale.DisplayName,
                    assetPath = entry.jsonAsset != null
                        ? AssetDatabase.GetAssetPath(entry.jsonAsset)
                        : $"Assets/Resources/Locales/{entry.locale.Code}.json"
                };

                var data = _database.GetLocaleData(entry.locale.Code);
                if (data != null)
                    localeSnapshot.data = new Dictionary<string, string>(data);

                snapshot.locales.Add(localeSnapshot);
            }

            return snapshot;
        }

        private void ApplySnapshot(LocalizationSnapshot snapshot)
        {
            if (_database == null || snapshot == null)
                return;

            _isApplyingHistory = true;

            try
            {
                var existingEntries = _database.Entries.ToList();
                var snapshotCodes = new HashSet<string>(
                    snapshot.locales.Select(locale => locale.code),
                    System.StringComparer.OrdinalIgnoreCase);

                foreach (var entry in existingEntries)
                {
                    if (snapshotCodes.Contains(entry.locale.Code))
                        continue;

                    string assetPath = entry.jsonAsset != null ? AssetDatabase.GetAssetPath(entry.jsonAsset) : null;
                    if (!string.IsNullOrEmpty(assetPath))
                        AssetDatabase.DeleteAsset(assetPath);
                }

                foreach (var entry in existingEntries)
                    _database.RemoveLocaleEntry(entry.locale.Code);

                foreach (var locale in snapshot.locales)
                {
                    string assetPath = EnsureLocaleAsset(locale);
                    string json = SimpleJsonParser.Serialize(locale.data);
                    File.WriteAllText(Path.GetFullPath(assetPath), json);
                    AssetDatabase.ImportAsset(assetPath);

                    var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    _database.AddLocaleEntry(new Locale(locale.code, locale.displayName), textAsset);
                }

                _database.InvalidateCache();
                _database.ParseJsonAssets();

                EditorUtility.SetDirty(_database);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                RefreshAll();
            }
            finally
            {
                _isApplyingHistory = false;
                UpdateHistoryButtons();
            }
        }

        private string EnsureLocaleAsset(LocaleSnapshot locale)
        {
            string assetPath = string.IsNullOrEmpty(locale.assetPath)
                ? $"Assets/Resources/Locales/{locale.code}.json"
                : locale.assetPath.Replace("\\", "/");

            LocalizationEditorUtility.EnsureDirectoryExists(Path.GetDirectoryName(assetPath));

            string fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
                File.WriteAllText(fullPath, "{}");

            return assetPath;
        }

        private void UpdateHistoryButtons()
        {
            _undoButton?.SetEnabled(_database != null && _undoStack.Count > 0);
            _redoButton?.SetEnabled(_database != null && _redoStack.Count > 0);
        }

        private void SaveDatabaseToJson(string localeCode)
        {
            if (_database == null) return;

            var entry = _database.Entries.FirstOrDefault(
                e => string.Equals(e.locale.Code, localeCode, System.StringComparison.OrdinalIgnoreCase));

            if (entry?.jsonAsset == null) return;

            var data = _database.GetLocaleData(localeCode);
            if (data != null)
                LocalizationEditorUtility.SaveLocaleJson(entry.jsonAsset, data);
        }

        private void SaveAllLocalesToJson()
        {
            if (_database == null) return;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var entry in _database.Entries)
                {
                    if (entry.jsonAsset == null) continue;

                    var data = _database.GetLocaleData(entry.locale.Code);
                    if (data != null)
                        LocalizationEditorUtility.SaveLocaleJson(entry.jsonAsset, data);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }

        #endregion

        #region Fallback UI

        private void BuildFallbackUI()
        {
            var root = rootVisualElement;

            // Toolbar 1
            var toolbar1 = new VisualElement();
            toolbar1.AddToClassList("toolbar");

            _searchField = new TextField { name = "search-field" };
            _searchField.style.flexGrow = 1;
            _searchField.style.minWidth = 200;
            toolbar1.Add(_searchField);

            _languageFilter = new DropdownField { name = "language-filter" };
            _languageFilter.style.minWidth = 120;
            toolbar1.Add(_languageFilter);

            toolbar1.Add(CreateSeparator());
            toolbar1.Add(CreateButton("btn-add-key", "+ 키", "btn-primary"));
            toolbar1.Add(CreateButton("btn-add-language", "+ 언어", "btn-primary"));
            root.Add(toolbar1);

            // Toolbar 2
            var toolbar2 = new VisualElement();
            toolbar2.AddToClassList("toolbar");
            toolbar2.Add(CreateButton("btn-import-csv", "CSV 가져오기", "btn-secondary"));
            toolbar2.Add(CreateButton("btn-export-csv", "CSV 내보내기", "btn-secondary"));
            toolbar2.Add(CreateSeparator());
            toolbar2.Add(CreateButton("btn-import-json", "JSON 가져오기", "btn-secondary"));
            toolbar2.Add(CreateButton("btn-export-json", "JSON 내보내기", "btn-secondary"));
            toolbar2.Add(CreateSeparator());
            toolbar2.Add(CreateButton("btn-undo", "실행 취소", "btn-secondary"));
            toolbar2.Add(CreateButton("btn-redo", "다시 실행", "btn-secondary"));
            var spacer = new VisualElement();
            spacer.AddToClassList("spacer");
            toolbar2.Add(spacer);
            toolbar2.Add(CreateButton("btn-generate-keys", "키 상수 생성", "btn-primary"));
            root.Add(toolbar2);

            // Table container
            _tableContainer = new VisualElement { name = "table-container" };
            _tableContainer.style.flexGrow = 1;
            root.Add(_tableContainer);

            // Empty state
            _emptyState = new VisualElement { name = "empty-state" };
            _emptyState.style.flexGrow = 1;
            _emptyState.style.alignItems = Align.Center;
            _emptyState.style.justifyContent = Justify.Center;
            _emptyState.style.display = DisplayStyle.None;
            _emptyState.Add(new Label("LocaleDatabase가 설정되지 않았습니다.") { style = { fontSize = 14 } });
            _emptyState.Add(new Label("Project Settings > AchEngine > Localization에서 먼저 설정하세요."));
            root.Add(_emptyState);

            // Footer
            var footer = new VisualElement();
            footer.AddToClassList("footer");
            _statsLabel = new Label { name = "stats-label", text = "키: 0 | 언어: 0 | 미번역: 0" };
            footer.Add(_statsLabel);
            var footerSpacer = new VisualElement();
            footerSpacer.AddToClassList("spacer");
            footer.Add(footerSpacer);
            _previewLocale = new DropdownField { name = "preview-locale", label = "미리보기" };
            _previewLocale.style.minWidth = 140;
            footer.Add(_previewLocale);
            root.Add(footer);
        }

        private Button CreateButton(string name, string text, string className)
        {
            var btn = new Button { name = name, text = text };
            btn.AddToClassList(className);
            return btn;
        }

        private VisualElement CreateSeparator()
        {
            var sep = new VisualElement();
            sep.AddToClassList("separator");
            return sep;
        }

        #endregion
    }

    /// <summary>
    /// 키 추가 팝업
    /// </summary>
    public class AddKeyPopup : PopupWindowContent
    {
        public event System.Action<string> OnConfirm;
        private string _key = "";

        public override Vector2 GetWindowSize() => new Vector2(300, 80);

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField("새 키 추가", EditorStyles.boldLabel);
            _key = EditorGUILayout.TextField("키", _key);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("취소", GUILayout.Width(60)))
                editorWindow.Close();
            if (GUILayout.Button("추가", GUILayout.Width(60)))
            {
                OnConfirm?.Invoke(_key);
                editorWindow.Close();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// 언어 추가 팝업
    /// </summary>
    public class AddLanguagePopup : PopupWindowContent
    {
        public event System.Action<string, string> OnConfirm;
        private string _code = "";
        private string _displayName = "";

        public override Vector2 GetWindowSize() => new Vector2(300, 100);

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField("새 언어 추가", EditorStyles.boldLabel);
            _code = EditorGUILayout.TextField("코드 (예: ko)", _code);
            _displayName = EditorGUILayout.TextField("표시 이름", _displayName);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("취소", GUILayout.Width(60)))
                editorWindow.Close();
            if (GUILayout.Button("추가", GUILayout.Width(60)))
            {
                OnConfirm?.Invoke(_code, _displayName);
                editorWindow.Close();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
