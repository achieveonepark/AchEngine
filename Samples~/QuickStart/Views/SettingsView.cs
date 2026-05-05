using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples
{
    public class SettingsView : UIView
    {
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button btnClose;
        [SerializeField] private Text bgmValueText;
        [SerializeField] private Text sfxValueText;

        protected override void OnInitialize()
        {
            btnClose?.onClick.AddListener(CloseSelf);

            bgmSlider?.onValueChanged.AddListener(value =>
            {
                if (bgmValueText != null)
                {
                    bgmValueText.text = $"{(int)(value * 100)}%";
                }
            });

            sfxSlider?.onValueChanged.AddListener(value =>
            {
                if (sfxValueText != null)
                {
                    sfxValueText.text = $"{(int)(value * 100)}%";
                }
            });
        }

        protected override void OnOpened(object payload)
        {
            if (payload is float bgm && bgmSlider != null)
            {
                bgmSlider.value = bgm;
            }
        }
    }
}
