using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UIElements;

namespace AchEngine.Localization.Editor
{
    public class FontAssetMakerWindow : EditorWindow
    {
        private const string OutputDirectory = "Assets/Fonts/Generated";
        private const string ExtraGameCharacters =
            "\u20A9\u2026\u201C\u201D\u2018\u2019\u2022\u00B7\u203B" +
            "\u2605\u2606\u2661\u2665\u2192\u2190\u2191\u2193" +
            "\u2713\u2714\u2715\u00D7\u25CB\u25CF\u25A0\u25A1\u25B2\u25BC";

        [SerializeField] private Font sourceFont;
        [SerializeField] private bool includeProjectTextAssets = true;
        [SerializeField] private bool buildKorean = true;
        [SerializeField] private bool buildEnglish = true;
        [SerializeField] private bool buildJapanese;

        private ObjectField _fontField;
        private Toggle _koreanToggle;
        private Toggle _englishToggle;
        private Toggle _japaneseToggle;
        private Toggle _projectTextToggle;
        private Label _databaseLabel;
        private Label _outputLabel;
        private Label _resultLabel;
        private Button _makeButton;

        [MenuItem("AchEngine/Localization/FontAsset Maker")]
        public static void Open()
        {
            var window = GetWindow<FontAssetMakerWindow>();
            window.titleContent = new GUIContent("FontAsset Maker");
            window.minSize = new Vector2(420f, 360f);
            window.Show();
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.paddingLeft = 16f;
            rootVisualElement.style.paddingRight = 16f;
            rootVisualElement.style.paddingTop = 14f;
            rootVisualElement.style.paddingBottom = 14f;

            var title = new Label("FontAsset Maker");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 18f;
            title.style.marginBottom = 10f;
            rootVisualElement.Add(title);

            _fontField = new ObjectField("Source Font")
            {
                objectType = typeof(Font),
                allowSceneObjects = false,
                value = sourceFont
            };
            _fontField.RegisterValueChangedCallback(evt =>
            {
                sourceFont = evt.newValue as Font;
                RefreshState();
            });
            rootVisualElement.Add(_fontField);

            var langHeader = new Label("Build Languages");
            langHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            langHeader.style.marginTop = 12f;
            langHeader.style.marginBottom = 2f;
            rootVisualElement.Add(langHeader);

            _koreanToggle = new Toggle("Korean (한글)")
            {
                value = buildKorean,
                tooltip = "한글 음절 전체 + Jamo + ko 로케일 문자열로 별도 FontAsset을 생성합니다."
            };
            _koreanToggle.RegisterValueChangedCallback(evt =>
            {
                buildKorean = evt.newValue;
                RefreshState();
            });
            rootVisualElement.Add(_koreanToggle);

            _englishToggle = new Toggle("English")
            {
                value = buildEnglish,
                tooltip = "ASCII + en 로케일 문자열로 별도 FontAsset을 생성합니다."
            };
            _englishToggle.style.marginTop = 2f;
            _englishToggle.RegisterValueChangedCallback(evt =>
            {
                buildEnglish = evt.newValue;
                RefreshState();
            });
            rootVisualElement.Add(_englishToggle);

            _japaneseToggle = new Toggle("Japanese (日本語)")
            {
                value = buildJapanese,
                tooltip = "히라가나 + 가타카나 + 상용 한자 + ja 로케일 문자열로 별도 FontAsset을 생성합니다.\n한자 범위(약 2만 자)로 인해 베이킹에 수 분이 소요될 수 있습니다."
            };
            _japaneseToggle.style.marginTop = 2f;
            _japaneseToggle.RegisterValueChangedCallback(evt =>
            {
                buildJapanese = evt.newValue;
                RefreshState();
            });
            rootVisualElement.Add(_japaneseToggle);

            _projectTextToggle = new Toggle("Include project CSV/JSON/TXT")
            {
                value = includeProjectTextAssets,
                tooltip = "Assets 하위 TextAsset 파일을 스캔해 모든 언어 빌드에 포함합니다."
            };
            _projectTextToggle.style.marginTop = 8f;
            _projectTextToggle.RegisterValueChangedCallback(evt =>
            {
                includeProjectTextAssets = evt.newValue;
                RefreshState();
            });
            rootVisualElement.Add(_projectTextToggle);

            _databaseLabel = new Label();
            _databaseLabel.style.marginTop = 8f;
            _databaseLabel.style.whiteSpace = WhiteSpace.Normal;
            rootVisualElement.Add(_databaseLabel);

            _outputLabel = new Label();
            _outputLabel.style.marginTop = 4f;
            _outputLabel.style.whiteSpace = WhiteSpace.Normal;
            rootVisualElement.Add(_outputLabel);

            _makeButton = new Button(MakeFontAssets)
            {
                text = "Make TMP FontAssets"
            };
            _makeButton.style.height = 34f;
            _makeButton.style.marginTop = 14f;
            rootVisualElement.Add(_makeButton);

            _resultLabel = new Label();
            _resultLabel.style.marginTop = 10f;
            _resultLabel.style.whiteSpace = WhiteSpace.Normal;
            rootVisualElement.Add(_resultLabel);

            RefreshState();
        }

        private void RefreshState()
        {
            var settings = LocalizationEditorUtility.GetOrCreateSettings();
            var database = settings != null ? settings.database : null;
            string textAssetSuffix = includeProjectTextAssets ? " + project text assets" : string.Empty;

            _databaseLabel.text = database != null
                ? $"Sources: Localization {database.name}{textAssetSuffix}"
                : $"Sources: no Localization database{textAssetSuffix}";

            bool anySelected = buildKorean || buildEnglish || buildJapanese;

            if (sourceFont != null && anySelected)
            {
                var assetNames = BuildSelectedAssetNames(sourceFont);
                _outputLabel.text = $"Output: {OutputDirectory}/\n  {string.Join("\n  ", assetNames)}";
            }
            else
            {
                _outputLabel.text = $"Output: {OutputDirectory}/";
            }

            _makeButton.SetEnabled(sourceFont != null && anySelected);
        }

        private List<string> BuildSelectedAssetNames(Font font)
        {
            var names = new List<string>();
            if (buildKorean) names.Add(SanitizeFileName($"{font.name}_Korean_TMP.asset"));
            if (buildEnglish) names.Add(SanitizeFileName($"{font.name}_English_TMP.asset"));
            if (buildJapanese) names.Add(SanitizeFileName($"{font.name}_Japanese_TMP.asset"));
            return names;
        }

        private void MakeFontAssets()
        {
            if (sourceFont == null)
            {
                EditorUtility.DisplayDialog("FontAsset Maker", "Source Font를 넣어주세요.", "OK");
                return;
            }

            if (!buildKorean && !buildEnglish && !buildJapanese)
            {
                EditorUtility.DisplayDialog("FontAsset Maker", "빌드할 언어를 하나 이상 선택해주세요.", "OK");
                return;
            }

            var jobs = new List<(string label, string suffix, string[] prefixes, bool korean, bool japanese)>();
            if (buildKorean)   jobs.Add(("Korean",   "Korean",   new[] { "ko" }, true,  false));
            if (buildEnglish)  jobs.Add(("English",  "English",  new[] { "en" }, false, false));
            if (buildJapanese) jobs.Add(("Japanese", "Japanese", new[] { "ja" }, false, true));

            var settings = LocalizationEditorUtility.GetOrCreateSettings();
            var database = settings != null ? settings.database : null;
            var additionalTexts = includeProjectTextAssets ? LoadProjectTextAssetTexts() : null;

            var resultLines = new List<string>();
            UnityEngine.Object lastAsset = null;

            try
            {
                for (int i = 0; i < jobs.Count; i++)
                {
                    var (label, suffix, prefixes, includeKorean, includeJapanese) = jobs[i];
                    float progress = (float)i / jobs.Count;
                    EditorUtility.DisplayProgressBar("FontAsset Maker", $"Baking {label}...", progress);

                    var characterSet = BuildCharacterSet(database, additionalTexts, prefixes, includeKorean, includeJapanese);
                    int atlasSize = ChooseAtlasSize(characterSet.CharacterCount);
                    string assetPath = BuildAssetPath(sourceFont, suffix);

                    var result = TMPFontAssetBaker.Bake(new TMPFontBakeRequest
                    {
                        SourceFont = sourceFont,
                        AssetPath = assetPath,
                        CharacterSet = characterSet.CharacterSet,
                        SamplingPointSize = 90,
                        AtlasPadding = 9,
                        AtlasWidth = atlasSize,
                        AtlasHeight = atlasSize,
                        RenderMode = GlyphRenderMode.SDFAA,
                        EnableMultiAtlas = true,
                        MakeStatic = true,
                        IncludeFontFeatures = true,
                        Overwrite = true
                    });

                    lastAsset = result.FontAsset;

                    if (result.HasMissingCharacters)
                    {
                        int missingCount = CountCodepoints(result.MissingCharacters);
                        string preview = BuildCharacterPreview(result.MissingCharacters, 60);
                        resultLines.Add($"[{label}] missing {missingCount} chars — {result.AssetPath}\n  Preview: {preview}");
                        Debug.LogWarning(
                            $"[FontAsset Maker] {label}: {missingCount}자 누락. Preview: {preview}",
                            result.FontAsset);
                    }
                    else
                    {
                        resultLines.Add(
                            $"[{label}] {result.AssetPath}\n  {characterSet.CharacterCount} chars, {atlasSize}x{atlasSize} atlas");
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (lastAsset != null)
                {
                    Selection.activeObject = lastAsset;
                    EditorGUIUtility.PingObject(lastAsset);
                }

                _resultLabel.text = string.Join("\n\n", resultLines);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("FontAsset Maker", ex.Message, "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static LocalizedTMPCharacterSetResult BuildCharacterSet(
            LocaleDatabase database,
            List<string> additionalTexts,
            string[] localePrefixes,
            bool includeKorean,
            bool includeJapanese)
        {
            var options = new LocalizedTMPCharacterSetOptions
            {
                LocaleCodePrefixes = localePrefixes,
                AdditionalTexts = additionalTexts,
                IncludeCommonAscii = true,
                IncludeKorean = includeKorean,
                IncludeJapanese = includeJapanese,
                IncludeLocalizationKeys = false,
                StripRichTextTags = true,
                StripFormatPlaceholders = true
            };

            if (database != null)
                return LocalizedTMPCharacterSetBuilder.Build(database, options);

            return LocalizedTMPCharacterSetBuilder.BuildFallback(options);
        }

        private static List<string> LoadProjectTextAssetTexts()
        {
            var texts = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!ShouldIncludeProjectTextAsset(path))
                    continue;

                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
                    texts.Add(textAsset.text);
            }

            return texts;
        }

        private static bool ShouldIncludeProjectTextAsset(string path)
        {
            string extension = Path.GetExtension(path);
            return string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".tsv", StringComparison.OrdinalIgnoreCase);
        }

        private static int ChooseAtlasSize(int characterCount)
        {
            if (characterCount <= 350)
                return 1024;

            if (characterCount <= 1400)
                return 2048;

            return 4096;
        }

        private static string BuildAssetPath(Font font, string languageSuffix)
        {
            string assetName = SanitizeFileName($"{font.name}_{languageSuffix}_TMP.asset");
            return $"{OutputDirectory}/{assetName}";
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                value = value.Replace(invalidChar, '_');

            return value.Replace(' ', '_');
        }

        private static int CountCodepoints(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    i++;

                count++;
            }

            return count;
        }

        private static string BuildCharacterPreview(string text, int maxCodepoints)
        {
            if (string.IsNullOrEmpty(text) || maxCodepoints <= 0)
                return string.Empty;

            int count = 0;
            int endIndex = 0;
            for (int i = 0; i < text.Length && count < maxCodepoints; i++)
            {
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    i++;

                endIndex = i + 1;
                count++;
            }

            string preview = text.Substring(0, endIndex);
            return CountCodepoints(text) > maxCodepoints ? preview + "..." : preview;
        }
    }
}
