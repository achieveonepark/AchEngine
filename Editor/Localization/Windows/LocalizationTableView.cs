using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Localization.Editor
{
    /// <summary>
    /// MultiColumnListView를 래핑한 localization 테이블 뷰.
    /// 키 × 언어 형태의 테이블을 표시하고 인라인 편집을 지원.
    /// </summary>
    public class LocalizationTableView : VisualElement
    {
        private MultiColumnListView _listView;
        private LocaleDatabase _database;
        private List<string> _allKeys;
        private List<string> _filteredKeys;
        private List<string> _localeCodes;
        private List<Locale> _locales;
        private string _searchFilter = "";

        /// <summary>셀 값 변경 시 (key, localeCode, newValue)</summary>
        public event Action<string, string, string> CellValueChanged;

        /// <summary>키 삭제 요청 시</summary>
        public event Action<string> KeyDeleteRequested;

        /// <summary>첫 키 추가 요청 시</summary>
        public event Action AddKeyRequested;

        /// <summary>첫 언어 추가 요청 시</summary>
        public event Action AddLanguageRequested;

        public LocalizationTableView()
        {
            style.flexGrow = 1;
        }

        /// <summary>
        /// 데이터베이스를 설정하고 테이블을 구축
        /// </summary>
        public void SetData(LocaleDatabase database)
        {
            _database = database;

            if (_database == null)
            {
                Clear();
                return;
            }

            _database.InvalidateCache();
            _database.ParseJsonAssets();

            _allKeys = _database.GetAllKeys();
            _locales = _database.GetAllLocales();
            _localeCodes = _database.GetAllLocaleCodes();

            ApplyFilter();
            RebuildListView();
        }

        /// <summary>
        /// 검색 필터 적용
        /// </summary>
        public void SetFilter(string searchText)
        {
            _searchFilter = searchText ?? "";
            ApplyFilter();

            if (_listView != null)
            {
                _listView.itemsSource = _filteredKeys;
                _listView.RefreshItems();
            }
        }

        /// <summary>
        /// 데이터를 다시 로드하고 테이블 갱신
        /// </summary>
        public void RefreshView()
        {
            if (_database == null) return;

            _database.InvalidateCache();
            _database.ParseJsonAssets();
            _allKeys = _database.GetAllKeys();
            _locales = _database.GetAllLocales();
            _localeCodes = _database.GetAllLocaleCodes();

            ApplyFilter();
            RebuildListView();
        }

        /// <summary>
        /// 통계 정보 반환 (키 수, 언어 수, 누락 수)
        /// </summary>
        public (int keyCount, int localeCount, int missingCount) GetStats()
        {
            if (_database == null) return (0, 0, 0);

            int missing = 0;
            foreach (var key in _allKeys)
            {
                foreach (var code in _localeCodes)
                {
                    if (!_database.TryGetValue(code, key, out var val) || string.IsNullOrEmpty(val))
                        missing++;
                }
            }

            return (_allKeys.Count, _localeCodes.Count, missing);
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrEmpty(_searchFilter))
            {
                _filteredKeys = new List<string>(_allKeys);
            }
            else
            {
                _filteredKeys = _allKeys
                    .Where(k => k.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }
        }

        private void RebuildListView()
        {
            Clear();

            if (_localeCodes == null || _localeCodes.Count == 0)
            {
                Add(CreateEmptyState(
                    "No languages added yet",
                    "Add your first language to create locale files and start entering translations.",
                    "+ Language",
                    () => AddLanguageRequested?.Invoke()));
                return;
            }

            if (_allKeys == null || _allKeys.Count == 0)
            {
                Add(CreateEmptyState(
                    "No localization keys yet",
                    "Create your first key, then fill each locale value directly in the table.",
                    "+ Key",
                    () => AddKeyRequested?.Invoke()));
                return;
            }

            if (_filteredKeys == null || _filteredKeys.Count == 0)
            {
                Add(CreateEmptyState(
                    "No matching keys",
                    "Try a different search term or clear the filter to see all localization entries."));
                return;
            }

            // 컬럼 정의
            var columns = new Columns();

            // Key 컬럼
            var keyColumn = new Column
            {
                name = "key",
                title = "Key",
                width = 220,
                minWidth = 150,
            };
            columns.Add(keyColumn);

            // 각 locale별 컬럼
            for (int i = 0; i < _locales.Count; i++)
            {
                var locale = _locales[i];
                var col = new Column
                {
                    name = locale.Code,
                    title = $"{locale.DisplayName} ({locale.Code})",
                    width = 200,
                    minWidth = 120,
                    stretchable = true,
                };
                columns.Add(col);
            }

            // 삭제 버튼 컬럼
            var deleteColumn = new Column
            {
                name = "delete",
                title = "",
                width = 30,
                maxWidth = 30,
                minWidth = 30,
            };
            columns.Add(deleteColumn);

            _listView = new MultiColumnListView(columns)
            {
                itemsSource = _filteredKeys,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                sortingMode = ColumnSortingMode.Default,
                selectionType = SelectionType.Single,
            };

            _listView.style.flexGrow = 1;

            // Key 컬럼 바인딩
            _listView.columns["key"].makeCell = () =>
            {
                var label = new Label();
                label.AddToClassList("key-cell");
                return label;
            };
            _listView.columns["key"].bindCell = (element, index) =>
            {
                var label = (Label)element;
                label.text = _filteredKeys[index];
            };

            // 각 locale 컬럼 바인딩
            foreach (var locale in _locales)
            {
                string localeCode = locale.Code;

                _listView.columns[localeCode].makeCell = () =>
                {
                    var container = new VisualElement();
                    container.AddToClassList("value-cell");
                    var textField = new TextField();
                    textField.multiline = false;
                    textField.isDelayed = true;
                    container.Add(textField);
                    return container;
                };

                _listView.columns[localeCode].bindCell = (element, index) =>
                {
                    var textField = element.Q<TextField>();
                    string key = _filteredKeys[index];
                    string code = localeCode;

                    _database.TryGetValue(code, key, out var value);
                    textField.SetValueWithoutNotify(value ?? "");

                    // 누락 표시
                    if (string.IsNullOrEmpty(value))
                        element.AddToClassList("table-cell-missing");
                    else
                        element.RemoveFromClassList("table-cell-missing");

                    // 이벤트 등록 (중복 방지를 위해 userData에 저장)
                    if (textField.userData is EventCallback<ChangeEvent<string>> oldCallback)
                        textField.UnregisterValueChangedCallback(oldCallback);

                    EventCallback<ChangeEvent<string>> callback = evt =>
                    {
                        CellValueChanged?.Invoke(key, code, evt.newValue);

                        if (string.IsNullOrEmpty(evt.newValue))
                            element.AddToClassList("table-cell-missing");
                        else
                            element.RemoveFromClassList("table-cell-missing");
                    };

                    textField.RegisterValueChangedCallback(callback);
                    textField.userData = callback;
                };
            }

            // 삭제 버튼 컬럼 바인딩
            _listView.columns["delete"].makeCell = () =>
            {
                var btn = new Button { text = "\u00d7" }; // multiplication sign ×
                btn.AddToClassList("row-delete-btn");
                return btn;
            };
            _listView.columns["delete"].bindCell = (element, index) =>
            {
                var btn = (Button)element;
                string key = _filteredKeys[index];

                btn.clickable = new Clickable(() =>
                {
                    KeyDeleteRequested?.Invoke(key);
                });
            };

            Add(_listView);
        }

        private VisualElement CreateEmptyState(
            string title,
            string message,
            string buttonText = null,
            Action buttonAction = null)
        {
            var container = new VisualElement();
            container.AddToClassList("table-empty-state");

            var titleLabel = new Label(title);
            titleLabel.AddToClassList("table-empty-title");
            container.Add(titleLabel);

            var messageLabel = new Label(message);
            messageLabel.AddToClassList("table-empty-message");
            container.Add(messageLabel);

            if (!string.IsNullOrEmpty(buttonText) && buttonAction != null)
            {
                var actions = new VisualElement();
                actions.AddToClassList("table-empty-actions");

                var button = new Button(buttonAction) { text = buttonText };
                button.AddToClassList("btn-primary");
                actions.Add(button);

                container.Add(actions);
            }

            return container;
        }
    }
}
