using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.Samples.Full.Data;
using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples.Full.UI
{
    /// <summary>
    /// UIViewCatalog에 "Settings" ID로 등록하세요. Layer: Popup.
    /// </summary>
    public class SettingsPopup : UIView
    {
        [Header("Sliders")]
        [SerializeField] private Slider sliderBgm;
        [SerializeField] private Slider sliderSfx;

        [Header("Buttons")]
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnSave;

        protected override void OnInitialize()
        {
            btnClose?.onClick.AddListener(CloseSelf);
            btnSave?.onClick.AddListener(OnSaveClicked);

            sliderBgm?.onValueChanged.AddListener(OnBgmChanged);
            sliderSfx?.onValueChanged.AddListener(OnSfxChanged);
        }

        protected override void OnOpened(object payload)
        {
            var cm = ServiceLocator.Get<ConfigManager>();

            if (sliderBgm != null)
                sliderBgm.value = cm.GetConfig<float>(GameConfig.KeyBgmVolume, 0.7f);
            if (sliderSfx != null)
                sliderSfx.value = cm.GetConfig<float>(GameConfig.KeySfxVolume, 1.0f);
        }

        private void OnBgmChanged(float value)
            => ServiceLocator.Get<SoundManager>().BgmVolume = value;

        private void OnSfxChanged(float value)
            => ServiceLocator.Get<SoundManager>().SfxVolume = value;

        private void OnSaveClicked()
        {
            var cm    = ServiceLocator.Get<ConfigManager>();
            var sound = ServiceLocator.Get<SoundManager>();

            cm.SetConfig(GameConfig.KeyBgmVolume, sound.BgmVolume);
            cm.SetConfig(GameConfig.KeySfxVolume, sound.SfxVolume);

            CloseSelf();
        }
    }
}
