using UnityEngine;

#if ACHENGINE_LOCALIZATION_TMP
using TMPro;
#endif

namespace AchEngine.Localization
{
    /// <summary>
    /// Text 또는 TextMeshPro 컴포넌트의 텍스트를 locale 변경 시 자동 업데이트하는 컴포넌트
    /// </summary>
    [AddComponentMenu("Achieve/Localization/Localized Text")]
    [DisallowMultipleComponent]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private LocalizedString localizedString;
        [SerializeField] private string[] formatArgs;

        private enum TextComponentType { None, UnityText, TextMeshPro }

        private TextComponentType _componentType = TextComponentType.None;
        private Component _textComponent;

        public LocalizedString LocalizedString
        {
            get => localizedString;
            set
            {
                localizedString = value;
                UpdateText();
            }
        }

        private void Awake()
        {
            DetectTextComponent();
        }

        private void OnEnable()
        {
            LocalizationManager.LocaleChanged += OnLocaleChanged;
            UpdateText();
        }

        private void OnDisable()
        {
            LocalizationManager.LocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(LocaleChangedEventArgs args)
        {
            UpdateText();
        }

        /// <summary>
        /// 키를 설정하고 텍스트를 업데이트
        /// </summary>
        public void SetKey(string key)
        {
            localizedString = new LocalizedString(key);
            UpdateText();
        }

        /// <summary>
        /// 포맷 인자를 설정하고 텍스트를 업데이트
        /// </summary>
        public void SetFormatArgs(params string[] args)
        {
            formatArgs = args;
            UpdateText();
        }

        /// <summary>
        /// 현재 locale에 맞게 텍스트를 업데이트
        /// </summary>
        public void UpdateText()
        {
            if (!localizedString.IsValid) return;
            if (_textComponent == null) DetectTextComponent();
            if (_textComponent == null) return;

            string text;
            if (formatArgs != null && formatArgs.Length > 0)
            {
                object[] args = new object[formatArgs.Length];
                for (int i = 0; i < formatArgs.Length; i++)
                    args[i] = formatArgs[i];
                text = localizedString.GetValue(args);
            }
            else
            {
                text = localizedString.Value;
            }

            SetText(text);
        }

        private void DetectTextComponent()
        {
#if ACHENGINE_LOCALIZATION_TMP
            var tmp = GetComponent<TMP_Text>();
            if (tmp != null)
            {
                _textComponent = tmp;
                _componentType = TextComponentType.TextMeshPro;
                return;
            }
#endif
            var unityText = GetComponent<UnityEngine.UI.Text>();
            if (unityText != null)
            {
                _textComponent = unityText;
                _componentType = TextComponentType.UnityText;
            }
        }

        private void SetText(string text)
        {
            switch (_componentType)
            {
#if ACHENGINE_LOCALIZATION_TMP
                case TextComponentType.TextMeshPro:
                    ((TMP_Text)_textComponent).text = text;
                    break;
#endif
                case TextComponentType.UnityText:
                    ((UnityEngine.UI.Text)_textComponent).text = text;
                    break;
            }
        }
    }
}
