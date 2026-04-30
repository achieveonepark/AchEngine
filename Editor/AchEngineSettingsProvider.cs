using System;
using System.Collections.Generic;
using System.Linq;
using AchEngine.Editor.Table;
using AchEngine.Editor.UI;
using AchEngine.Localization;
using AchEngine.Localization.Editor;
using AchEngine.UI;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor
{
    internal static class AchEngineSettingsProvider
    {
        // ─────────────────────────────────────────────
        // Root: Project/AchEngine → Overview
        // ─────────────────────────────────────────────
        [SettingsProvider]
        public static SettingsProvider CreateOverview()
        {
            return new SettingsProvider("Project/AchEngine", SettingsScope.Project)
            {
                label = "AchEngine",
                activateHandler = BuildOverviewPanel,
                keywords = new[]
                {
                    "achengine", "di", "vcontainer", "memorypack",
                    "table", "ui", "addressables", "localization"
                }
            };
        }

        // ─────────────────────────────────────────────
        // Sub: Project/AchEngine/Table Loader
        // ─────────────────────────────────────────────
        [SettingsProvider]
        public static SettingsProvider CreateTableLoader()
        {
            return new SettingsProvider("Project/AchEngine/Table Loader", SettingsScope.Project)
            {
                label = "Table Loader",
                activateHandler = (ctx, root) => BuildTablePanel(root),
                keywords = new[] { "table", "google sheets", "csv", "memorypack", "codegen", "bake" }
            };
        }

        // ─────────────────────────────────────────────
        // Sub: Project/AchEngine/UI Workspace
        // ─────────────────────────────────────────────
        [SettingsProvider]
        public static SettingsProvider CreateUIWorkspace()
        {
            return new SettingsProvider("Project/AchEngine/UI Workspace", SettingsScope.Project)
            {
                label = "UI Workspace",
                activateHandler = (ctx, root) => BuildUIPanel(root),
                keywords = new[] { "ui", "view", "catalog", "layer", "pool", "uiroot", "vcontainer" }
            };
        }

        internal static void BuildUIWorkspaceSettingsPanel(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            BuildUIPanel(root);
        }

        // ─────────────────────────────────────────────
        // Sub: Project/AchEngine/Localization
        // ─────────────────────────────────────────────
        [SettingsProvider]
        public static SettingsProvider CreateLocalization()
        {
            return new SettingsProvider("Project/AchEngine/Localization", SettingsScope.Project)
            {
                label = "Localization",
                activateHandler = (ctx, root) => BuildLocalizationPanel(root),
                keywords = new[]
                {
                    "achengine", "localization", "language", "locale",
                    "translation", "i18n", "codegen", "키", "다국어"
                }
            };
        }

        // ═════════════════════════════════════════════════════
        // Overview Panel
        // ═════════════════════════════════════════════════════

        private static void BuildOverviewPanel(string searchContext, VisualElement root)
        {
            AchEngineEditorUI.ApplyRootStyle(root);
            var scroll = AchEngineEditorUI.MakeScrollContent(root);

            // Header
            scroll.Add(AchEngineEditorUI.MakePageTitle("AchEngine", "1.0.0"));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "Unity 게임 개발을 위한 통합 툴킷. " +
                "VContainer 기반 DI, UI 관리, Addressables, Localization, " +
                "Google Sheets 테이블 파이프라인을 하나의 패키지로 제공합니다."));

            // Module Status
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSectionTitle("모듈"));

            bool hasVContainer   = Type.GetType("VContainer.LifetimeScope, VContainer") != null;
            bool hasMemoryPack   = Type.GetType("MemoryPack.MemoryPackSerializer, MemoryPack.Core") != null;
            bool hasAddressables = Type.GetType("UnityEngine.AddressableAssets.Addressables, Unity.Addressables") != null;
            bool hasTMP          = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro") != null;

            var moduleGrid = new VisualElement();
            moduleGrid.style.marginBottom = 8f;
            AddModuleRow(moduleGrid, "Table System",  "Google Sheets → CSV → Binary 파이프라인 + 타입-세이프 접근", true,            "Project/AchEngine/Table Loader");
            AddModuleRow(moduleGrid, "UI System",     "레이어 기반 View 관리, Object Pool, 트랜지션 지원",          true,            "Project/AchEngine/UI Workspace");
            AddModuleRow(moduleGrid, "Addressables",  "에셋 캐싱, 참조 카운팅, 씬 단위 수명 주기 관리",            hasAddressables, "Project/AchEngine/Addressables",
                () => Client.Add("com.unity.addressables"), "설치");
            AddModuleRow(moduleGrid, "Localization",  "JSON 기반 다국어, 타입-세이프 키 코드 생성",                 true,            "Project/AchEngine/Localization");
            AddModuleRow(moduleGrid, "VContainer DI", "AchEngineInstaller 래퍼, ServiceLocator 제공",               hasVContainer,   null,
                () => Application.OpenURL("https://github.com/hadashiA/VContainer"), "GitHub");
            AddModuleRow(moduleGrid, "MemoryPack",    "Binary 직렬화 (Table 고성능 로드)",                          hasMemoryPack,   null,
                () => Application.OpenURL("https://github.com/Cysharp/MemoryPack"), "GitHub");
            AddModuleRow(moduleGrid, "TextMeshPro",   "LocalizedText TMP 지원",                                     hasTMP,          null);
            scroll.Add(moduleGrid);

            // DI 사용법
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSectionTitle("DI 설정 (VContainer 래퍼)"));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "VContainer를 직접 다루지 않아도 됩니다. " +
                "AchEngineInstaller를 상속해 서비스를 등록하고, " +
                "AchEngineScope의 Installers 배열에 추가하세요."));
            scroll.Add(AchEngineEditorUI.MakeCodeBlock(
                "// 1. Installer 작성\n" +
                "public class GameInstaller : AchEngineInstaller\n" +
                "{\n" +
                "    public override void Install(IServiceBuilder builder)\n" +
                "    {\n" +
                "        builder.Register<IGameService, GameService>()\n" +
                "               .Register<IPlayerService, PlayerService>(ServiceLifetime.Transient);\n" +
                "    }\n" +
                "}\n\n" +
                "// 2. AchEngineScope 씬에 추가 → Installers에 GameInstaller 드래그\n\n" +
                "// 3. [Inject] 또는 ServiceLocator로 접근\n" +
                "var svc = ServiceLocator.Resolve<IGameService>();"));

            // 패키지 정보
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSectionTitle("패키지 정보"));

            var infoGrid = new VisualElement();
            infoGrid.style.marginBottom = 8f;
            infoGrid.Add(AchEngineEditorUI.MakeInfoRow("Package ID", "com.engine.achieve"));
            infoGrid.Add(AchEngineEditorUI.MakeInfoRow("Version",    "1.0.0"));
            infoGrid.Add(AchEngineEditorUI.MakeInfoRow("Unity",      "2021.3+"));
            infoGrid.Add(AchEngineEditorUI.MakeInfoRow("필수",        "com.unity.ugui"));
            infoGrid.Add(AchEngineEditorUI.MakeInfoRow("권장",        "jp.hadashikick.vcontainer"));
            infoGrid.Add(AchEngineEditorUI.MakeInfoRow("선택",        "com.cysharp.memorypack, com.unity.textmeshpro"));
            scroll.Add(infoGrid);

            // 에디터 메뉴
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSectionTitle("에디터 메뉴"));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "Tools > AchEngine > Table Loader\n" +
                "Tools > AchEngine > UI Workspace\n" +
                "Project Settings > AchEngine > Addressables"));
        }

        // ═════════════════════════════════════════════════════
        // Table Panel
        // ═════════════════════════════════════════════════════

        private static void BuildTablePanel(VisualElement root)
        {
            AchEngineEditorUI.ApplyRootStyle(root);
            var scroll = AchEngineEditorUI.MakeScrollContent(root);

            scroll.Add(AchEngineEditorUI.MakePageTitle("Table Loader", null));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "Google Sheets에서 CSV를 다운로드하고, " +
                "C# 데이터 클래스를 자동 생성하며, " +
                "MemoryPack (또는 JSON)으로 직렬화합니다.\n" +
                "설정은 Assets/TableLoaderSettings.asset에 저장됩니다."));

            var settings = TableLoaderSettings.GetOrCreate();
            var so = new SerializedObject(settings);

            // Google Spreadsheet
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("Google Spreadsheet"));

            var spreadsheetField = new TextField("Spreadsheet ID")
            {
                value = settings.spreadsheetId,
                isDelayed = true
            };
            spreadsheetField.style.marginBottom = 6f;
            spreadsheetField.RegisterValueChangedCallback(evt =>
            {
                so.Update();
                so.FindProperty("spreadsheetId").stringValue = evt.newValue;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
            });
            scroll.Add(spreadsheetField);

            var openButton = new Button(() =>
            {
                if (!string.IsNullOrEmpty(settings.spreadsheetId))
                    Application.OpenURL(settings.GetSpreadsheetUrl());
            }) { text = "브라우저에서 열기" };
            openButton.AddToClassList("btn-secondary");
            openButton.style.marginBottom = 12f;
            scroll.Add(openButton);

            // Paths
            scroll.Add(AchEngineEditorUI.MakeSubTitle("출력 경로"));
            scroll.Add(AchEngineEditorUI.MakeSerializedTextField(so, settings, "csvOutputPath",    "CSV 출력 경로"));
            scroll.Add(AchEngineEditorUI.MakeSerializedTextField(so, settings, "codeOutputPath",   "생성 코드 경로"));
            scroll.Add(AchEngineEditorUI.MakeSerializedTextField(so, settings, "binaryOutputPath", "바이너리 출력 경로"));

            // Automation
            scroll.Add(AchEngineEditorUI.MakeSubTitle("자동화"));
            scroll.Add(AchEngineEditorUI.MakeSerializedToggle(so, settings, "autoGenerateOnDownload", "다운로드 후 자동 코드 생성"));
            scroll.Add(AchEngineEditorUI.MakeSerializedToggle(so, settings, "autoBakeOnGenerate",     "코드 생성 후 자동 베이크"));

            // Sheets
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("시트 목록"));
            scroll.Add(AchEngineEditorUI.MakeBodyText("각 행: [활성] [시트명] [GID] [클래스명]"));

            var sheetContainer = new VisualElement();
            sheetContainer.style.marginBottom = 8f;

            Action refreshSheets = null;
            refreshSheets = () =>
            {
                sheetContainer.Clear();
                for (int i = 0; i < settings.sheets.Count; i++)
                {
                    var index = i;
                    var sheet = settings.sheets[i];
                    sheetContainer.Add(MakeSheetRow(sheet, () =>
                    {
                        settings.sheets.RemoveAt(index);
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                        refreshSheets?.Invoke();
                    }));
                }
            };
            refreshSheets();
            scroll.Add(sheetContainer);

            var addSheetButton = new Button(() =>
            {
                settings.sheets.Add(new SheetInfo());
                EditorUtility.SetDirty(settings);
                refreshSheets();
            }) { text = "+ 시트 추가" };
            addSheetButton.AddToClassList("btn-primary");
            addSheetButton.style.marginBottom = 12f;
            scroll.Add(addSheetButton);

            // Pipeline actions
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("파이프라인"));

            var pipelineRow = AchEngineEditorUI.MakeButtonRow();

            var saveButton = new Button(() =>
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }) { text = "설정 저장" };
            saveButton.AddToClassList("btn-primary");

            var openWindowButton = new Button(TableLoaderWindow.ShowWindow)
            { text = "Table Loader 창 열기" };
            openWindowButton.AddToClassList("btn-secondary");

            pipelineRow.Add(saveButton);
            pipelineRow.Add(openWindowButton);
            scroll.Add(pipelineRow);
        }

        // ═════════════════════════════════════════════════════
        // UI Panel
        // ═════════════════════════════════════════════════════

        private static void BuildUIPanel(VisualElement root)
        {
            AchEngineEditorUI.ApplyRootStyle(root);
            var scroll = AchEngineEditorUI.MakeScrollContent(root);

            scroll.Add(AchEngineEditorUI.MakePageTitle("UI Workspace", null));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "레이어 기반 UI 관리 시스템. " +
                "UIViewCatalog에 등록된 View를 ID 또는 타입으로 Show/Close합니다. " +
                "Object Pool, 트랜지션 애니메이션, Single Instance 모드를 지원합니다."));

            // Scene state
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("현재 씬 상태"));
            scroll.Add(AchEngineEditorUI.MakeStatusRow("UI Root",      GetSceneObjectName(typeof(UIRoot))));
            scroll.Add(AchEngineEditorUI.MakeStatusRow("Bootstrapper", GetSceneObjectName(typeof(UIBootstrapper))));

            // Quick actions
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("빠른 작업"));

            var actionRow = AchEngineEditorUI.MakeButtonRow();
            actionRow.style.marginBottom = 12f;

            var createRootBtn = new Button(() =>
            {
                if (AchEngineUIEditorUtility.FindUIRootInOpenScenes() == null)
                    AchEngineUIEditorUtility.CreateUIRoot();
            }) { text = "UI Root 생성" };
            createRootBtn.AddToClassList("btn-primary");

            var openWorkspaceBtn = new Button(() => AchEngineUIWorkspaceWindow.Open(null))
            { text = "UI Workspace 열기" };
            openWorkspaceBtn.AddToClassList("btn-secondary");

            actionRow.Add(createRootBtn);
            actionRow.Add(openWorkspaceBtn);
            scroll.Add(actionRow);

            // Layer order
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("레이어 렌더 순서"));

            var layerGrid = new VisualElement();
            layerGrid.style.marginBottom = 8f;
            layerGrid.Add(AchEngineEditorUI.MakeInfoRow("Background", "SortingOrder  0  — 배경 화면"));
            layerGrid.Add(AchEngineEditorUI.MakeInfoRow("Screen",     "SortingOrder 10  — 기본 화면"));
            layerGrid.Add(AchEngineEditorUI.MakeInfoRow("Popup",      "SortingOrder 20  — 팝업 / 다이얼로그"));
            layerGrid.Add(AchEngineEditorUI.MakeInfoRow("Overlay",    "SortingOrder 30  — 전체 오버레이"));
            layerGrid.Add(AchEngineEditorUI.MakeInfoRow("Tooltip",    "SortingOrder 40  — 툴팁"));
            scroll.Add(layerGrid);

            // UIView lifecycle
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("UIView 수명 주기"));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "OnInitialize()  →  최초 생성 시 1회\n" +
                "OnBeforeOpen()  →  Show() 직전\n" +
                "OnOpened()      →  트랜지션 완료 후\n" +
                "OnBeforeClose() →  Close() 직전\n" +
                "OnClosed()      →  트랜지션 완료 후 (Pool 반환)"));

            // DI 통합
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("DI 통합"));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "AchEngineScope를 씬에 추가하면 IUIService가 자동 등록됩니다."));
            scroll.Add(AchEngineEditorUI.MakeCodeBlock(
                "// [Inject] 사용\n" +
                "[Inject] readonly IUIService _ui;\n" +
                "_ui.Show(\"MainMenu\");\n\n" +
                "// ServiceLocator 사용 (MonoBehaviour 등)\n" +
                "ServiceLocator.Resolve<IUIService>().Show(\"GameHUD\");"));
        }

        // ═════════════════════════════════════════════════════
        // Localization Panel
        // ═════════════════════════════════════════════════════

        private static void BuildLocalizationPanel(VisualElement root)
        {
            AchEngineEditorUI.ApplyRootStyle(root);
            var scroll = AchEngineEditorUI.MakeScrollContent(root);

            scroll.Add(AchEngineEditorUI.MakePageTitle("Localization", null));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "JSON 기반 다국어 시스템. 로케일 전환, 폴백, 시스템 언어 자동 감지, " +
                "타입-세이프 키 상수 코드 생성을 지원합니다.\n" +
                "설정은 Assets/Resources/LocalizationSettings.asset에 저장됩니다."));

            var settings = LocalizationEditorUtility.GetOrCreateSettings();
            var so = new SerializedObject(settings);

            // ── 데이터베이스 ──────────────────────────────────
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("데이터베이스"));

            var dbField = new ObjectField("Locale Database")
            {
                objectType = typeof(LocaleDatabase),
                value      = settings.database
            };
            dbField.style.marginBottom = 8f;
            scroll.Add(dbField);

            var defaultDropdown  = new DropdownField("기본 로케일");
            var fallbackDropdown = new DropdownField("폴백 로케일");
            defaultDropdown.style.marginBottom  = 4f;
            fallbackDropdown.style.marginBottom = 4f;

            void RefreshLocaleDropdowns()
            {
                var choices = new List<string> { "(없음)" };
                if (settings.database != null)
                {
                    settings.database.InvalidateCache();
                    settings.database.ParseJsonAssets();
                    choices.AddRange(settings.database.GetAllLocales()
                        .Select(l => $"{l.DisplayName} ({l.Code})"));
                }
                defaultDropdown.choices  = choices;
                fallbackDropdown.choices = choices;
                defaultDropdown.index    = FindLocaleIndex(choices, settings.defaultLocaleCode);
                fallbackDropdown.index   = FindLocaleIndex(choices, settings.fallbackLocaleCode);
            }

            dbField.RegisterValueChangedCallback(evt =>
            {
                settings.database = evt.newValue as LocaleDatabase;
                SaveLocalizationSettings(settings);
                RefreshLocaleDropdowns();
            });

            var dbActionRow = AchEngineEditorUI.MakeButtonRow();
            dbActionRow.style.marginBottom = 4f;

            var createDbBtn = new Button(() =>
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Locale Database 생성", "LocaleDatabase", "asset", "저장 위치 선택");
                if (!string.IsNullOrEmpty(path))
                {
                    var db = LocalizationEditorUtility.CreateDatabase(path);
                    settings.database = db;
                    SaveLocalizationSettings(settings);
                    dbField.value = db;
                    RefreshLocaleDropdowns();
                }
            }) { text = "Database 생성" };

            var openEditorBtn = new Button(LocalizationEditorWindow.Open)
            { text = "편집기 열기" };
            openEditorBtn.AddToClassList("btn-secondary");

            dbActionRow.Add(createDbBtn);
            dbActionRow.Add(openEditorBtn);
            scroll.Add(dbActionRow);

            // ── 로케일 설정 ───────────────────────────────────
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("로케일 설정"));

            scroll.Add(defaultDropdown);
            scroll.Add(fallbackDropdown);

            defaultDropdown.RegisterValueChangedCallback(evt =>
            {
                var code = ExtractLocaleCode(evt.newValue);
                if (code != null) { settings.defaultLocaleCode = code; SaveLocalizationSettings(settings); }
            });
            fallbackDropdown.RegisterValueChangedCallback(evt =>
            {
                var code = ExtractLocaleCode(evt.newValue);
                if (code != null) { settings.fallbackLocaleCode = code; SaveLocalizationSettings(settings); }
            });

            RefreshLocaleDropdowns();

            var autoDetectToggle = new Toggle("시스템 언어 자동 감지")
            { value = settings.autoDetectSystemLanguage };
            autoDetectToggle.style.marginBottom = 4f;
            autoDetectToggle.RegisterValueChangedCallback(evt =>
            {
                settings.autoDetectSystemLanguage = evt.newValue;
                SaveLocalizationSettings(settings);
            });

            var autoInitToggle = new Toggle("앱 시작 시 자동 초기화")
            { value = settings.autoInitialize };
            autoInitToggle.style.marginBottom = 4f;
            autoInitToggle.RegisterValueChangedCallback(evt =>
            {
                settings.autoInitialize = evt.newValue;
                SaveLocalizationSettings(settings);
            });

            scroll.Add(autoDetectToggle);
            scroll.Add(autoInitToggle);

            // ── 코드 생성 ─────────────────────────────────────
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSubTitle("키 상수 코드 생성"));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "dot-notation 키를 타입-세이프 중첩 클래스로 변환합니다.\n" +
                "예: \"menu.start\"  →  L.Menu.Start"));

            scroll.Add(AchEngineEditorUI.MakeSerializedTextField(so, settings, "generatedClassName", "클래스 이름"));
            scroll.Add(AchEngineEditorUI.MakeSerializedTextField(so, settings, "generatedNamespace", "네임스페이스"));

            var pathRow = new VisualElement();
            pathRow.style.flexDirection = FlexDirection.Row;
            pathRow.style.marginBottom  = 8f;

            var outputPathField = new TextField("출력 경로")
            {
                value     = settings.generatedOutputPath,
                isDelayed = true
            };
            outputPathField.style.flexGrow    = 1f;
            outputPathField.style.marginRight = 4f;
            outputPathField.RegisterValueChangedCallback(evt =>
            {
                settings.generatedOutputPath = evt.newValue;
                SaveLocalizationSettings(settings);
            });

            var browseBtn = new Button(() =>
            {
                string selected = EditorUtility.OpenFolderPanel("출력 경로 선택", "Assets", "");
                if (!string.IsNullOrEmpty(selected))
                {
                    if (selected.StartsWith(Application.dataPath))
                        selected = "Assets" + selected.Substring(Application.dataPath.Length);
                    settings.generatedOutputPath = selected;
                    outputPathField.value        = selected;
                    SaveLocalizationSettings(settings);
                }
            }) { text = "..." };
            browseBtn.style.width      = 30f;
            browseBtn.style.flexShrink = 0f;
            browseBtn.style.alignSelf  = Align.FlexEnd;

            pathRow.Add(outputPathField);
            pathRow.Add(browseBtn);
            scroll.Add(pathRow);

            var generateBtn = new Button(() =>
            {
                if (settings.database == null)
                {
                    EditorUtility.DisplayDialog("오류", "Locale Database를 먼저 설정하세요.", "확인");
                    return;
                }
                LocalizationKeyGenerator.Generate(settings);
                EditorUtility.DisplayDialog("완료", "키 상수가 생성됐습니다.", "확인");
            }) { text = "키 상수 생성" };
            generateBtn.AddToClassList("btn-primary");
            scroll.Add(generateBtn);
        }

        // ═════════════════════════════════════════════════════
        // 모듈 행 (Overview 전용)
        // ═════════════════════════════════════════════════════

        private static void AddModuleRow(
            VisualElement parent,
            string moduleName,
            string description,
            bool installed,
            string settingsPath,
            Action onNotInstalled = null,
            string notInstalledBtnText = null)
        {
            var row = new VisualElement();
            row.style.flexDirection   = FlexDirection.Row;
            row.style.alignItems      = Align.Center;
            row.style.paddingTop      = 6f;
            row.style.paddingBottom   = 6f;
            row.style.paddingLeft     = 8f;
            row.style.paddingRight    = 8f;
            row.style.marginBottom    = 4f;
            row.style.backgroundColor = new StyleColor(AchEngineEditorUI.ColorSurface);
            row.style.borderTopLeftRadius     = 4f;
            row.style.borderTopRightRadius    = 4f;
            row.style.borderBottomLeftRadius  = 4f;
            row.style.borderBottomRightRadius = 4f;

            // Status dot
            var dot = new VisualElement();
            dot.style.width  = 8f;
            dot.style.height = 8f;
            dot.style.borderTopLeftRadius     = 4f;
            dot.style.borderTopRightRadius    = 4f;
            dot.style.borderBottomLeftRadius  = 4f;
            dot.style.borderBottomRightRadius = 4f;
            dot.style.backgroundColor = installed
                ? new StyleColor(AchEngineEditorUI.ColorGreen)
                : new StyleColor(AchEngineEditorUI.ColorTextMuted);
            dot.style.marginRight = 8f;
            dot.style.flexShrink  = 0f;
            row.Add(dot);

            // Name + description
            var textBlock = new VisualElement();
            textBlock.style.flexGrow = 1f;

            var nameLabel = new Label(moduleName);
            nameLabel.style.fontSize                = 12f;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color                   = new StyleColor(AchEngineEditorUI.ColorText);
            textBlock.Add(nameLabel);

            var descLabel = new Label(description);
            descLabel.style.fontSize   = 11f;
            descLabel.style.color      = new StyleColor(AchEngineEditorUI.ColorTextMuted);
            descLabel.style.whiteSpace = WhiteSpace.Normal;
            textBlock.Add(descLabel);

            row.Add(textBlock);

            // Right-side action
            if (settingsPath != null && installed)
            {
                var btn = new Button(() => SettingsService.OpenProjectSettings(settingsPath))
                { text = "설정" };
                btn.style.width      = 44f;
                btn.style.height     = 22f;
                btn.style.fontSize   = 11f;
                btn.style.marginLeft = 8f;
                btn.style.flexShrink = 0f;
                row.Add(btn);
            }
            else if (!installed)
            {
                if (onNotInstalled != null)
                {
                    var btn = new Button(onNotInstalled)
                    { text = notInstalledBtnText ?? "설치" };
                    btn.style.width             = 54f;
                    btn.style.height            = 22f;
                    btn.style.fontSize          = 11f;
                    btn.style.marginLeft        = 8f;
                    btn.style.flexShrink        = 0f;
                    btn.style.color             = new StyleColor(Color.white);
                    btn.style.backgroundColor   = new StyleColor(AchEngineEditorUI.ColorButtonBlue);
                    btn.style.borderTopWidth    = 0f;
                    btn.style.borderRightWidth  = 0f;
                    btn.style.borderBottomWidth = 0f;
                    btn.style.borderLeftWidth   = 0f;
                    btn.style.borderTopLeftRadius     = 3f;
                    btn.style.borderTopRightRadius    = 3f;
                    btn.style.borderBottomLeftRadius  = 3f;
                    btn.style.borderBottomRightRadius = 3f;
                    row.Add(btn);
                }
                else
                {
                    var badge = new Label("미설치");
                    badge.style.fontSize   = 11f;
                    badge.style.color      = new StyleColor(AchEngineEditorUI.ColorTextMuted);
                    badge.style.marginLeft = 8f;
                    badge.style.flexShrink = 0f;
                    row.Add(badge);
                }
            }

            parent.Add(row);
        }

        // ═════════════════════════════════════════════════════
        // Sheet Row (Table Loader 전용)
        // ═════════════════════════════════════════════════════

        private static VisualElement MakeSheetRow(SheetInfo sheet, Action onRemove)
        {
            var row = new VisualElement();
            row.style.flexDirection   = FlexDirection.Row;
            row.style.alignItems      = Align.Center;
            row.style.paddingTop      = 4f;
            row.style.paddingBottom   = 4f;
            row.style.paddingLeft     = 6f;
            row.style.paddingRight    = 6f;
            row.style.marginBottom    = 4f;
            row.style.backgroundColor = new StyleColor(AchEngineEditorUI.ColorSurface);
            row.style.borderTopLeftRadius     = 3f;
            row.style.borderTopRightRadius    = 3f;
            row.style.borderBottomLeftRadius  = 3f;
            row.style.borderBottomRightRadius = 3f;

            var enabledToggle = new Toggle { value = sheet.enabled };
            enabledToggle.style.width       = 20f;
            enabledToggle.style.marginRight = 4f;
            enabledToggle.RegisterValueChangedCallback(evt => sheet.enabled = evt.newValue);

            var nameField = new TextField { value = sheet.sheetName };
            nameField.style.flexGrow    = 2f;
            nameField.style.marginRight = 4f;
            nameField.RegisterValueChangedCallback(evt => sheet.sheetName = evt.newValue);

            var gidField = new TextField { value = sheet.sheetGid };
            gidField.style.width       = 70f;
            gidField.style.marginRight = 4f;
            gidField.RegisterValueChangedCallback(evt => sheet.sheetGid = evt.newValue);

            var classField = new TextField { value = sheet.className };
            classField.style.flexGrow    = 1f;
            classField.style.marginRight = 4f;
            classField.RegisterValueChangedCallback(evt => sheet.className = evt.newValue);

            var removeBtn = new Button(onRemove) { text = "X" };
            removeBtn.style.width             = 24f;
            removeBtn.style.height            = 22f;
            removeBtn.style.backgroundColor   = new StyleColor(AchEngineEditorUI.ColorButtonDanger);
            removeBtn.style.color             = new StyleColor(Color.white);
            removeBtn.style.borderTopWidth    = 0f;
            removeBtn.style.borderRightWidth  = 0f;
            removeBtn.style.borderBottomWidth = 0f;
            removeBtn.style.borderLeftWidth   = 0f;
            removeBtn.style.borderTopLeftRadius     = 3f;
            removeBtn.style.borderTopRightRadius    = 3f;
            removeBtn.style.borderBottomLeftRadius  = 3f;
            removeBtn.style.borderBottomRightRadius = 3f;
            removeBtn.style.unityTextAlign = TextAnchor.MiddleCenter;

            row.Add(enabledToggle);
            row.Add(nameField);
            row.Add(gidField);
            row.Add(classField);
            row.Add(removeBtn);
            return row;
        }

        // ═════════════════════════════════════════════════════
        // 유틸
        // ═════════════════════════════════════════════════════

        private static int FindLocaleIndex(List<string> choices, string code)
        {
            for (int i = 1; i < choices.Count; i++)
                if (choices[i].Contains($"({code})")) return i;
            return 0;
        }

        private static string ExtractLocaleCode(string choice)
        {
            if (string.IsNullOrEmpty(choice) || choice == "(없음)") return null;
            int s = choice.LastIndexOf('(');
            int e = choice.LastIndexOf(')');
            return (s >= 0 && e > s) ? choice.Substring(s + 1, e - s - 1) : choice;
        }

        private static void SaveLocalizationSettings(LocalizationSettings settings)
        {
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static string GetSceneObjectName(Type type)
        {
            var component = UnityEngine.Object.FindObjectOfType(type) as Component;
            return component != null ? component.gameObject.name : string.Empty;
        }
    }
}
