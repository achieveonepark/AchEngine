using UnityEngine;
#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine.Managers
{
    public class SoundManager : IManager
    {
        private AudioSource _bgmSource;
        private AudioSource _sfxSource;
        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;

#if ACHENGINE_UNITASK
        public UniTask Initialize()
        {
            SetupAudioSources();
            return UniTask.CompletedTask;
        }
#else
        public Task Initialize()
        {
            SetupAudioSources();
            return Task.CompletedTask;
        }
#endif

        private void SetupAudioSources()
        {
            var go = new GameObject("[AchEngine] SoundManager");
            Object.DontDestroyOnLoad(go);
            _bgmSource = go.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _sfxSource = go.AddComponent<AudioSource>();
        }

        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                if (_bgmSource != null) _bgmSource.volume = _bgmVolume;
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                if (_sfxSource != null) _sfxSource.volume = _sfxVolume;
            }
        }

        public void PlayBgm(AudioClip clip)
        {
            if (_bgmSource == null || clip == null) return;
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }

        public void StopBgm()
        {
            _bgmSource?.Stop();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (_sfxSource == null || clip == null) return;
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }
    }
}
