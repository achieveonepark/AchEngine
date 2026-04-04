using System;
using AchEngine.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Assets.Editor
{
    internal static class AddressableManagerSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/AchEngine/Addressables", SettingsScope.Project)
            {
                label = "Addressables",
                activateHandler = (ctx, root) => BuildAddressablesPanel(root),
                keywords = new[]
                {
                    "achengine", "addressables", "asset", "bundle",
                    "remote", "aws", "gcs", "build", "watched", "folder"
                }
            };
        }

        private static void BuildAddressablesPanel(VisualElement root)
        {
            AchEngineEditorUI.ApplyRootStyle(root);
            var scroll = AchEngineEditorUI.MakeScrollContent(root);

            var settings = AddressableManagerEditorSettings.instance;

            scroll.Add(AchEngineEditorUI.MakePageTitle("Addressables", null));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "Addressable Asset System 통합 설정. " +
                "감시 폴더를 등록하면 에셋이 자동으로 그룹에 추가되고, " +
                "원격 배포를 위한 클라우드 버킷 설정과 빌드 자동화를 지원합니다."));

            // ── Addressables 설정 배너 ─────────────────────────────
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                var banner = new VisualElement();
                banner.style.backgroundColor        = new StyleColor(new Color(0.35f, 0.25f, 0.10f));
                banner.style.borderTopLeftRadius    = 4f;
                banner.style.borderTopRightRadius   = 4f;
                banner.style.borderBottomLeftRadius  = 4f;
                banner.style.borderBottomRightRadius = 4f;
                banner.style.paddingTop    = 10f;
                banner.style.paddingBottom = 10f;
                banner.style.paddingLeft   = 12f;
                banner.style.paddingRight  = 12f;
                banner.style.marginBottom  = 12f;

                var warningLabel = new Label("⚠  AddressableAssetSettings 가 없습니다.");
                warningLabel.style.fontSize     = 12f;
                warningLabel.style.color        = new StyleColor(new Color(1f, 0.80f, 0.30f));
                warningLabel.style.marginBottom = 4f;
                banner.Add(warningLabel);

                var pathLabel = new Label(
                    $"기본 생성 경로: {AddressableAssetSettingsDefaultObject.DefaultAssetPath}");
                pathLabel.style.fontSize     = 11f;
                pathLabel.style.color        = new StyleColor(AchEngineEditorUI.ColorTextMuted);
                pathLabel.style.marginBottom = 6f;
                banner.Add(pathLabel);

                var createBtn = new Button(() =>
                {
                    var created = AddressableAssetSettingsDefaultObject.GetSettings(true);
                    if (created == null)
                    {
                        EditorUtility.DisplayDialog(
                            "Addressables 설정 생성",
                            "AddressableAssetSettings를 생성하지 못했습니다.", "확인");
                        return;
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorGUIUtility.PingObject(created);
                    EditorUtility.DisplayDialog(
                        "Addressables 설정 생성",
                        $"생성 완료\n경로: {AssetDatabase.GetAssetPath(created)}", "확인");
                    // 패널 재빌드
                    root.Clear();
                    BuildAddressablesPanel(root);
                }) { text = "AddressableAssetSettings 생성" };
                banner.Add(createBtn);

                scroll.Add(banner);
            }

            // ── 감시 폴더 ──────────────────────────────────────────
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSectionTitle("감시 폴더"));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "폴더를 등록하면 하위 에셋이 지정된 Addressables 그룹에 자동 추가됩니다."));

            var folderList = new VisualElement();
            folderList.style.marginBottom = 8f;

            void RefreshFolderList()
            {
                folderList.Clear();
                for (int i = 0; i < settings.watchedFolders.Count; i++)
                {
                    var idx    = i;
                    var folder = settings.watchedFolders[i];
                    folderList.Add(CreateFolderEntry(folder, () =>
                    {
                        settings.watchedFolders.RemoveAt(idx);
                        settings.SaveSettings();
                        RefreshFolderList();
                    }));
                }
            }

            RefreshFolderList();
            scroll.Add(folderList);

            var folderBtnRow = AchEngineEditorUI.MakeButtonRow();

            var addFolderBtn = new Button(() =>
            {
                settings.watchedFolders.Add(new WatchedFolderConfig());
                settings.SaveSettings();
                RefreshFolderList();
            }) { text = "+ 폴더 추가" };
            addFolderBtn.AddToClassList("btn-primary");

            var rescanBtn = new Button(BuildMenuItems.RescanWatchedFolders)
            { text = "다시 스캔" };
            rescanBtn.AddToClassList("btn-secondary");

            folderBtnRow.Add(addFolderBtn);
            folderBtnRow.Add(rescanBtn);
            scroll.Add(folderBtnRow);

            // ── 원격 구성 ──────────────────────────────────────────
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSectionTitle("원격 구성"));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "클라우드 스토리지 정보를 입력하면 원격 URL이 자동 생성됩니다."));

            var providerField   = new EnumField("클라우드 공급자", settings.cloudProvider);
            var bucketNameField = new TextField("버킷 이름")    { value = settings.bucketName      ?? "" };
            var regionField     = new TextField("버킷 리전")    { value = settings.bucketRegion    ?? "" };
            var catalogUrlField = new TextField("카탈로그 URL") { value = settings.remoteCatalogUrl ?? "", isDelayed = true };
            var bundleUrlField  = new TextField("번들 URL")     { value = settings.remoteBundleUrl  ?? "", isDelayed = true };

            providerField.style.marginBottom   = 4f;
            bucketNameField.style.marginBottom = 4f;
            regionField.style.marginBottom     = 4f;
            catalogUrlField.style.marginBottom = 4f;
            bundleUrlField.style.marginBottom  = 4f;

            // 생성된 URL 카드
            var generatedUrlCard  = AchEngineEditorUI.MakeCard();
            var generatedUrlLabel = new Label();
            generatedUrlLabel.style.fontSize   = 11f;
            generatedUrlLabel.style.color      = new StyleColor(AchEngineEditorUI.ColorTextMuted);
            generatedUrlLabel.style.whiteSpace = WhiteSpace.Normal;
            generatedUrlCard.Add(generatedUrlLabel);

            void UpdateGeneratedUrl()
            {
                var url = settings.cloudProvider switch
                {
                    CloudProvider.AWSS3 =>
                        $"https://{settings.bucketName}.s3.{settings.bucketRegion}.amazonaws.com/[BuildTarget]",
                    CloudProvider.GoogleCloudStorage =>
                        $"https://storage.googleapis.com/{settings.bucketName}/[BuildTarget]",
                    _ => ""
                };
                generatedUrlLabel.text       = string.IsNullOrEmpty(url) ? "(Custom URL 사용)" : $"생성된 URL: {url}";
                generatedUrlCard.style.display = string.IsNullOrEmpty(url) ? DisplayStyle.None : DisplayStyle.Flex;
            }

            providerField.RegisterValueChangedCallback(evt =>
            {
                settings.cloudProvider = (CloudProvider)evt.newValue;
                settings.SaveSettings();
                UpdateGeneratedUrl();
            });
            bucketNameField.RegisterValueChangedCallback(evt =>
            {
                settings.bucketName = evt.newValue;
                settings.SaveSettings();
                UpdateGeneratedUrl();
            });
            regionField.RegisterValueChangedCallback(evt =>
            {
                settings.bucketRegion = evt.newValue;
                settings.SaveSettings();
                UpdateGeneratedUrl();
            });
            catalogUrlField.RegisterValueChangedCallback(evt =>
            {
                settings.remoteCatalogUrl = evt.newValue;
                settings.SaveSettings();
            });
            bundleUrlField.RegisterValueChangedCallback(evt =>
            {
                settings.remoteBundleUrl = evt.newValue;
                settings.SaveSettings();
            });

            scroll.Add(providerField);
            scroll.Add(bucketNameField);
            scroll.Add(regionField);
            scroll.Add(generatedUrlCard);
            scroll.Add(catalogUrlField);
            scroll.Add(bundleUrlField);
            UpdateGeneratedUrl();

            // ── 빌드 설정 ──────────────────────────────────────────
            scroll.Add(AchEngineEditorUI.MakeDivider());
            scroll.Add(AchEngineEditorUI.MakeSectionTitle("빌드 설정"));

            var autoBuildToggle = new Toggle("플레이어 빌드 전 자동 빌드")
            { value = settings.autoBuildBeforePlayerBuild };
            autoBuildToggle.style.marginBottom = 4f;
            autoBuildToggle.RegisterValueChangedCallback(evt =>
            {
                settings.autoBuildBeforePlayerBuild = evt.newValue;
                settings.SaveSettings();
            });

            var enforceToggle = new Toggle("플레이 모드에서 기존 빌드 강제 사용")
            { value = settings.enforceUseExistingBuild };
            enforceToggle.style.marginBottom = 4f;
            enforceToggle.RegisterValueChangedCallback(evt =>
            {
                settings.enforceUseExistingBuild = evt.newValue;
                settings.SaveSettings();
            });

            scroll.Add(autoBuildToggle);
            scroll.Add(enforceToggle);

            var buildBtnRow = AchEngineEditorUI.MakeButtonRow();

            var buildBtn = new Button(BuildMenuItems.BuildAddressableContent)
            { text = "콘텐츠 빌드" };
            buildBtn.AddToClassList("btn-primary");

            var cleanBuildBtn = new Button(BuildMenuItems.CleanBuildAddressableContent)
            { text = "클린 빌드" };

            buildBtnRow.Add(buildBtn);
            buildBtnRow.Add(cleanBuildBtn);
            scroll.Add(buildBtnRow);
        }

        // ─────────────────────────────────────────────────────────────
        // 감시 폴더 엔트리
        // ─────────────────────────────────────────────────────────────
        private static VisualElement CreateFolderEntry(WatchedFolderConfig folder, Action onRemove)
        {
            var card = AchEngineEditorUI.MakeCard();
            card.style.marginBottom = 6f;

            // 헤더 행: 경로 + 찾기 + 제거
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.marginBottom  = 6f;

            var pathField = new TextField("폴더 경로") { value = folder.folderPath };
            pathField.style.flexGrow    = 1f;
            pathField.style.marginRight = 4f;
            pathField.RegisterValueChangedCallback(evt => folder.folderPath = evt.newValue);

            var browseBtn = new Button(() =>
            {
                var selected = EditorUtility.OpenFolderPanel("감시 폴더 선택", "Assets", "");
                if (!string.IsNullOrEmpty(selected))
                {
                    var dataPath = Application.dataPath;
                    if (selected.StartsWith(dataPath))
                        selected = "Assets" + selected.Substring(dataPath.Length);
                    folder.folderPath = selected;
                    pathField.value   = selected;
                }
            }) { text = "..." };
            browseBtn.style.width      = 30f;
            browseBtn.style.flexShrink = 0f;
            browseBtn.style.alignSelf  = Align.FlexEnd;

            var removeBtn = new Button(onRemove) { text = "X" };
            removeBtn.style.width             = 24f;
            removeBtn.style.height            = 22f;
            removeBtn.style.marginLeft        = 4f;
            removeBtn.style.flexShrink        = 0f;
            removeBtn.style.alignSelf         = Align.FlexEnd;
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

            headerRow.Add(pathField);
            headerRow.Add(browseBtn);
            headerRow.Add(removeBtn);
            card.Add(headerRow);

            // 상세 설정
            var groupField = new TextField("그룹 이름") { value = folder.groupName };
            groupField.style.marginBottom = 4f;
            groupField.RegisterValueChangedCallback(evt => folder.groupName = evt.newValue);

            var namingField = new EnumField("주소 생성 방식", folder.namingMode);
            namingField.style.marginBottom = 4f;
            namingField.RegisterValueChangedCallback(evt =>
                folder.namingMode = (AddressNamingMode)evt.newValue);

            var recursiveToggle = new Toggle("하위 폴더 포함") { value = folder.recursive };
            recursiveToggle.style.marginBottom = 4f;
            recursiveToggle.RegisterValueChangedCallback(evt => folder.recursive = evt.newValue);

            var labelsField = new TextField("라벨 (쉼표로 구분)")
            { value = string.Join(", ", folder.labels) };
            labelsField.style.marginBottom = 4f;
            labelsField.RegisterValueChangedCallback(evt =>
            {
                folder.labels.Clear();
                foreach (var lbl in evt.newValue.Split(','))
                {
                    var t = lbl.Trim();
                    if (!string.IsNullOrEmpty(t)) folder.labels.Add(t);
                }
            });

            card.Add(groupField);
            card.Add(namingField);
            card.Add(recursiveToggle);
            card.Add(labelsField);

            return card;
        }
    }
}
