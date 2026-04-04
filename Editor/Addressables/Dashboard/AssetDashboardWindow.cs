using System.IO;
using System.Linq;
using AchEngine.Assets.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Assets.Editor.Dashboard
{
    public class AssetDashboardWindow : EditorWindow
    {
        private ScrollView _assetList;
        private Label _totalCountLabel;
        private Label _notPlayingLabel;
        private TextField _searchField;
        private string _searchFilter = "";

        [MenuItem("AchEngine/Addressables/Dashboard")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetDashboardWindow>();
            window.titleContent = new GUIContent("Addressables 대시보드");
            window.minSize = new Vector2(600, 300);
        }

        private void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                GetPackagePath("Editor/UI/DashboardView.uxml"));

            if (uxml != null)
            {
                uxml.CloneTree(rootVisualElement);
            }
            else
            {
                BuildFallbackUI();
                return;
            }

            _assetList = rootVisualElement.Q<ScrollView>("asset-list");
            _totalCountLabel = rootVisualElement.Q<Label>("label-total-count");
            _notPlayingLabel = rootVisualElement.Q<Label>("label-not-playing");

            var searchField = rootVisualElement.Q("search-field");
            if (searchField != null)
            {
                var textInput = searchField.Q<TextField>();
                if (textInput != null)
                {
                    textInput.RegisterValueChangedCallback(evt =>
                    {
                        _searchFilter = evt.newValue;
                        RefreshList();
                    });
                }
            }

            var refreshBtn = rootVisualElement.Q<Button>("btn-refresh");
            refreshBtn.clicked += RefreshList;

            var releaseAllBtn = rootVisualElement.Q<Button>("btn-release-all");
            releaseAllBtn.clicked += () =>
            {
                if (Application.isPlaying && AddressableManager.Instance != null)
                {
                    if (EditorUtility.DisplayDialog("전체 해제",
                        "캐시된 에셋 핸들을 모두 해제하시겠습니까?",
                        "예", "취소"))
                    {
                        AddressableManager.Instance.ReleaseAll();
                        RefreshList();
                    }
                }
            };

            // 목록 헤더 추가
            AddListHeader();
            RefreshList();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                RefreshList();
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            RefreshList();
        }

        private void AddListHeader()
        {
            var header = new VisualElement();
            header.AddToClassList("list-header");

            header.Add(CreateHeaderLabel("주소", 200));
            header.Add(CreateHeaderLabel("타입", 100));
            header.Add(CreateHeaderLabel("참조", 60));
            header.Add(CreateHeaderLabel("씬", 100));
            header.Add(CreateHeaderLabel("로드 시간", 70));
            header.Add(CreateHeaderLabel("", 60));

            _assetList?.parent?.Insert(_assetList.parent.IndexOf(_assetList), header);
        }

        private Label CreateHeaderLabel(string text, int minWidth)
        {
            return new Label(text) { style = { minWidth = minWidth } };
        }

        private void RefreshList()
        {
            if (_assetList == null)
                return;

            _assetList.Clear();

            if (!Application.isPlaying || AddressableManager.Instance == null)
            {
                if (_notPlayingLabel != null)
                    _notPlayingLabel.style.display = DisplayStyle.Flex;
                if (_totalCountLabel != null)
                    _totalCountLabel.text = "플레이 모드가 아닙니다";
                return;
            }

            if (_notPlayingLabel != null)
                _notPlayingLabel.style.display = DisplayStyle.None;

            var entries = AddressableManager.Instance.GetCacheSnapshot();
            var filtered = entries
                .Where(kvp => string.IsNullOrEmpty(_searchFilter)
                              || kvp.Key.Contains(_searchFilter, System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(kvp => kvp.Key)
                .ToList();

            foreach (var kvp in filtered)
            {
                _assetList.Add(CreateAssetEntry(kvp.Key, kvp.Value));
            }

            if (_totalCountLabel != null)
                _totalCountLabel.text = $"총 {filtered.Count} / {entries.Count}개 에셋 로드됨";
        }

        private VisualElement CreateAssetEntry(string address, HandleEntry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("asset-entry");

            var addressLabel = new Label(address);
            addressLabel.AddToClassList("asset-address");

            var typeLabel = new Label(entry.AssetType?.Name ?? "알 수 없음");
            typeLabel.AddToClassList("asset-type");

            var refCountLabel = new Label(entry.ReferenceCount.ToString());
            refCountLabel.AddToClassList("asset-refcount");

            var sceneLabel = new Label(entry.OwnerScene?.name ?? "전역");
            sceneLabel.AddToClassList("asset-scene");

            var timeLabel = new Label($"{entry.LoadTime:F1}s");
            timeLabel.AddToClassList("asset-time");

            var releaseBtn = new Button(() =>
            {
                AddressableManager.Instance?.Release(address);
                RefreshList();
            })
            {
                text = "해제"
            };
            releaseBtn.AddToClassList("asset-release-btn");
            releaseBtn.AddToClassList("btn-secondary");

            // 상태 색상 적용
            if (entry.IsValid)
                addressLabel.AddToClassList("status-loaded");
            else
                addressLabel.AddToClassList("status-failed");

            row.Add(addressLabel);
            row.Add(typeLabel);
            row.Add(refCountLabel);
            row.Add(sceneLabel);
            row.Add(timeLabel);
            row.Add(releaseBtn);

            return row;
        }

        private void BuildFallbackUI()
        {
            rootVisualElement.Add(new Label("대시보드 UXML을 찾을 수 없습니다. 플레이 모드에서 API로 에셋을 로드하세요."));
        }

        private static string GetPackagePath(string relativePath)
        {
            var packagePath = $"Packages/com.achieveone.addressables-manager/{relativePath}";
            if (File.Exists(Path.GetFullPath(packagePath)))
                return packagePath;

            var guids = AssetDatabase.FindAssets("t:VisualTreeAsset DashboardView");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("AddressablesManager") || path.Contains("addressables-manager"))
                    return path;
            }

            return packagePath;
        }
    }
}
