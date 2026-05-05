using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AchEngine.UI
{
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
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
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
