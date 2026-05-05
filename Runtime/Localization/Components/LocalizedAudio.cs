using System;
using UnityEngine;

namespace AchEngine.Localization
{
    /// <summary>
    /// locale에 따라 AudioSource의 AudioClip을 자동 교체하는 컴포넌트
    /// </summary>
    [AddComponentMenu("Achieve/Localization/Localized Audio")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class LocalizedAudio : MonoBehaviour
    {
        [Serializable]
        public struct LocaleAudioClip
        {
            public string localeCode;
            public AudioClip clip;
        }

        [SerializeField] private LocaleAudioClip[] clips;
        [SerializeField] private AudioClip fallbackClip;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            LocalizationManager.LocaleChanged += OnLocaleChanged;
            UpdateAudio();
        }

        private void OnDisable()
        {
            LocalizationManager.LocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(LocaleChangedEventArgs args)
        {
            UpdateAudio();
        }

        public void UpdateAudio()
        {
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null || clips == null) return;

            string currentCode = LocalizationManager.CurrentLocale.Code;

            foreach (var entry in clips)
            {
                if (string.Equals(entry.localeCode, currentCode, StringComparison.OrdinalIgnoreCase))
                {
                    _audioSource.clip = entry.clip;
                    return;
                }
            }

            if (fallbackClip != null)
                _audioSource.clip = fallbackClip;
        }
    }
}
