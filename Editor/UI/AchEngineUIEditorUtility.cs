using System;
using System.Collections.Generic;
using System.IO;
using AchEngine.UI;
using UnityEditor;
using UnityEngine;

namespace AchEngine.Editor.UI
{
    internal static class AchEngineUIEditorUtility
    {
        internal struct CatalogIssue
        {
            public CatalogIssue(string message, MessageType messageType)
            {
                Message = message;
                MessageType = messageType;
            }

            public string Message { get; }
            public MessageType MessageType { get; }
        }

        public static UIViewCatalog CreateViewCatalogAsset(string directory = null)
        {
            var asset = ScriptableObject.CreateInstance<UIViewCatalog>();
            var targetDirectory = string.IsNullOrWhiteSpace(directory) ? GetSelectedDirectory() : directory;
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(targetDirectory, "UIViewCatalog.asset").Replace("\\", "/"));

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        public static UIRoot CreateUIRoot(GameObject parent = null)
        {
            var root = UIRoot.CreateDefault();
            GameObjectUtility.SetParentAndAlign(root.gameObject, parent);
            Undo.RegisterCreatedObjectUndo(root.gameObject, "UI 루트 생성");
            Selection.activeGameObject = root.gameObject;
            return root;
        }

        public static UIBootstrapper CreateBootstrapper(
            UIViewCatalog catalog = null,
            UIRoot root = null,
            GameObject parent = null)
        {
            var bootstrapperObject = new GameObject("UI Bootstrapper", typeof(UIBootstrapper));
            GameObjectUtility.SetParentAndAlign(bootstrapperObject, parent);
            Undo.RegisterCreatedObjectUndo(bootstrapperObject, "UI 부트스트래퍼 생성");

            var bootstrapper = bootstrapperObject.GetComponent<UIBootstrapper>();
            AssignBootstrapperReferences(bootstrapper, catalog, root);

            Selection.activeGameObject = bootstrapperObject;
            return bootstrapper;
        }

        public static void AssignBootstrapperReferences(UIBootstrapper bootstrapper, UIViewCatalog catalog, UIRoot root)
        {
            if (bootstrapper == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(bootstrapper);
            serializedObject.Update();
            serializedObject.FindProperty("catalog").objectReferenceValue = catalog;
            serializedObject.FindProperty("root").objectReferenceValue = root;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrapper);
        }

        public static UIViewCatalog GetAssignedCatalog(UIBootstrapper bootstrapper)
        {
            if (bootstrapper == null)
            {
                return null;
            }

            var serializedObject = new SerializedObject(bootstrapper);
            return serializedObject.FindProperty("catalog").objectReferenceValue as UIViewCatalog;
        }

        public static UIRoot GetAssignedRoot(UIBootstrapper bootstrapper)
        {
            if (bootstrapper == null)
            {
                return null;
            }

            var serializedObject = new SerializedObject(bootstrapper);
            return serializedObject.FindProperty("root").objectReferenceValue as UIRoot;
        }

        public static UIRoot FindUIRootInOpenScenes()
        {
            return UnityEngine.Object.FindObjectOfType<UIRoot>();
        }

        public static UIBootstrapper FindBootstrapperInOpenScenes()
        {
            return UnityEngine.Object.FindObjectOfType<UIBootstrapper>();
        }

        public static List<CatalogIssue> CollectCatalogIssues(UIViewCatalog catalog)
        {
            var issues = new List<CatalogIssue>();
            if (catalog == null)
            {
                issues.Add(new CatalogIssue("UI 엔트리 편집을 시작하려면 UIViewCatalog를 생성하거나 연결하세요.", MessageType.Info));
                return issues;
            }

            if (catalog.Entries.Count == 0)
            {
                issues.Add(new CatalogIssue("카탈로그가 비어 있습니다. 엔트리를 직접 추가하거나 선택한 UIView 프리팹을 가져오세요.", MessageType.Info));
                return issues;
            }

            var usedIds = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < catalog.Entries.Count; index++)
            {
                var entry = catalog.Entries[index];
                if (entry == null)
                {
                    issues.Add(new CatalogIssue($"엔트리 {index + 1}이 비어 있습니다.", MessageType.Warning));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.Id))
                {
                    issues.Add(new CatalogIssue($"엔트리 {index + 1}에 뷰 ID가 없습니다.", MessageType.Warning));
                }
                else if (!usedIds.TryAdd(entry.Id, index))
                {
                    issues.Add(new CatalogIssue($"엔트리 {usedIds[entry.Id] + 1}와 {index + 1}에 중복된 뷰 ID '{entry.Id}'가 있습니다.", MessageType.Error));
                }

                if (entry.Prefab == null)
                {
                    issues.Add(new CatalogIssue($"엔트리 {index + 1} ('{entry.Id}')에 UIView 프리팹이 할당되지 않았습니다.", MessageType.Warning));
                }

                if (!entry.Pooled && entry.PrewarmCount > 0)
                {
                    issues.Add(new CatalogIssue($"엔트리 '{entry.Id}'는 사전 생성 수가 0보다 크지만 풀링이 비활성화되어 있습니다.", MessageType.Info));
                }
            }

            return issues;
        }

        public static int AddSelectedViewPrefabs(UIViewCatalog catalog)
        {
            if (catalog == null)
            {
                return 0;
            }

            var selectedPrefabs = new List<UIView>();
            foreach (var selectedObject in Selection.objects)
            {
                if (!(selectedObject is GameObject gameObject) || !EditorUtility.IsPersistent(gameObject))
                {
                    continue;
                }

                if (!gameObject.TryGetComponent<UIView>(out var view))
                {
                    continue;
                }

                selectedPrefabs.Add(view);
            }

            if (selectedPrefabs.Count == 0)
            {
                return 0;
            }

            Undo.RecordObject(catalog, "UI 뷰 엔트리 추가");

            var serializedObject = new SerializedObject(catalog);
            serializedObject.Update();
            var entriesProperty = serializedObject.FindProperty("entries");
            var reservedIds = CollectReservedIds(catalog);
            var addedCount = 0;

            for (var index = 0; index < selectedPrefabs.Count; index++)
            {
                var view = selectedPrefabs[index];
                if (ContainsPrefab(entriesProperty, view))
                {
                    continue;
                }

                var uniqueId = MakeUniqueId(view.name, reservedIds);
                var newIndex = entriesProperty.arraySize;
                entriesProperty.InsertArrayElementAtIndex(newIndex);
                var entryProperty = entriesProperty.GetArrayElementAtIndex(newIndex);
                InitializeEntry(entryProperty, uniqueId, view);
                addedCount++;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(catalog);
            return addedCount;
        }

        public static string MakeUniqueId(UIViewCatalog catalog, string seed)
        {
            var reservedIds = CollectReservedIds(catalog);
            return MakeUniqueId(seed, reservedIds);
        }

        private static HashSet<string> CollectReservedIds(UIViewCatalog catalog)
        {
            var reservedIds = new HashSet<string>(StringComparer.Ordinal);
            if (catalog == null)
            {
                return reservedIds;
            }

            for (var index = 0; index < catalog.Entries.Count; index++)
            {
                var entry = catalog.Entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Id))
                {
                    continue;
                }

                reservedIds.Add(entry.Id);
            }

            return reservedIds;
        }

        private static string MakeUniqueId(string seed, ISet<string> reservedIds)
        {
            var baseId = NormalizeId(seed);
            var candidate = baseId;
            var suffix = 1;
            while (reservedIds.Contains(candidate))
            {
                suffix++;
                candidate = $"{baseId}{suffix}";
            }

            reservedIds.Add(candidate);
            return candidate;
        }

        private static bool ContainsPrefab(SerializedProperty entriesProperty, UIView prefab)
        {
            for (var index = 0; index < entriesProperty.arraySize; index++)
            {
                var entryProperty = entriesProperty.GetArrayElementAtIndex(index);
                if (entryProperty.FindPropertyRelative("prefab").objectReferenceValue == prefab)
                {
                    return true;
                }
            }

            return false;
        }

        private static void InitializeEntry(SerializedProperty entryProperty, string id, UIView prefab)
        {
            entryProperty.FindPropertyRelative("id").stringValue = id;
            entryProperty.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            entryProperty.FindPropertyRelative("layer").intValue = (int)UILayerId.Screen;
            entryProperty.FindPropertyRelative("pooled").boolValue = true;
            entryProperty.FindPropertyRelative("prewarmCount").intValue = 0;
            entryProperty.FindPropertyRelative("singleInstance").boolValue = true;
        }

        private static string NormalizeId(string seed)
        {
            if (string.IsNullOrWhiteSpace(seed))
            {
                return "NewView";
            }

            var trimmed = seed.Trim();
            var buffer = new char[trimmed.Length];
            var count = 0;
            for (var index = 0; index < trimmed.Length; index++)
            {
                var character = trimmed[index];
                if (char.IsWhiteSpace(character))
                {
                    continue;
                }

                buffer[count] = character;
                count++;
            }

            return count == 0 ? "NewView" : new string(buffer, 0, count);
        }

        private static string GetSelectedDirectory()
        {
            var path = "Assets";
            foreach (var selected in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                var assetPath = AssetDatabase.GetAssetPath(selected);
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    continue;
                }

                if (Directory.Exists(assetPath))
                {
                    return assetPath;
                }

                var directory = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    return directory.Replace("\\", "/");
                }
            }

            return path;
        }
    }
}
