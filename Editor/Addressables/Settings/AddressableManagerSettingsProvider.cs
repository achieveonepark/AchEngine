#if ACHENGINE_ADDRESSABLES
using System;
using System.Linq;
using System.Reflection;
using AchEngine.Assets.Internal;
using AchEngine.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
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
                activateHandler = (_, root) => BuildAddressablesPanel(root),
                keywords = new[]
                {
                    "achengine", "addressables", "asset", "bundle",
                    "remote", "aws", "gcs", "build", "watched", "folder",
                    "runtime", "dashboard", "cache"
                }
            };
        }

        private static void BuildAddressablesPanel(VisualElement root)
        {
            root.Clear();
            AchEngineEditorUI.ApplyRootStyle(root);

            var scroll = AchEngineEditorUI.MakeScrollContent(root);
            var settings = AddressableManagerEditorSettings.instance;

            scroll.Add(AchEngineEditorUI.MakePageTitle("Addressables"));
            scroll.Add(AchEngineEditorUI.MakeBodyText(
                "Configure watched folders, remote content, player-content builds, and runtime cache inspection from one place."));

            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                scroll.Add(CreateMissingSettingsBanner(root));
            }

            BuildWatchedFoldersSection(scroll, settings);
            BuildRemoteSettingsSection(scroll, settings);
            BuildBuildSettingsSection(scroll, settings);
            BuildRuntimeDashboardSection(scroll);
        }

        private static VisualElement CreateMissingSettingsBanner(VisualElement root)
        {
            var banner = AchEngineEditorUI.MakeCard();
            banner.style.backgroundColor = new StyleColor(new Color(0.35f, 0.25f, 0.10f));
            banner.style.marginBottom = 12f;

            var warningLabel = new Label("AddressableAssetSettings is missing.");
            warningLabel.style.fontSize = 12f;
            warningLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            warningLabel.style.color = new StyleColor(new Color(1f, 0.80f, 0.30f));
            warningLabel.style.marginBottom = 4f;
            banner.Add(warningLabel);

            var pathLabel = new Label($"Default path: {AddressableAssetSettingsDefaultObject.DefaultAssetPath}");
            pathLabel.style.fontSize = 11f;
            pathLabel.style.color = new StyleColor(AchEngineEditorUI.ColorTextMuted);
            pathLabel.style.marginBottom = 6f;
            banner.Add(pathLabel);

            var createButton = new Button(() =>
            {
                var created = AddressableAssetSettingsDefaultObject.GetSettings(true);
                if (created == null)
                {
                    EditorUtility.DisplayDialog(
                        "Addressables",
                        "Failed to create AddressableAssetSettings.",
                        "OK");
                    return;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorGUIUtility.PingObject(created);
                BuildAddressablesPanel(root);
            })
            {
                text = "Create AddressableAssetSettings"
            };
            createButton.AddToClassList("btn-primary");
            banner.Add(createButton);

            return banner;
        }

        private static void BuildWatchedFoldersSection(VisualElement parent, AddressableManagerEditorSettings settings)
        {
            parent.Add(AchEngineEditorUI.MakeDivider());
            parent.Add(AchEngineEditorUI.MakeSectionTitle("Watched Folders"));
            parent.Add(AchEngineEditorUI.MakeBodyText(
                "Assets inside watched folders are automatically added to the configured Addressables group."));

            var folderList = new VisualElement();
            folderList.style.marginBottom = 8f;

            void RefreshFolderList()
            {
                folderList.Clear();
                for (int i = 0; i < settings.watchedFolders.Count; i++)
                {
                    var index = i;
                    var folder = settings.watchedFolders[i];
                    folderList.Add(CreateFolderEntry(folder, () =>
                    {
                        settings.watchedFolders.RemoveAt(index);
                        settings.SaveSettings();
                        RefreshFolderList();
                    }));
                }
            }

            RefreshFolderList();
            parent.Add(folderList);

            var buttonRow = AchEngineEditorUI.MakeButtonRow();

            var addFolderButton = new Button(() =>
            {
                settings.watchedFolders.Add(new WatchedFolderConfig());
                settings.SaveSettings();
                RefreshFolderList();
            })
            {
                text = "Add Folder"
            };
            addFolderButton.AddToClassList("btn-primary");

            var rescanButton = new Button(RescanWatchedFolders)
            {
                text = "Rescan"
            };
            rescanButton.AddToClassList("btn-secondary");

            buttonRow.Add(addFolderButton);
            buttonRow.Add(rescanButton);
            parent.Add(buttonRow);
        }

        private static void BuildRemoteSettingsSection(VisualElement parent, AddressableManagerEditorSettings settings)
        {
            parent.Add(AchEngineEditorUI.MakeDivider());
            parent.Add(AchEngineEditorUI.MakeSectionTitle("Remote Content"));
            parent.Add(AchEngineEditorUI.MakeBodyText(
                "Choose a cloud provider or specify custom catalog and bundle URLs."));

            var providerField = new EnumField("Cloud Provider", settings.cloudProvider);
            var bucketNameField = new TextField("Bucket Name") { value = settings.bucketName ?? string.Empty };
            var regionField = new TextField("Region") { value = settings.bucketRegion ?? string.Empty };
            var catalogUrlField = new TextField("Catalog URL")
            {
                value = settings.remoteCatalogUrl ?? string.Empty,
                isDelayed = true
            };
            var bundleUrlField = new TextField("Bundle URL")
            {
                value = settings.remoteBundleUrl ?? string.Empty,
                isDelayed = true
            };

            providerField.style.marginBottom = 4f;
            bucketNameField.style.marginBottom = 4f;
            regionField.style.marginBottom = 4f;
            catalogUrlField.style.marginBottom = 4f;
            bundleUrlField.style.marginBottom = 4f;

            var generatedUrlCard = AchEngineEditorUI.MakeCard();
            var generatedUrlLabel = new Label();
            generatedUrlLabel.style.fontSize = 11f;
            generatedUrlLabel.style.color = new StyleColor(AchEngineEditorUI.ColorTextMuted);
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
                    _ => string.Empty
                };

                generatedUrlLabel.text = string.IsNullOrEmpty(url)
                    ? "Using custom URLs."
                    : $"Generated base URL: {url}";
                generatedUrlCard.style.display = string.IsNullOrEmpty(url)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
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

            parent.Add(providerField);
            parent.Add(bucketNameField);
            parent.Add(regionField);
            parent.Add(generatedUrlCard);
            parent.Add(catalogUrlField);
            parent.Add(bundleUrlField);
            UpdateGeneratedUrl();
        }

        private static void BuildBuildSettingsSection(VisualElement parent, AddressableManagerEditorSettings settings)
        {
            parent.Add(AchEngineEditorUI.MakeDivider());
            parent.Add(AchEngineEditorUI.MakeSectionTitle("Build Settings"));

            var autoBuildToggle = new Toggle("Auto-build before Player build")
            {
                value = settings.autoBuildBeforePlayerBuild
            };
            autoBuildToggle.style.marginBottom = 4f;
            autoBuildToggle.RegisterValueChangedCallback(evt =>
            {
                settings.autoBuildBeforePlayerBuild = evt.newValue;
                settings.SaveSettings();
            });

            parent.Add(autoBuildToggle);

            var buttonRow = AchEngineEditorUI.MakeButtonRow();

            var buildButton = new Button(BuildAddressableContent)
            {
                text = "Build Content"
            };
            buildButton.AddToClassList("btn-primary");

            var cleanBuildButton = new Button(CleanBuildAddressableContent)
            {
                text = "Clean Build"
            };
            cleanBuildButton.AddToClassList("btn-secondary");

            buttonRow.Add(buildButton);
            buttonRow.Add(cleanBuildButton);
            parent.Add(buttonRow);
        }

        private static void BuildRuntimeDashboardSection(VisualElement parent)
        {
            parent.Add(AchEngineEditorUI.MakeDivider());
            parent.Add(AchEngineEditorUI.MakeSectionTitle("Runtime Dashboard"));
            parent.Add(AchEngineEditorUI.MakeBodyText(
                "Play Mode only. Inspect cached entries, search them, and release handles directly from Project Settings."));

            var card = AchEngineEditorUI.MakeCard();

            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.marginBottom = 8f;

            var searchField = new ToolbarSearchField();
            searchField.style.flexGrow = 1f;
            searchField.style.marginRight = 6f;

            var refreshButton = new Button { text = "Refresh" };
            refreshButton.AddToClassList("btn-secondary");
            refreshButton.style.marginRight = 6f;

            var releaseAllButton = new Button { text = "Release All" };
            releaseAllButton.AddToClassList("btn-danger");

            toolbar.Add(searchField);
            toolbar.Add(refreshButton);
            toolbar.Add(releaseAllButton);
            card.Add(toolbar);

            var stateLabel = new Label();
            stateLabel.style.fontSize = 11f;
            stateLabel.style.color = new StyleColor(AchEngineEditorUI.ColorTextMuted);
            stateLabel.style.whiteSpace = WhiteSpace.Normal;
            stateLabel.style.marginBottom = 6f;
            card.Add(stateLabel);

            var headerRow = CreateRuntimeHeaderRow();
            headerRow.style.marginBottom = 4f;
            card.Add(headerRow);

            var assetList = new ScrollView();
            assetList.style.maxHeight = 280f;
            assetList.style.marginBottom = 8f;
            card.Add(assetList);

            var totalCountLabel = new Label();
            totalCountLabel.style.fontSize = 11f;
            totalCountLabel.style.color = new StyleColor(AchEngineEditorUI.ColorTextMuted);
            card.Add(totalCountLabel);

            string searchFilter = string.Empty;
            void RefreshView()
            {
                RefreshRuntimeDashboard(assetList, headerRow, stateLabel, totalCountLabel, searchFilter, RefreshView);
            }

            searchField.RegisterValueChangedCallback(evt =>
            {
                searchFilter = evt.newValue ?? string.Empty;
                RefreshView();
            });

            refreshButton.clicked += RefreshView;
            releaseAllButton.clicked += () =>
            {
                var manager = FindRuntimeManager();
                if (manager == null)
                {
                    RefreshView();
                    return;
                }

                if (!EditorUtility.DisplayDialog(
                        "Release All Cached Assets",
                        "Release every cached Addressables handle tracked by the current manager?",
                        "Release All",
                        "Cancel"))
                {
                    return;
                }

                manager.ReleaseAll();
                RefreshView();
            };

            card.schedule.Execute(RefreshView).Every(500);
            RefreshView();
            parent.Add(card);
        }

        private static void RefreshRuntimeDashboard(
            ScrollView assetList,
            VisualElement headerRow,
            Label stateLabel,
            Label totalCountLabel,
            string searchFilter,
            Action refreshAction)
        {
            assetList.Clear();

            if (!Application.isPlaying)
            {
                headerRow.style.display = DisplayStyle.None;
                stateLabel.style.display = DisplayStyle.Flex;
                stateLabel.text = "Enter Play Mode to inspect the runtime cache.";
                totalCountLabel.text = "Play Mode only.";
                return;
            }

            var manager = FindRuntimeManager();
            if (manager == null)
            {
                headerRow.style.display = DisplayStyle.None;
                stateLabel.style.display = DisplayStyle.Flex;
                stateLabel.text = "AddressableManager is not active in the current scene yet.";
                totalCountLabel.text = "0 cached assets.";
                return;
            }

            var entries = manager.GetCacheSnapshot();
            var filteredEntries = entries
                .Where(kvp => string.IsNullOrWhiteSpace(searchFilter)
                    || kvp.Key.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(kvp => kvp.Key)
                .ToList();

            headerRow.style.display = DisplayStyle.Flex;
            totalCountLabel.text = $"{filteredEntries.Count} / {entries.Count} cached assets";

            if (filteredEntries.Count == 0)
            {
                stateLabel.style.display = DisplayStyle.Flex;
                stateLabel.text = string.IsNullOrWhiteSpace(searchFilter)
                    ? "No cached assets are currently tracked."
                    : "No cached assets match the current filter.";
                return;
            }

            stateLabel.style.display = DisplayStyle.None;
            foreach (var entry in filteredEntries)
            {
                assetList.Add(CreateRuntimeAssetRow(entry.Key, entry.Value, refreshAction));
            }
        }

        private static AddressableManager FindRuntimeManager()
        {
            return Application.isPlaying
                ? UnityEngine.Object.FindObjectOfType<AddressableManager>()
                : null;
        }

        private static VisualElement CreateRuntimeHeaderRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 4f;
            row.style.paddingBottom = 4f;
            row.style.paddingLeft = 8f;
            row.style.paddingRight = 8f;
            row.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.25f));
            row.style.borderBottomWidth = 1f;
            row.style.borderBottomColor = new StyleColor(AchEngineEditorUI.ColorDivider);

            var addressLabel = CreateRuntimeHeaderLabel("Address", 220f);
            addressLabel.style.flexGrow = 1f;

            row.Add(addressLabel);
            row.Add(CreateRuntimeHeaderLabel("Type", 110f));
            row.Add(CreateRuntimeHeaderLabel("Refs", 50f, TextAnchor.MiddleCenter));
            row.Add(CreateRuntimeHeaderLabel("Scene", 110f));
            row.Add(CreateRuntimeHeaderLabel("Load", 70f, TextAnchor.MiddleRight));
            row.Add(CreateRuntimeHeaderLabel(string.Empty, 72f));

            return row;
        }

        private static Label CreateRuntimeHeaderLabel(string text, float minWidth, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var label = new Label(text);
            label.style.minWidth = minWidth;
            label.style.fontSize = 11f;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = new StyleColor(AchEngineEditorUI.ColorTextMuted);
            label.style.unityTextAlign = alignment;
            return label;
        }

        private static VisualElement CreateRuntimeAssetRow(string address, HandleEntry entry, Action refreshAction)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 6f;
            row.style.paddingBottom = 6f;
            row.style.paddingLeft = 8f;
            row.style.paddingRight = 8f;
            row.style.marginBottom = 2f;
            row.style.borderBottomWidth = 1f;
            row.style.borderBottomColor = new StyleColor(new Color(1f, 1f, 1f, 0.05f));

            var addressLabel = new Label(address);
            addressLabel.style.flexGrow = 1f;
            addressLabel.style.minWidth = 220f;
            addressLabel.style.fontSize = 12f;
            addressLabel.style.color = new StyleColor(entry.IsValid
                ? AchEngineEditorUI.ColorGreen
                : AchEngineEditorUI.ColorRed);

            var typeLabel = CreateRuntimeValueLabel(entry.AssetType?.Name ?? "-", 110f);
            var refCountLabel = CreateRuntimeValueLabel(entry.ReferenceCount.ToString(), 50f, TextAnchor.MiddleCenter);
            var sceneLabel = CreateRuntimeValueLabel(entry.OwnerScene?.name ?? "Global", 110f);
            var loadTimeLabel = CreateRuntimeValueLabel($"{entry.LoadTime:F1}s", 70f, TextAnchor.MiddleRight);

            var releaseButton = new Button(() =>
            {
                var manager = FindRuntimeManager();
                manager?.Release(address);
                refreshAction?.Invoke();
            })
            {
                text = "Release"
            };
            releaseButton.AddToClassList("btn-secondary");
            releaseButton.style.minWidth = 72f;

            row.Add(addressLabel);
            row.Add(typeLabel);
            row.Add(refCountLabel);
            row.Add(sceneLabel);
            row.Add(loadTimeLabel);
            row.Add(releaseButton);
            return row;
        }

        private static Label CreateRuntimeValueLabel(string text, float minWidth, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var label = new Label(text);
            label.style.minWidth = minWidth;
            label.style.fontSize = 11f;
            label.style.color = new StyleColor(AchEngineEditorUI.ColorTextBody);
            label.style.unityTextAlign = alignment;
            return label;
        }

        private static void RescanWatchedFolders()
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                EditorUtility.DisplayDialog(
                    "Addressables",
                    "Create AddressableAssetSettings before rescanning watched folders.",
                    "OK");
                return;
            }

            var processedCount = AddressableAutoMarker.RescanWatchedFolders();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Addressables] Rescanned watched folders. Processed {processedCount} assets.");
        }

        private static void BuildAddressableContent()
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                EditorUtility.DisplayDialog(
                    "Addressables Build",
                    "Create AddressableAssetSettings before building content.",
                    "OK");
                return;
            }

            AssetDatabase.SaveAssets();
            if (!TryInvokeAddressableSettingsMethod("BuildPlayerContent", out var error))
            {
                EditorUtility.DisplayDialog(
                    "Addressables Build",
                    error?.Message ?? "Unable to invoke the Addressables build API.",
                    "OK");
                return;
            }

            AssetDatabase.Refresh();
            Debug.Log("[Addressables] Build completed.");
        }

        private static void CleanBuildAddressableContent()
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                EditorUtility.DisplayDialog(
                    "Addressables Clean Build",
                    "Create AddressableAssetSettings before running a clean build.",
                    "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Addressables Clean Build",
                    "Clean the existing player content and rebuild it now?",
                    "Clean Build",
                    "Cancel"))
            {
                return;
            }

            AssetDatabase.SaveAssets();
            if (!TryInvokeAddressableSettingsMethod("CleanPlayerContent", out var cleanError))
            {
                EditorUtility.DisplayDialog(
                    "Addressables Clean Build",
                    cleanError?.Message ?? "Unable to invoke the Addressables clean API.",
                    "OK");
                return;
            }

            if (!TryInvokeAddressableSettingsMethod("BuildPlayerContent", out var buildError))
            {
                EditorUtility.DisplayDialog(
                    "Addressables Clean Build",
                    buildError?.Message ?? "Player content was cleaned, but the rebuild failed.",
                    "OK");
                return;
            }

            AssetDatabase.Refresh();
            Debug.Log("[Addressables] Clean build completed.");
        }

        private static bool TryInvokeAddressableSettingsMethod(string methodName, out Exception error)
        {
            error = null;

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var methods = typeof(AddressableAssetSettings)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.Name == methodName)
                .OrderBy(method => method.GetParameters().Length)
                .ToArray();

            foreach (var method in methods)
            {
                try
                {
                    var args = method.GetParameters()
                        .Select(parameter => CreateDefaultMethodArgument(parameter, settings))
                        .ToArray();
                    method.Invoke(null, args);
                    return true;
                }
                catch (ArgumentException)
                {
                }
                catch (TargetInvocationException ex)
                {
                    error = ex.InnerException ?? ex;
                    return false;
                }
            }

            error = new MissingMethodException(typeof(AddressableAssetSettings).FullName, methodName);
            return false;
        }

        private static object CreateDefaultMethodArgument(ParameterInfo parameter, AddressableAssetSettings settings)
        {
            var parameterType = parameter.ParameterType;
            if (parameterType.IsByRef)
            {
                parameterType = parameterType.GetElementType();
            }

            if (parameterType == null)
            {
                return null;
            }

            if (settings != null && parameterType.IsInstanceOfType(settings))
            {
                return settings;
            }

            return parameterType.IsValueType
                ? Activator.CreateInstance(parameterType)
                : null;
        }

        private static VisualElement CreateFolderEntry(WatchedFolderConfig folder, Action onRemove)
        {
            var card = AchEngineEditorUI.MakeCard();
            card.style.marginBottom = 6f;

            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.marginBottom = 6f;

            var pathField = new TextField("Folder Path") { value = folder.folderPath };
            pathField.style.flexGrow = 1f;
            pathField.style.marginRight = 4f;
            pathField.RegisterValueChangedCallback(evt => folder.folderPath = evt.newValue);

            var browseButton = new Button(() =>
            {
                var selected = EditorUtility.OpenFolderPanel("Select Watched Folder", "Assets", string.Empty);
                if (string.IsNullOrEmpty(selected))
                {
                    return;
                }

                var dataPath = Application.dataPath;
                if (selected.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
                {
                    selected = "Assets" + selected.Substring(dataPath.Length);
                }

                folder.folderPath = selected;
                pathField.value = selected;
            })
            {
                text = "..."
            };
            browseButton.style.width = 30f;
            browseButton.style.flexShrink = 0f;
            browseButton.style.alignSelf = Align.FlexEnd;

            var removeButton = new Button(onRemove) { text = "X" };
            removeButton.style.width = 24f;
            removeButton.style.height = 22f;
            removeButton.style.marginLeft = 4f;
            removeButton.style.flexShrink = 0f;
            removeButton.style.alignSelf = Align.FlexEnd;
            removeButton.style.backgroundColor = new StyleColor(AchEngineEditorUI.ColorButtonDanger);
            removeButton.style.color = new StyleColor(Color.white);
            removeButton.style.borderTopWidth = 0f;
            removeButton.style.borderRightWidth = 0f;
            removeButton.style.borderBottomWidth = 0f;
            removeButton.style.borderLeftWidth = 0f;
            removeButton.style.borderTopLeftRadius = 3f;
            removeButton.style.borderTopRightRadius = 3f;
            removeButton.style.borderBottomLeftRadius = 3f;
            removeButton.style.borderBottomRightRadius = 3f;

            headerRow.Add(pathField);
            headerRow.Add(browseButton);
            headerRow.Add(removeButton);
            card.Add(headerRow);

            var groupField = new TextField("Group Name") { value = folder.groupName };
            groupField.style.marginBottom = 4f;
            groupField.RegisterValueChangedCallback(evt => folder.groupName = evt.newValue);

            var namingField = new EnumField("Address Mode", folder.namingMode);
            namingField.style.marginBottom = 4f;
            namingField.RegisterValueChangedCallback(evt =>
                folder.namingMode = (AddressNamingMode)evt.newValue);

            var recursiveToggle = new Toggle("Include Subfolders") { value = folder.recursive };
            recursiveToggle.style.marginBottom = 4f;
            recursiveToggle.RegisterValueChangedCallback(evt => folder.recursive = evt.newValue);

            var labelsField = new TextField("Labels (comma-separated)")
            {
                value = string.Join(", ", folder.labels)
            };
            labelsField.style.marginBottom = 4f;
            labelsField.RegisterValueChangedCallback(evt =>
            {
                folder.labels.Clear();
                foreach (var rawLabel in evt.newValue.Split(','))
                {
                    var label = rawLabel.Trim();
                    if (!string.IsNullOrEmpty(label))
                    {
                        folder.labels.Add(label);
                    }
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
#endif
