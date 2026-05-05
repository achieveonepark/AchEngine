using System;
using System.IO;
using AchEngine.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor.UI
{
    public sealed class AchEngineUIWorkspaceWindow : EditorWindow
    {
        private UIViewCatalog catalog;
        private SerializedObject catalogSerializedObject;
        private SerializedProperty entriesProperty;

        private VisualElement panelGuide;
        private VisualElement panelCatalog;
        private VisualElement panelScene;

        private ObjectField catalogField;
        private Label catalogStatusLabel;
        private VisualElement entriesContainer;
        private VisualElement validationContainer;
        private Button addEntryButton;
        private Button addSelectedButton;

        private ObjectField rootField;
        private ObjectField bootstrapperField;
        private Label sceneStatusLabel;
        private Button createBootstrapperButton;
        private Button assignCatalogButton;

        public static void Open(UIViewCatalog initialCatalog)
        {
            var window = GetWindow<AchEngineUIWorkspaceWindow>();
            window.titleContent = new GUIContent("UI Workspace");
            window.minSize = new Vector2(480f, 560f);
            window.Show();
            window.SetCatalog(initialCatalog);
        }

        private void OnEnable()
        {
            Selection.selectionChanged += HandleSelectionChanged;
            Undo.undoRedoPerformed += HandleUndoRedo;
            EditorApplication.hierarchyChanged += HandleHierarchyChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= HandleSelectionChanged;
            Undo.undoRedoPerformed -= HandleUndoRedo;
            EditorApplication.hierarchyChanged -= HandleHierarchyChanged;
        }

        public void CreateGUI()
        {
            var uxmlPath = FindAssetPath<VisualTreeAsset>("AchEngineUIWorkspaceWindow");
            var ussPath = FindAssetPath<StyleSheet>("AchEngineUIWorkspaceWindow");

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

            panelGuide = rootVisualElement.Q("panel-guide");
            panelCatalog = rootVisualElement.Q("panel-catalog");
            panelScene = rootVisualElement.Q("panel-scene");

            SetupTabs();
            SetupCatalogPanel();
            SetupScenePanel();

            SetCatalog(catalog ?? Selection.activeObject as UIViewCatalog);
            RefreshSceneSection();
        }

        private void SetupTabs()
        {
            rootVisualElement.Q<Button>("tab-guide")?.RegisterCallback<ClickEvent>(_ => SwitchTab("tab-guide", panelGuide));
            rootVisualElement.Q<Button>("tab-catalog")?.RegisterCallback<ClickEvent>(_ => SwitchTab("tab-catalog", panelCatalog));
            rootVisualElement.Q<Button>("tab-scene")?.RegisterCallback<ClickEvent>(_ => SwitchTab("tab-scene", panelScene));
        }

        private void SwitchTab(string activeTabName, VisualElement activePanel)
        {
            string[] tabNames = { "tab-guide", "tab-catalog", "tab-scene" };
            VisualElement[] panels = { panelGuide, panelCatalog, panelScene };

            for (int i = 0; i < tabNames.Length; i++)
            {
                var button = rootVisualElement.Q<Button>(tabNames[i]);
                if (tabNames[i] == activeTabName)
                {
                    button?.AddToClassList("tab-active");
                    panels[i]?.RemoveFromClassList("panel-hidden");
                }
                else
                {
                    button?.RemoveFromClassList("tab-active");
                    panels[i]?.AddToClassList("panel-hidden");
                }
            }
        }

        private void SetupCatalogPanel()
        {
            var catalogFieldRow = rootVisualElement.Q("catalog-field-row");
            if (catalogFieldRow != null)
            {
                catalogField = new ObjectField("카탈로그")
                {
                    objectType = typeof(UIViewCatalog),
                    allowSceneObjects = false
                };
                catalogField.style.flexGrow = 1f;
                catalogField.style.marginRight = 6f;
                catalogField.RegisterValueChangedCallback(evt => SetCatalog(evt.newValue as UIViewCatalog));

                var createButton = new Button(CreateCatalog) { text = "새로 만들기" };
                createButton.AddToClassList("btn-secondary");

                catalogFieldRow.Add(catalogField);
                catalogFieldRow.Add(createButton);
            }

            var catalogActionRow = rootVisualElement.Q("catalog-action-row");
            if (catalogActionRow != null)
            {
                addEntryButton = new Button(AddEmptyEntry) { text = "+ 엔트리 추가" };
                addEntryButton.AddToClassList("btn-primary");

                addSelectedButton = new Button(AddSelectedPrefabs) { text = "선택 프리팹 가져오기" };
                addSelectedButton.AddToClassList("btn-secondary");

                var validateButton = new Button(RefreshValidation) { text = "검사" };
                validateButton.AddToClassList("btn-secondary");

                catalogActionRow.Add(addEntryButton);
                catalogActionRow.Add(addSelectedButton);
                catalogActionRow.Add(validateButton);
            }

            catalogStatusLabel = rootVisualElement.Q<Label>("label-catalog-status");
            entriesContainer = rootVisualElement.Q("entries-container");
            validationContainer = rootVisualElement.Q("validation-container");
        }

        private void SetupScenePanel()
        {
            var fieldsContainer = rootVisualElement.Q("scene-fields-container");
            if (fieldsContainer != null)
            {
                rootField = new ObjectField("UI 루트")
                {
                    objectType = typeof(UIRoot),
                    allowSceneObjects = true
                };
                rootField.SetEnabled(false);
                rootField.style.marginBottom = 6f;

                bootstrapperField = new ObjectField("부트스트래퍼")
                {
                    objectType = typeof(UIBootstrapper),
                    allowSceneObjects = true
                };
                bootstrapperField.SetEnabled(false);
                bootstrapperField.style.marginBottom = 6f;

                fieldsContainer.Add(rootField);
                fieldsContainer.Add(bootstrapperField);
            }

            var sceneActionRow = rootVisualElement.Q("scene-action-row");
            if (sceneActionRow != null)
            {
                var createRootButton = new Button(EnsureRootInScene) { text = "UI 루트 생성" };
                createRootButton.AddToClassList("btn-primary");

                createBootstrapperButton = new Button(EnsureBootstrapperInScene) { text = "부트스트래퍼 생성" };
                createBootstrapperButton.AddToClassList("btn-primary");

                assignCatalogButton = new Button(AssignCatalogToBootstrapper) { text = "카탈로그 할당" };
                assignCatalogButton.AddToClassList("btn-secondary");

                sceneActionRow.Add(createRootButton);
                sceneActionRow.Add(createBootstrapperButton);
                sceneActionRow.Add(assignCatalogButton);
            }

            sceneStatusLabel = rootVisualElement.Q<Label>("label-scene-status");
        }

        private void CreateCatalog()
        {
            SetCatalog(AchEngineUIEditorUtility.CreateViewCatalogAsset());
        }

        private void AddEmptyEntry()
        {
            if (catalog == null) return;

            ModifyCatalog(() =>
            {
                var newIndex = entriesProperty.arraySize;
                entriesProperty.InsertArrayElementAtIndex(newIndex);
                InitializeEntry(entriesProperty.GetArrayElementAtIndex(newIndex), AchEngineUIEditorUtility.MakeUniqueId(catalog, "NewView"), null);
            });
        }

        private void AddSelectedPrefabs()
        {
            if (catalog == null) return;

            var count = AchEngineUIEditorUtility.AddSelectedViewPrefabs(catalog);
            catalogSerializedObject = new SerializedObject(catalog);
            entriesProperty = catalogSerializedObject.FindProperty("entries");
            RefreshEntries();
            RefreshValidation();
            ShowNotification(new GUIContent(count > 0
                ? $"{count}개의 프리팹을 추가했습니다."
                : "먼저 하나 이상의 UIView 프리팹을 선택하세요."));
        }

        private void SetCatalog(UIViewCatalog newCatalog)
        {
            catalog = newCatalog;
            catalogSerializedObject = catalog != null ? new SerializedObject(catalog) : null;
            entriesProperty = catalogSerializedObject?.FindProperty("entries");

            catalogField?.SetValueWithoutNotify(catalog);

            RefreshCatalogStatus();
            RefreshEntries();
            RefreshValidation();
            RefreshSceneSection();
        }

        private void RefreshCatalogStatus()
        {
            if (catalogStatusLabel == null) return;

            if (catalog == null)
            {
                catalogStatusLabel.text = "편집할 UIViewCatalog를 선택하세요.";
                SetCatalogActionState(false);
                return;
            }

            var count = catalog.Entries.Count;
            catalogStatusLabel.text = count == 1
                ? $"'{catalog.name}' - 엔트리 1개"
                : $"'{catalog.name}' - 엔트리 {count}개";
            SetCatalogActionState(true);
        }

        private void SetCatalogActionState(bool enabled)
        {
            addEntryButton?.SetEnabled(enabled);
            addSelectedButton?.SetEnabled(enabled);
            createBootstrapperButton?.SetEnabled(enabled);
            assignCatalogButton?.SetEnabled(enabled && AchEngineUIEditorUtility.FindBootstrapperInOpenScenes() != null);
        }

        private void RefreshEntries()
        {
            if (entriesContainer == null) return;
            entriesContainer.Clear();

            if (catalog == null || entriesProperty == null)
            {
                entriesContainer.Add(MakeHelpBox("엔트리를 편집하려면 카탈로그를 선택하세요.", HelpBoxMessageType.Info));
                return;
            }

            catalogSerializedObject.Update();
            entriesProperty = catalogSerializedObject.FindProperty("entries");

            if (entriesProperty.arraySize == 0)
            {
                entriesContainer.Add(MakeHelpBox("아직 엔트리가 없습니다. '+ 엔트리 추가'를 누르거나 선택한 프리팹을 가져오세요.", HelpBoxMessageType.Info));
                return;
            }

            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                entriesContainer.Add(CreateEntryCard(i));
            }
        }

        private VisualElement CreateEntryCard(int index)
        {
            var entryProperty = entriesProperty.GetArrayElementAtIndex(index);
            var idProperty = entryProperty.FindPropertyRelative("id");
            var prefabProperty = entryProperty.FindPropertyRelative("prefab");
            var layerProperty = entryProperty.FindPropertyRelative("layer");
            var pooledProperty = entryProperty.FindPropertyRelative("pooled");
            var prewarmProperty = entryProperty.FindPropertyRelative("prewarmCount");
            var singleInstanceProperty = entryProperty.FindPropertyRelative("singleInstance");

            var card = new VisualElement();
            card.AddToClassList("entry-card");

            var header = new VisualElement();
            header.AddToClassList("entry-card-header");

            var title = new Label(string.IsNullOrWhiteSpace(idProperty.stringValue) ? $"엔트리 {index + 1}" : idProperty.stringValue);
            title.AddToClassList("entry-card-title");
            header.Add(title);

            var removeButton = new Button(() => RemoveEntry(index)) { text = "삭제" };
            removeButton.AddToClassList("btn-danger");
            header.Add(removeButton);
            card.Add(header);

            var idField = new TextField("뷰 ID")
            {
                value = idProperty.stringValue,
                isDelayed = true
            };
            idField.AddToClassList("entry-field");
            idField.RegisterValueChangedCallback(evt => ModifyCatalog(() => idProperty.stringValue = evt.newValue));
            card.Add(idField);

            var prefabField = new ObjectField("프리팹")
            {
                objectType = typeof(UIView),
                allowSceneObjects = false,
                value = prefabProperty.objectReferenceValue
            };
            prefabField.AddToClassList("entry-field");
            prefabField.RegisterValueChangedCallback(evt => ModifyCatalog(() =>
            {
                prefabProperty.objectReferenceValue = evt.newValue;
                if (string.IsNullOrWhiteSpace(idProperty.stringValue) && evt.newValue != null)
                {
                    idProperty.stringValue = AchEngineUIEditorUtility.MakeUniqueId(catalog, evt.newValue.name);
                }
            }));
            card.Add(prefabField);

            var layerField = new EnumField("레이어", (UILayerId)layerProperty.intValue);
            layerField.AddToClassList("entry-field");
            layerField.RegisterValueChangedCallback(evt => ModifyCatalog(() => layerProperty.intValue = Convert.ToInt32(evt.newValue)));
            card.Add(layerField);

            var toggleRow = new VisualElement();
            toggleRow.AddToClassList("entry-toggle-row");

            var pooledToggle = new Toggle("풀링 사용") { value = pooledProperty.boolValue };
            pooledToggle.style.marginRight = 16f;
            pooledToggle.RegisterValueChangedCallback(evt => ModifyCatalog(() =>
            {
                pooledProperty.boolValue = evt.newValue;
                if (!evt.newValue)
                {
                    prewarmProperty.intValue = 0;
                }
            }));
            toggleRow.Add(pooledToggle);

            var singleInstanceToggle = new Toggle("단일 인스턴스") { value = singleInstanceProperty.boolValue };
            singleInstanceToggle.RegisterValueChangedCallback(evt => ModifyCatalog(() => singleInstanceProperty.boolValue = evt.newValue));
            toggleRow.Add(singleInstanceToggle);
            card.Add(toggleRow);

            var prewarmField = new IntegerField("사전 생성 수")
            {
                value = prewarmProperty.intValue,
                isDelayed = true
            };
            prewarmField.AddToClassList("entry-field");
            prewarmField.SetEnabled(pooledProperty.boolValue);
            prewarmField.RegisterValueChangedCallback(evt => ModifyCatalog(() => prewarmProperty.intValue = Mathf.Max(0, evt.newValue)));
            card.Add(prewarmField);

            var warning = BuildEntryWarning(index, idProperty.stringValue, prefabProperty.objectReferenceValue as UIView, pooledProperty.boolValue, prewarmProperty.intValue);
            if (!string.IsNullOrEmpty(warning))
            {
                card.Add(MakeHelpBox(warning, HelpBoxMessageType.Warning));
            }

            return card;
        }

        private string BuildEntryWarning(int index, string id, UIView prefab, bool pooled, int prewarm)
        {
            if (string.IsNullOrWhiteSpace(id)) return $"엔트리 {index + 1}에 뷰 ID가 없습니다.";
            if (prefab == null) return $"'{id}'에 프리팹이 할당되지 않았습니다.";
            if (!pooled && prewarm > 0) return $"'{id}'는 사전 생성 수가 설정되어 있지만 풀링이 비활성화되어 있습니다.";
            return string.Empty;
        }

        private void RemoveEntry(int index)
        {
            if (catalog == null) return;
            ModifyCatalog(() => entriesProperty.DeleteArrayElementAtIndex(index));
        }

        private void ModifyCatalog(Action change)
        {
            if (catalog == null || catalogSerializedObject == null || entriesProperty == null) return;

            Undo.RecordObject(catalog, "UI 카탈로그 편집");
            catalogSerializedObject.Update();
            entriesProperty = catalogSerializedObject.FindProperty("entries");
            change();
            catalogSerializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(catalog);
            RefreshCatalogStatus();
            RefreshEntries();
            RefreshValidation();
        }

        private void InitializeEntry(SerializedProperty entryProperty, string id, UIView prefab)
        {
            entryProperty.FindPropertyRelative("id").stringValue = id;
            entryProperty.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            entryProperty.FindPropertyRelative("layer").intValue = (int)UILayerId.Screen;
            entryProperty.FindPropertyRelative("pooled").boolValue = true;
            entryProperty.FindPropertyRelative("prewarmCount").intValue = 0;
            entryProperty.FindPropertyRelative("singleInstance").boolValue = true;
        }

        private void RefreshValidation()
        {
            if (validationContainer == null) return;
            validationContainer.Clear();

            var issues = AchEngineUIEditorUtility.CollectCatalogIssues(catalog);
            if (issues.Count == 0)
            {
                var okLabel = new Label("문제가 없습니다");
                okLabel.AddToClassList("validation-ok");
                validationContainer.Add(okLabel);
                return;
            }

            foreach (var issue in issues)
            {
                var box = MakeHelpBox(issue.Message, ToHelpBoxMessageType(issue.MessageType));
                box.AddToClassList("validation-item");
                validationContainer.Add(box);
            }
        }

        private void EnsureRootInScene()
        {
            var existingRoot = AchEngineUIEditorUtility.FindUIRootInOpenScenes();
            if (existingRoot == null)
            {
                AchEngineUIEditorUtility.CreateUIRoot();
                ShowNotification(new GUIContent("UI 루트를 생성했습니다."));
            }
            else
            {
                Selection.activeObject = existingRoot.gameObject;
                ShowNotification(new GUIContent("기존 UI 루트를 사용합니다."));
            }

            RefreshSceneSection();
        }

        private void EnsureBootstrapperInScene()
        {
            if (catalog == null)
            {
                ShowNotification(new GUIContent("부트스트래퍼를 만들기 전에 카탈로그를 선택하세요."));
                return;
            }

            var existingRoot = AchEngineUIEditorUtility.FindUIRootInOpenScenes() ?? AchEngineUIEditorUtility.CreateUIRoot();
            var existingBootstrapper = AchEngineUIEditorUtility.FindBootstrapperInOpenScenes();

            if (existingBootstrapper == null)
            {
                AchEngineUIEditorUtility.CreateBootstrapper(catalog, existingRoot);
                ShowNotification(new GUIContent("부트스트래퍼를 생성했습니다."));
            }
            else
            {
                AchEngineUIEditorUtility.AssignBootstrapperReferences(existingBootstrapper, catalog, existingRoot);
                Selection.activeObject = existingBootstrapper.gameObject;
                ShowNotification(new GUIContent("부트스트래퍼를 업데이트했습니다."));
            }

            RefreshSceneSection();
        }

        private void AssignCatalogToBootstrapper()
        {
            var bootstrapper = AchEngineUIEditorUtility.FindBootstrapperInOpenScenes();
            if (bootstrapper == null || catalog == null) return;

            AchEngineUIEditorUtility.AssignBootstrapperReferences(bootstrapper, catalog, AchEngineUIEditorUtility.FindUIRootInOpenScenes());
            RefreshSceneSection();
            ShowNotification(new GUIContent("카탈로그를 부트스트래퍼에 연결했습니다."));
        }

        private void RefreshSceneSection()
        {
            var sceneRoot = AchEngineUIEditorUtility.FindUIRootInOpenScenes();
            var bootstrapper = AchEngineUIEditorUtility.FindBootstrapperInOpenScenes();
            var assignedCatalog = AchEngineUIEditorUtility.GetAssignedCatalog(bootstrapper);
            var assignedRoot = AchEngineUIEditorUtility.GetAssignedRoot(bootstrapper);

            rootField?.SetValueWithoutNotify(sceneRoot);
            bootstrapperField?.SetValueWithoutNotify(bootstrapper);

            if (sceneStatusLabel != null)
            {
                if (sceneRoot == null)
                {
                    sceneStatusLabel.text = "UI 루트를 찾지 못했습니다.";
                }
                else if (bootstrapper == null)
                {
                    sceneStatusLabel.text = $"UI 루트: {sceneRoot.name}. 부트스트래퍼를 찾지 못했습니다.";
                }
                else if (assignedCatalog == null)
                {
                    sceneStatusLabel.text = $"UI 루트: {sceneRoot.name}. 부트스트래퍼는 있지만 카탈로그가 연결되지 않았습니다.";
                }
                else
                {
                    var assignedRootName = assignedRoot != null ? assignedRoot.name : "없음";
                    sceneStatusLabel.text = $"UI 루트: {sceneRoot.name}. 카탈로그: {assignedCatalog.name}. 연결된 루트: {assignedRootName}.";
                }
            }

            createBootstrapperButton?.SetEnabled(catalog != null);
            assignCatalogButton?.SetEnabled(catalog != null && bootstrapper != null);
        }

        private void HandleSelectionChanged()
        {
            if (Selection.activeObject is UIViewCatalog selectedCatalog && selectedCatalog != catalog)
            {
                SetCatalog(selectedCatalog);
            }
        }

        private void HandleUndoRedo()
        {
            catalogSerializedObject?.Update();
            RefreshCatalogStatus();
            RefreshEntries();
            RefreshValidation();
            RefreshSceneSection();
        }

        private void HandleHierarchyChanged()
        {
            RefreshSceneSection();
        }

        private static HelpBox MakeHelpBox(string text, HelpBoxMessageType type)
        {
            var box = new HelpBox(text, type);
            box.style.flexShrink = 0f;
            return box;
        }

        private static HelpBoxMessageType ToHelpBoxMessageType(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Error => HelpBoxMessageType.Error,
                MessageType.Warning => HelpBoxMessageType.Warning,
                _ => HelpBoxMessageType.Info,
            };
        }

        private static string FindAssetPath<T>(string name) where T : UnityEngine.Object
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name} {name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == name)
                {
                    return path;
                }
            }

            return null;
        }
    }
}
