using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

namespace AchEngine.UI
{
    /// <summary>
    /// UI 씬 루트. Canvas, 레이어별 RectTransform, 뷰 풀 영역을 관리합니다.
    /// UIService가 레이어 루트를 참조할 때 이 컴포넌트를 사용합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIRoot : MonoBehaviour
    {
        [Serializable]
        private struct LayerBinding
        {
            public UILayerId Layer;
            public RectTransform Root;
        }

        [SerializeField] private RectTransform layersRoot;
        [SerializeField] private RectTransform pooledRoot;
        [SerializeField] private List<LayerBinding> layers = new List<LayerBinding>();

        private readonly Dictionary<UILayerId, RectTransform> layerMap = new Dictionary<UILayerId, RectTransform>();

        /// <summary>안전 영역 안쪽으로 줄일 레이어. Background·Overlay는 화면 전체를 덮어야 하므로 제외합니다.</summary>
        private static readonly UILayerId[] SafeAreaLayers =
        {
            UILayerId.Screen,
            UILayerId.Popup,
            UILayerId.Tooltip
        };

        /// <summary>풀링된 뷰를 숨겨두는 루트 RectTransform입니다.</summary>
        public RectTransform PoolRoot
        {
            get
            {
                EnsureRuntimeStructure();
                return pooledRoot;
            }
        }

        private void Awake()
        {
            EnsureRuntimeStructure();
            EnsureEventSystem();
        }

        private void Reset()
        {
            SetupCanvas(true);
            EnsureRuntimeStructure();
            EnsureEventSystem();
        }

        /// <summary>지정한 레이어의 루트 RectTransform을 반환합니다. 없으면 자동으로 생성합니다.</summary>
        public RectTransform GetLayerRoot(UILayerId layer)
        {
            EnsureRuntimeStructure();
            if (!layerMap.TryGetValue(layer, out var root) || root == null)
            {
                root = CreateStretchChild(layer.ToString(), layersRoot, true);
                layers.Add(new LayerBinding { Layer = layer, Root = root });
                RebuildLayerMap();
            }

            return root;
        }

        /// <summary>Canvas, CanvasScaler, UIRoot가 붙은 기본 구성의 GameObject를 생성해 반환합니다.</summary>
        public static UIRoot CreateDefault(string name = "UI Root")
        {
            var rootObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(UIRoot));

            var root = rootObject.GetComponent<UIRoot>();
            root.SetupCanvas(true);
            root.EnsureRuntimeStructure();
            root.EnsureEventSystem();
            return root;
        }

        internal void EnsureRuntimeStructure()
        {
            SetupCanvas(false);

            if (layersRoot == null)
            {
                layersRoot = CreateStretchChild("Layers", transform, true);
            }

            if (pooledRoot == null)
            {
                pooledRoot = CreateStretchChild("Pool", transform, false);
            }

            EnsureLayer(UILayerId.Background);
            EnsureLayer(UILayerId.Screen);
            EnsureLayer(UILayerId.Popup);
            EnsureLayer(UILayerId.Overlay);
            EnsureLayer(UILayerId.Tooltip);
            RebuildLayerMap();
            EnsureSafeAreaLayers();

            if (pooledRoot != null)
            {
                pooledRoot.gameObject.SetActive(false);
            }
        }

        private void EnsureLayer(UILayerId layer)
        {
            for (var index = 0; index < layers.Count; index++)
            {
                if (layers[index].Layer == layer && layers[index].Root != null)
                {
                    return;
                }
            }

            layers.Add(new LayerBinding
            {
                Layer = layer,
                Root = CreateStretchChild(layer.ToString(), layersRoot, true)
            });
        }

        /// <summary>Screen·Popup·Tooltip 레이어 루트가 노치·홈 인디케이터를 피하도록 UISafeAreaFitter를 붙입니다.</summary>
        private void EnsureSafeAreaLayers()
        {
            for (var index = 0; index < SafeAreaLayers.Length; index++)
            {
                if (!layerMap.TryGetValue(SafeAreaLayers[index], out var root) || root == null)
                {
                    continue;
                }

                if (!root.TryGetComponent<UISafeAreaFitter>(out _))
                {
                    root.gameObject.AddComponent<UISafeAreaFitter>();
                }
            }
        }

        private void RebuildLayerMap()
        {
            layers.Sort((left, right) => left.Layer.CompareTo(right.Layer));
            layerMap.Clear();

            for (var index = 0; index < layers.Count; index++)
            {
                var binding = layers[index];
                if (binding.Root == null)
                {
                    continue;
                }

                binding.Root.SetSiblingIndex(index);
                layerMap[binding.Layer] = binding.Root;
            }
        }

        private void SetupCanvas(bool overwriteSettings)
        {
            var canvas = GetOrAddComponent<Canvas>(gameObject);
            var scaler = GetOrAddComponent<CanvasScaler>(gameObject);
            GetOrAddComponent<GraphicRaycaster>(gameObject);

            if (overwriteSettings)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
        }

        private static RectTransform CreateStretchChild(string name, Transform parent, bool active)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.SetActive(active);
            var rectTransform = child.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            return rectTransform;
        }

        private static T GetOrAddComponent<T>(GameObject target)
            where T : Component
        {
            if (!target.TryGetComponent<T>(out var component))
            {
                component = target.AddComponent<T>();
            }

            return component;
        }
    }
}
