#if UNITY_6000_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AchEngine.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor.UI
{
    /// <summary>
    /// Rich UI Toolkit inspector for <see cref="UIViewCatalog"/>.
    /// Requires Unity 6000.3 or newer.
    /// </summary>
    [CustomEditor(typeof(UIViewCatalog))]
    public sealed class UIViewCatalogInspector : UnityEditor.Editor
    {
        private enum LayerFilter { All, Background, Screen, Popup, Overlay, Tooltip }

        private SerializedProperty _entriesProp;

        private VisualElement _root;
        private Label _countBadge;
        private Label _headerSubtitle;
        private VisualElement _dropZone;
        private ToolbarSearchField _searchField;
        private EnumField _layerFilterField;
        private ScrollView _listScroll;
        private Label _emptyLabel;
        private VisualElement _validationContainer;
        private VisualElement _footer;

        private string _filterText = string.Empty;
        private int _layerFilter = -1;
        private int _lastEntryCount = -1;

        public override VisualElement CreateInspectorGUI()
        {
            _entriesProp = serializedObject.FindProperty("entries");

            _root = new VisualElement();
            _root.AddToClassList("vc-root");

            var styleSheet = LoadStyleSheet();
            if (styleSheet != null)
            {
                _root.styleSheets.Add(styleSheet);
            }

            BuildHeader(_root);
            BuildActionBar(_root);
            BuildDropZone(_root);
            BuildToolbar(_root);
            BuildList(_root);
            BuildValidation(_root);
            BuildFooter(_root);

            _root.Bind(serializedObject);

            _root.TrackSerializedObjectValue(serializedObject, _ =>
            {
                if (serializedObject == null || serializedObject.targetObject == null)
                {
                    return;
                }

                if (_entriesProp.arraySize != _lastEntryCount)
                {
                    RefreshList();
                }

                UpdateHeader();
                RefreshValidation();
                RefreshFooter();
            });

            RefreshList();
            UpdateHeader();
            RefreshValidation();
            RefreshFooter();

            return _root;
        }

        // ------------------------------------------------------------
        // Build
        // ------------------------------------------------------------

        private void BuildHeader(VisualElement parent)
        {
            var header = new VisualElement();
            header.AddToClassList("vc-header");

            var icon = new VisualElement();
            icon.AddToClassList("vc-header-icon");
            var iconContent = EditorGUIUtility.IconContent("ScriptableObject Icon");
            if (iconContent?.image is Texture2D tex)
            {
                icon.style.backgroundImage = new StyleBackground(tex);
            }
            header.Add(icon);

            var col = new VisualElement();
            col.AddToClassList("vc-header-title-col");

            var title = new Label("UI View Catalog");
            title.AddToClassList("vc-header-title");
            col.Add(title);

            _headerSubtitle = new Label(string.Empty);
            _headerSubtitle.AddToClassList("vc-header-subtitle");
            col.Add(_headerSubtitle);

            header.Add(col);

            _countBadge = new Label("0");
            _countBadge.AddToClassList("vc-count-badge");
            header.Add(_countBadge);

            parent.Add(header);
        }

        private void BuildActionBar(VisualElement parent)
        {
            var bar = new VisualElement();
            bar.AddToClassList("vc-actions");

            bar.Add(MakeButton("+ Empty Entry", AddEmptyEntry, "vc-btn", "vc-btn-primary"));
            bar.Add(MakeButton("+ From Selection", AddFromSelection, "vc-btn"));
            bar.Add(MakeButton("Sort by Id", () => SortBy(EntrySortKey.Id), "vc-btn"));
            bar.Add(MakeButton("Sort by Layer", () => SortBy(EntrySortKey.Layer), "vc-btn"));
            bar.Add(MakeButton("Clean Empty", RemoveEmptyEntries, "vc-btn"));
            bar.Add(MakeButton("Open Workspace", OpenWorkspace, "vc-btn"));

            parent.Add(bar);
        }

        private void BuildDropZone(VisualElement parent)
        {
            _dropZone = new VisualElement();
            _dropZone.AddToClassList("vc-dropzone");
            _dropZone.Add(new Label("Drop UIView prefabs here to add them to the catalog."));

            _dropZone.RegisterCallback<DragEnterEvent>(_ =>
            {
                if (HasAcceptableDragPayload())
                {
                    _dropZone.AddToClassList("vc-dropzone--hover");
                }
            });
            _dropZone.RegisterCallback<DragLeaveEvent>(_ =>
                _dropZone.RemoveFromClassList("vc-dropzone--hover"));
            _dropZone.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = HasAcceptableDragPayload()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
                evt.StopPropagation();
            });
            _dropZone.RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.AcceptDrag();
                _dropZone.RemoveFromClassList("vc-dropzone--hover");
                var added = AddPrefabsFromDrag();
                _headerSubtitle.text = added > 0
                    ? $"{added} entry(ies) added via drag & drop."
                    : "No UIView prefabs were found in the drag payload.";
                evt.StopPropagation();
            });

            parent.Add(_dropZone);
        }

        private void BuildToolbar(VisualElement parent)
        {
            var bar = new VisualElement();
            bar.AddToClassList("vc-toolbar");

            _searchField = new ToolbarSearchField();
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _filterText = evt.newValue ?? string.Empty;
                RefreshList();
            });
            bar.Add(_searchField);

            _layerFilterField = new EnumField("Layer", LayerFilter.All);
            _layerFilterField.AddToClassList("vc-toolbar-filter");
            _layerFilterField.RegisterValueChangedCallback(evt =>
            {
                var value = (LayerFilter)evt.newValue;
                _layerFilter = value == LayerFilter.All ? -1 : (int)LayerFilterToId(value);
                RefreshList();
            });
            bar.Add(_layerFilterField);

            parent.Add(bar);
        }

        private void BuildList(VisualElement parent)
        {
            _listScroll = new ScrollView(ScrollViewMode.Vertical);
            _listScroll.AddToClassList("vc-list");
            parent.Add(_listScroll);

            _emptyLabel = new Label("No entries match the current filter.");
            _emptyLabel.AddToClassList("vc-list-empty");
            parent.Add(_emptyLabel);
        }

        private void BuildValidation(VisualElement parent)
        {
            _validationContainer = new VisualElement();
            _validationContainer.AddToClassList("vc-validation");
            parent.Add(_validationContainer);
        }

        private void BuildFooter(VisualElement parent)
        {
            _footer = new VisualElement();
            _footer.AddToClassList("vc-footer");
            parent.Add(_footer);
        }

        // ------------------------------------------------------------
        // Refresh
        // ------------------------------------------------------------

        private void UpdateHeader()
        {
            var count = _entriesProp.arraySize;
            _countBadge.text = count.ToString();
            var path = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(path))
            {
                _headerSubtitle.text = path;
            }
            else if (string.IsNullOrEmpty(_headerSubtitle.text))
            {
                _headerSubtitle.text = $"{count} entry(ies).";
            }
        }

        private void RefreshList()
        {
            _listScroll.Clear();
            _lastEntryCount = _entriesProp.arraySize;

            var visible = 0;
            for (var index = 0; index < _entriesProp.arraySize; index++)
            {
                var entryProp = _entriesProp.GetArrayElementAtIndex(index);
                if (!MatchesFilter(entryProp))
                {
                    continue;
                }

                _listScroll.Add(CreateEntryCard(entryProp, index));
                visible++;
            }

            _listScroll.style.display = visible == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            _emptyLabel.style.display = visible == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _emptyLabel.text = _entriesProp.arraySize == 0
                ? "This catalog has no entries yet. Use the buttons above or drag UIView prefabs into the drop zone."
                : "No entries match the current filter.";
        }

        private bool MatchesFilter(SerializedProperty entryProp)
        {
            var idProp = entryProp.FindPropertyRelative("id");
            var layerProp = entryProp.FindPropertyRelative("layer");
            var id = idProp != null ? idProp.stringValue : string.Empty;

            if (!string.IsNullOrEmpty(_filterText) &&
                (id == null || id.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) < 0))
            {
                return false;
            }

            if (_layerFilter >= 0 && (layerProp == null || layerProp.intValue != _layerFilter))
            {
                return false;
            }

            return true;
        }

        private VisualElement CreateEntryCard(SerializedProperty entryProp, int index)
        {
            var card = new VisualElement();
            card.AddToClassList("vc-entry");

            var idProp = entryProp.FindPropertyRelative("id");
            var layerProp = entryProp.FindPropertyRelative("layer");
            var prefabProp = entryProp.FindPropertyRelative("prefab");

            var statusClass = "vc-entry--ok";
            if (prefabProp.objectReferenceValue == null)
            {
                statusClass = "vc-entry--warn";
            }
            if (string.IsNullOrWhiteSpace(idProp.stringValue))
            {
                statusClass = "vc-entry--error";
            }
            card.AddToClassList(statusClass);

            var head = new VisualElement();
            head.AddToClassList("vc-entry-head");

            var expandToggle = new Toggle { value = false, tooltip = "Expand details" };
            expandToggle.AddToClassList("vc-entry-toggle");
            head.Add(expandToggle);

            var indexLabel = new Label($"#{index}");
            indexLabel.AddToClassList("vc-entry-index");
            head.Add(indexLabel);

            var titleLabel = new Label(string.IsNullOrWhiteSpace(idProp.stringValue) ? "<unnamed>" : idProp.stringValue);
            titleLabel.AddToClassList("vc-entry-title");
            head.Add(titleLabel);

            var layerPill = new Label(((UILayerId)layerProp.intValue).ToString());
            layerPill.AddToClassList("vc-entry-layer-pill");
            head.Add(layerPill);

            head.Add(MakeIconButton("\u25B2", () => MoveEntry(index, -1), "Move up"));
            head.Add(MakeIconButton("\u25BC", () => MoveEntry(index, 1), "Move down"));
            head.Add(MakeIconButton("\u29C9", () => DuplicateEntry(index), "Duplicate"));
            head.Add(MakeIconButton("\u25CE", () =>
            {
                if (prefabProp.objectReferenceValue != null)
                {
                    EditorGUIUtility.PingObject(prefabProp.objectReferenceValue);
                }
            }, "Ping prefab"));

            var removeButton = MakeIconButton("\u00D7", () => RemoveEntry(index), "Remove");
            removeButton.AddToClassList("vc-btn-danger");
            head.Add(removeButton);

            card.Add(head);

            var body = new VisualElement();
            body.AddToClassList("vc-entry-body");
            body.style.display = DisplayStyle.None;
            expandToggle.RegisterValueChangedCallback(evt =>
                body.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);

            body.Add(new PropertyField(idProp, "Id"));
            body.Add(new PropertyField(prefabProp, "Prefab"));
            body.Add(new PropertyField(layerProp, "Layer"));
            body.Add(new PropertyField(entryProp.FindPropertyRelative("pooled"), "Pooled"));
            body.Add(new PropertyField(entryProp.FindPropertyRelative("prewarmCount"), "Prewarm Count"));
            body.Add(new PropertyField(entryProp.FindPropertyRelative("singleInstance"), "Single Instance"));

            card.Add(body);

            // Live update the header row while the user edits fields inside the body.
            card.TrackPropertyValue(idProp, p =>
            {
                var value = p.stringValue;
                titleLabel.text = string.IsNullOrWhiteSpace(value) ? "<unnamed>" : value;
            });
            card.TrackPropertyValue(layerProp, p =>
                layerPill.text = ((UILayerId)p.intValue).ToString());

            card.Bind(serializedObject);
            return card;
        }

        private void RefreshValidation()
        {
            _validationContainer.Clear();
            var issues = AchEngineUIEditorUtility.CollectCatalogIssues(target as UIViewCatalog);
            foreach (var issue in issues)
            {
                _validationContainer.Add(new HelpBox(issue.Message, ToHelpBoxType(issue.MessageType)));
            }
        }

        private void RefreshFooter()
        {
            _footer.Clear();
            var total = _entriesProp.arraySize;
            _footer.Add(MakeStat($"Total: {total}", true));

            if (total == 0)
            {
                return;
            }

            var counts = new Dictionary<UILayerId, int>();
            for (var index = 0; index < total; index++)
            {
                var layerProp = _entriesProp.GetArrayElementAtIndex(index).FindPropertyRelative("layer");
                var layer = (UILayerId)layerProp.intValue;
                counts[layer] = counts.TryGetValue(layer, out var c) ? c + 1 : 1;
            }

            foreach (var kv in counts.OrderBy(k => (int)k.Key))
            {
                _footer.Add(MakeStat($"{kv.Key}: {kv.Value}", false));
            }
        }

        // ------------------------------------------------------------
        // Actions
        // ------------------------------------------------------------

        private void AddEmptyEntry()
        {
            var catalog = target as UIViewCatalog;
            if (catalog == null)
            {
                return;
            }

            serializedObject.Update();
            var newIndex = _entriesProp.arraySize;
            _entriesProp.InsertArrayElementAtIndex(newIndex);

            var newEntry = _entriesProp.GetArrayElementAtIndex(newIndex);
            newEntry.FindPropertyRelative("id").stringValue =
                AchEngineUIEditorUtility.MakeUniqueId(catalog, "NewView");
            newEntry.FindPropertyRelative("prefab").objectReferenceValue = null;
            newEntry.FindPropertyRelative("layer").intValue = (int)UILayerId.Screen;
            newEntry.FindPropertyRelative("pooled").boolValue = true;
            newEntry.FindPropertyRelative("prewarmCount").intValue = 0;
            newEntry.FindPropertyRelative("singleInstance").boolValue = true;
            serializedObject.ApplyModifiedProperties();
            RefreshList();
        }

        private void AddFromSelection()
        {
            var catalog = target as UIViewCatalog;
            if (catalog == null)
            {
                return;
            }

            var added = AchEngineUIEditorUtility.AddSelectedViewPrefabs(catalog);
            _headerSubtitle.text = added > 0
                ? $"{added} entry(ies) added from current selection."
                : "Select one or more UIView prefabs in the Project window first.";
            RefreshList();
        }

        private void OpenWorkspace()
        {
            AchEngineUIWorkspaceWindow.Open(target as UIViewCatalog);
        }

        private void RemoveEmptyEntries()
        {
            serializedObject.Update();
            var removed = 0;
            for (var index = _entriesProp.arraySize - 1; index >= 0; index--)
            {
                var entry = _entriesProp.GetArrayElementAtIndex(index);
                var id = entry.FindPropertyRelative("id").stringValue;
                var prefab = entry.FindPropertyRelative("prefab").objectReferenceValue;
                if (string.IsNullOrWhiteSpace(id) && prefab == null)
                {
                    _entriesProp.DeleteArrayElementAtIndex(index);
                    removed++;
                }
            }
            serializedObject.ApplyModifiedProperties();

            _headerSubtitle.text = removed > 0
                ? $"Removed {removed} empty entry(ies)."
                : "No empty entries to remove.";
            RefreshList();
        }

        private enum EntrySortKey { Id, Layer }

        private void SortBy(EntrySortKey key)
        {
            serializedObject.Update();

            var snapshots = new List<EntrySnapshot>(_entriesProp.arraySize);
            for (var index = 0; index < _entriesProp.arraySize; index++)
            {
                var entry = _entriesProp.GetArrayElementAtIndex(index);
                snapshots.Add(new EntrySnapshot
                {
                    Id = entry.FindPropertyRelative("id").stringValue,
                    Prefab = entry.FindPropertyRelative("prefab").objectReferenceValue,
                    Layer = entry.FindPropertyRelative("layer").intValue,
                    Pooled = entry.FindPropertyRelative("pooled").boolValue,
                    PrewarmCount = entry.FindPropertyRelative("prewarmCount").intValue,
                    SingleInstance = entry.FindPropertyRelative("singleInstance").boolValue,
                });
            }

            snapshots.Sort(key == EntrySortKey.Id
                ? (a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal)
                : (Comparison<EntrySnapshot>)((a, b) =>
                {
                    var cmp = a.Layer.CompareTo(b.Layer);
                    return cmp != 0 ? cmp : string.Compare(a.Id, b.Id, StringComparison.Ordinal);
                }));

            for (var index = 0; index < snapshots.Count; index++)
            {
                var entry = _entriesProp.GetArrayElementAtIndex(index);
                var snapshot = snapshots[index];
                entry.FindPropertyRelative("id").stringValue = snapshot.Id;
                entry.FindPropertyRelative("prefab").objectReferenceValue = snapshot.Prefab;
                entry.FindPropertyRelative("layer").intValue = snapshot.Layer;
                entry.FindPropertyRelative("pooled").boolValue = snapshot.Pooled;
                entry.FindPropertyRelative("prewarmCount").intValue = snapshot.PrewarmCount;
                entry.FindPropertyRelative("singleInstance").boolValue = snapshot.SingleInstance;
            }

            serializedObject.ApplyModifiedProperties();
            RefreshList();
        }

        private void MoveEntry(int from, int direction)
        {
            serializedObject.Update();
            var to = from + direction;
            if (to < 0 || to >= _entriesProp.arraySize)
            {
                return;
            }
            _entriesProp.MoveArrayElement(from, to);
            serializedObject.ApplyModifiedProperties();
            RefreshList();
        }

        private void DuplicateEntry(int index)
        {
            serializedObject.Update();
            _entriesProp.InsertArrayElementAtIndex(index);
            var copied = _entriesProp.GetArrayElementAtIndex(index + 1);
            var idProp = copied.FindPropertyRelative("id");
            idProp.stringValue = AchEngineUIEditorUtility.MakeUniqueId(target as UIViewCatalog, idProp.stringValue);
            serializedObject.ApplyModifiedProperties();
            RefreshList();
        }

        private void RemoveEntry(int index)
        {
            serializedObject.Update();
            _entriesProp.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            RefreshList();
        }

        // ------------------------------------------------------------
        // Drag & drop helpers
        // ------------------------------------------------------------

        private static bool HasAcceptableDragPayload()
        {
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is GameObject go && EditorUtility.IsPersistent(go) && go.GetComponent<UIView>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private int AddPrefabsFromDrag()
        {
            var catalog = target as UIViewCatalog;
            if (catalog == null)
            {
                return 0;
            }

            var previousSelection = Selection.objects;
            try
            {
                Selection.objects = DragAndDrop.objectReferences;
                return AchEngineUIEditorUtility.AddSelectedViewPrefabs(catalog);
            }
            finally
            {
                Selection.objects = previousSelection;
            }
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------

        private static Button MakeButton(string text, Action onClick, params string[] classes)
        {
            var button = new Button(onClick) { text = text };
            foreach (var c in classes)
            {
                button.AddToClassList(c);
            }
            return button;
        }

        private static Button MakeIconButton(string glyph, Action onClick, string tooltip)
        {
            var button = new Button(onClick) { text = glyph, tooltip = tooltip };
            button.AddToClassList("vc-btn");
            button.AddToClassList("vc-btn-icon");
            return button;
        }

        private static Label MakeStat(string text, bool accent)
        {
            var label = new Label(text);
            label.AddToClassList("vc-footer-stat");
            if (accent)
            {
                label.AddToClassList("vc-footer-stat--accent");
            }
            return label;
        }

        private static HelpBoxMessageType ToHelpBoxType(MessageType type) => type switch
        {
            MessageType.Info => HelpBoxMessageType.Info,
            MessageType.Warning => HelpBoxMessageType.Warning,
            MessageType.Error => HelpBoxMessageType.Error,
            _ => HelpBoxMessageType.None,
        };

        private static UILayerId LayerFilterToId(LayerFilter filter) => filter switch
        {
            LayerFilter.Background => UILayerId.Background,
            LayerFilter.Screen => UILayerId.Screen,
            LayerFilter.Popup => UILayerId.Popup,
            LayerFilter.Overlay => UILayerId.Overlay,
            LayerFilter.Tooltip => UILayerId.Tooltip,
            _ => UILayerId.Screen,
        };

        private StyleSheet LoadStyleSheet()
        {
            var script = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(script);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var directory = Path.GetDirectoryName(path)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(directory))
            {
                return null;
            }

            var ussPath = $"{directory}/UIViewCatalogInspector.uss";
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
        }

        private struct EntrySnapshot
        {
            public string Id;
            public UnityEngine.Object Prefab;
            public int Layer;
            public bool Pooled;
            public int PrewarmCount;
            public bool SingleInstance;
        }
    }
}
#else
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using AchEngine.UI;

namespace AchEngine.Editor.UI
{
    // Fallback: the polished inspector targets Unity 6000.3+ only.
    // On older editors we expose a plain default inspector so the asset stays editable.
    [CustomEditor(typeof(UIViewCatalog))]
    public sealed class UIViewCatalogInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new HelpBox(
                "The polished UIViewCatalog inspector requires Unity 6000.3 or newer.",
                HelpBoxMessageType.Info));
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            return root;
        }
    }
}
#endif
