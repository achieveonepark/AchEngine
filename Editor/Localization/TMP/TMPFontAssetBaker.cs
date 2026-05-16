using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace AchEngine.Localization.Editor
{
    public sealed class TMPFontBakeRequest
    {
        public Font SourceFont;
        public string AssetPath;
        public string CharacterSet;
        public int SamplingPointSize = 90;
        public int AtlasPadding = 9;
        public int AtlasWidth = 1024;
        public int AtlasHeight = 1024;
        public GlyphRenderMode RenderMode = GlyphRenderMode.SDFAA;
        public bool EnableMultiAtlas = true;
        public bool MakeStatic = true;
        public bool IncludeFontFeatures = true;
        public bool Overwrite = true;
    }

    public readonly struct TMPFontBakeResult
    {
        public readonly TMP_FontAsset FontAsset;
        public readonly string AssetPath;
        public readonly string MissingCharacters;
        public readonly int RequestedCharacterCount;

        public bool HasMissingCharacters => !string.IsNullOrEmpty(MissingCharacters);

        public TMPFontBakeResult(
            TMP_FontAsset fontAsset,
            string assetPath,
            string missingCharacters,
            int requestedCharacterCount)
        {
            FontAsset = fontAsset;
            AssetPath = assetPath;
            MissingCharacters = missingCharacters;
            RequestedCharacterCount = requestedCharacterCount;
        }
    }

    public static class TMPFontAssetBaker
    {
        public static TMPFontBakeResult Bake(TMPFontBakeRequest request)
        {
            ValidateRequest(request);

            string assetPath = NormalizeAssetPath(request.AssetPath);
            EnsureAssetDirectory(assetPath);

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
            {
                if (request.Overwrite)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
                else
                {
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                }
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(
                request.SourceFont,
                request.SamplingPointSize,
                request.AtlasPadding,
                request.RenderMode,
                request.AtlasWidth,
                request.AtlasHeight,
                AtlasPopulationMode.Dynamic,
                request.EnableMultiAtlas);

            if (fontAsset == null)
                throw new InvalidOperationException($"TMP FontAsset 생성에 실패했습니다. Source font: {request.SourceFont.name}");

            fontAsset.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            string missingCharacters = string.Empty;
            if (!string.IsNullOrEmpty(request.CharacterSet))
                fontAsset.TryAddCharacters(request.CharacterSet, out missingCharacters, request.IncludeFontFeatures);

            if (request.MakeStatic)
                fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            fontAsset.creationSettings = BuildCreationSettings(request);
            RenameSubAssets(fontAsset);
            EnsureSubAssets(fontAsset);

            EditorUtility.SetDirty(fontAsset);
            if (fontAsset.material != null)
                EditorUtility.SetDirty(fontAsset.material);

            if (fontAsset.atlasTextures != null)
            {
                for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                {
                    if (fontAsset.atlasTextures[i] != null)
                        EditorUtility.SetDirty(fontAsset.atlasTextures[i]);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);

            return new TMPFontBakeResult(
                fontAsset,
                assetPath,
                missingCharacters,
                CountCodepoints(request.CharacterSet));
        }

        private static void ValidateRequest(TMPFontBakeRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.SourceFont == null)
                throw new ArgumentException("Source font is required.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.AssetPath))
                throw new ArgumentException("Asset path is required.", nameof(request));

            if (request.SamplingPointSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.SamplingPointSize));

            if (request.AtlasPadding < 0)
                throw new ArgumentOutOfRangeException(nameof(request.AtlasPadding));

            if (request.AtlasWidth <= 0 || request.AtlasHeight <= 0)
                throw new ArgumentException("Atlas size must be greater than zero.", nameof(request));
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            assetPath = assetPath.Replace('\\', '/');
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal) && !assetPath.StartsWith("Packages/", StringComparison.Ordinal))
                throw new ArgumentException("Asset path must start with Assets/ or Packages/.", nameof(assetPath));

            if (!assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                assetPath += ".asset";

            return assetPath;
        }

        private static void EnsureAssetDirectory(string assetPath)
        {
            string directory = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(directory))
                return;

            string fullPath = Path.GetFullPath(directory);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
        }

        private static FontAssetCreationSettings BuildCreationSettings(TMPFontBakeRequest request)
        {
            string sourceFontPath = AssetDatabase.GetAssetPath(request.SourceFont);
            return new FontAssetCreationSettings
            {
                sourceFontFileName = request.SourceFont.name,
                sourceFontFileGUID = AssetDatabase.AssetPathToGUID(sourceFontPath),
                faceIndex = 0,
                pointSizeSamplingMode = 0,
                pointSize = request.SamplingPointSize,
                padding = request.AtlasPadding,
                paddingMode = 2,
                packingMode = 0,
                atlasWidth = request.AtlasWidth,
                atlasHeight = request.AtlasHeight,
                characterSetSelectionMode = 7,
                characterSequence = request.CharacterSet ?? string.Empty,
                referencedFontAssetGUID = string.Empty,
                referencedTextAssetGUID = string.Empty,
                fontStyle = 0,
                fontStyleModifier = 0,
                renderMode = (int)request.RenderMode,
                includeFontFeatures = request.IncludeFontFeatures
            };
        }

        private static void RenameSubAssets(TMP_FontAsset fontAsset)
        {
            if (fontAsset.atlasTextures != null)
            {
                for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                {
                    if (fontAsset.atlasTextures[i] != null)
                        fontAsset.atlasTextures[i].name = $"{fontAsset.name} Atlas {i}";
                }
            }

            if (fontAsset.material != null)
                fontAsset.material.name = $"{fontAsset.name} Material";
        }

        private static void EnsureSubAssets(TMP_FontAsset fontAsset)
        {
            if (fontAsset.atlasTextures != null)
            {
                for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                    EnsureSubAsset(fontAsset, fontAsset.atlasTextures[i]);
            }

            EnsureSubAsset(fontAsset, fontAsset.material);
        }

        private static void EnsureSubAsset(TMP_FontAsset fontAsset, UnityEngine.Object subAsset)
        {
            if (subAsset == null)
                return;

            string subAssetPath = AssetDatabase.GetAssetPath(subAsset);
            if (string.IsNullOrEmpty(subAssetPath))
                AssetDatabase.AddObjectToAsset(subAsset, fontAsset);
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
    }
}
