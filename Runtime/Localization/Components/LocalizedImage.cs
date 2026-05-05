using System;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Localization
{
    /// <summary>
    /// locale에 따라 Image의 Sprite를 자동 교체하는 컴포넌트
    /// </summary>
    [AddComponentMenu("Achieve/Localization/Localized Image")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class LocalizedImage : MonoBehaviour
    {
        [Serializable]
        public struct LocaleSprite
        {
            public string localeCode;
            public Sprite sprite;
        }

        [SerializeField] private LocaleSprite[] sprites;
        [SerializeField] private Sprite fallbackSprite;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            LocalizationManager.LocaleChanged += OnLocaleChanged;
            UpdateImage();
        }

        private void OnDisable()
        {
            LocalizationManager.LocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(LocaleChangedEventArgs args)
        {
            UpdateImage();
        }

        public void UpdateImage()
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_image == null || sprites == null) return;

            string currentCode = LocalizationManager.CurrentLocale.Code;

            foreach (var entry in sprites)
            {
                if (string.Equals(entry.localeCode, currentCode, StringComparison.OrdinalIgnoreCase))
                {
                    _image.sprite = entry.sprite;
                    return;
                }
            }

            if (fallbackSprite != null)
                _image.sprite = fallbackSprite;
        }
    }
}
