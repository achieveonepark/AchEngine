using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace AchEngine.Managers
{
    /// <summary>
    /// BGM 및 SFX 재생, 페이드, 뮤트, 공간 음향을 통합 관리하는 오디오 매니저.
    /// </summary>
    public class AudioManager : IManager
    {
        // BGM 재생에 사용하는 AudioSource
        private AudioSource _bgmSource;

        // SFX 채널 풀 (동시 재생 지원)
        private AudioSource[] _sfxPool;

        // SFX 풀 크기
        private const int SfxPoolSize = 8;

        // 현재 SFX 풀에서 사용할 채널 인덱스
        private int _sfxPoolIndex;

        // BGM 볼륨 (0~1)
        private float _bgmVolume = 1f;

        // SFX 볼륨 (0~1)
        private float _sfxVolume = 1f;

        // BGM 뮤트 여부
        private bool _bgmMuted;

        // SFX 뮤트 여부
        private bool _sfxMuted;

        // 코루틴 실행을 위한 MonoBehaviour 헬퍼
        private AudioManagerBehaviour _behaviour;

        // 현재 실행 중인 BGM 페이드 코루틴 참조 (중복 방지)
        private Coroutine _bgmFadeCoroutine;

        // 현재 실행 중인 BGM 볼륨 페이드 코루틴 참조 (중복 방지)
        private Coroutine _bgmVolumeCoroutine;

        /// <summary>
        /// BGM 볼륨 프로퍼티. 값은 0~1로 클램프된다.
        /// </summary>
        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                // 뮤트 상태가 아닐 때만 실제 볼륨에 반영
                if (_bgmSource != null && !_bgmMuted)
                    _bgmSource.volume = _bgmVolume;
            }
        }

        /// <summary>
        /// SFX 볼륨 프로퍼티. 값은 0~1로 클램프된다.
        /// </summary>
        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                // 뮤트 상태가 아닐 때만 SFX 풀 전체에 반영
                if (!_sfxMuted)
                    ApplySfxVolumeToPool(_sfxVolume);
            }
        }

        // ─────────────────────────────────────────────
        // IManager 초기화
        // ─────────────────────────────────────────────

        public Task Initialize()
        {
            SetupAudioSources();
            return Task.CompletedTask;
        }

        /// <summary>
        /// DontDestroyOnLoad GameObject에 오디오 소스 및 MonoBehaviour 헬퍼를 구성한다.
        /// </summary>
        private void SetupAudioSources()
        {
            var go = new GameObject("[AchEngine] AudioManager");
            Object.DontDestroyOnLoad(go);

            // MonoBehaviour 헬퍼 추가 (코루틴 전용)
            _behaviour = go.AddComponent<AudioManagerBehaviour>();

            // BGM 소스 설정: 루프 재생
            _bgmSource = go.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.volume = _bgmVolume;

            // SFX 풀 생성: 8개의 채널로 동시 재생 지원
            _sfxPool = new AudioSource[SfxPoolSize];
            for (int i = 0; i < SfxPoolSize; i++)
            {
                var sfxSource = go.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.volume = _sfxVolume;
                _sfxPool[i] = sfxSource;
            }
        }

        // ─────────────────────────────────────────────
        // BGM 재생 / 정지
        // ─────────────────────────────────────────────

        /// <summary>
        /// BGM을 즉시 재생한다 (페이드 없음).
        /// 동일 클립이 이미 재생 중이면 무시한다.
        /// </summary>
        public void PlayBgm(AudioClip clip)
        {
            if (_bgmSource == null || clip == null) return;
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

            _bgmSource.clip = clip;
            _bgmSource.volume = _bgmMuted ? 0f : _bgmVolume;
            _bgmSource.Play();
        }

        /// <summary>
        /// BGM을 페이드 인으로 전환한다.
        /// 현재 재생 중인 BGM이 있으면 페이드 아웃 후 새 클립을 페이드 인한다.
        /// </summary>
        public void PlayBgm(AudioClip clip, float fadeDuration = 0.5f)
        {
            if (_bgmSource == null || clip == null) return;

            // 이미 같은 클립을 재생 중이면 무시
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

            // 진행 중인 페이드 코루틴 중단
            if (_bgmFadeCoroutine != null)
                _behaviour.StopCoroutine(_bgmFadeCoroutine);

            _bgmFadeCoroutine = _behaviour.StartCoroutine(
                CrossFadeBgmCoroutine(clip, fadeDuration));
        }

        /// <summary>
        /// BGM을 즉시 정지한다 (페이드 없음).
        /// </summary>
        public void StopBgm()
        {
            if (_bgmFadeCoroutine != null)
            {
                _behaviour.StopCoroutine(_bgmFadeCoroutine);
                _bgmFadeCoroutine = null;
            }
            _bgmSource?.Stop();
        }

        /// <summary>
        /// BGM을 페이드 아웃 후 정지한다.
        /// </summary>
        public void StopBgm(float fadeDuration = 0.5f)
        {
            if (_bgmSource == null || !_bgmSource.isPlaying) return;

            if (_bgmFadeCoroutine != null)
                _behaviour.StopCoroutine(_bgmFadeCoroutine);

            _bgmFadeCoroutine = _behaviour.StartCoroutine(
                FadeOutBgmCoroutine(fadeDuration));
        }

        // ─────────────────────────────────────────────
        // BGM 볼륨 페이드
        // ─────────────────────────────────────────────

        /// <summary>
        /// BGM 볼륨을 지정한 시간 동안 목표 값으로 서서히 변경한다.
        /// fadeDuration이 0이면 즉시 적용한다.
        /// </summary>
        public void SetBgmVolume(float volume, float fadeDuration = 0f)
        {
            _bgmVolume = Mathf.Clamp01(volume);

            if (_bgmSource == null) return;

            // 뮤트 상태에서는 내부 값만 갱신하고 실제 소스는 건드리지 않는다
            if (_bgmMuted) return;

            // 진행 중인 볼륨 페이드 중단
            if (_bgmVolumeCoroutine != null)
                _behaviour.StopCoroutine(_bgmVolumeCoroutine);

            if (fadeDuration <= 0f)
            {
                _bgmSource.volume = _bgmVolume;
            }
            else
            {
                _bgmVolumeCoroutine = _behaviour.StartCoroutine(
                    FadeBgmVolumeCoroutine(_bgmSource.volume, _bgmVolume, fadeDuration));
            }
        }

        // ─────────────────────────────────────────────
        // SFX 재생
        // ─────────────────────────────────────────────

        /// <summary>
        /// SFX를 풀에서 빈 채널을 찾아 재생한다.
        /// 모든 채널이 사용 중이면 라운드로빈 방식으로 가장 오래된 채널을 덮어쓴다.
        /// </summary>
        public void PlaySfx(AudioClip clip)
        {
            if (clip == null) return;

            var source = GetAvailableSfxSource();
            source.spatialBlend = 0f; // 2D 재생
            source.transform.localPosition = Vector3.zero;
            source.volume = _sfxMuted ? 0f : _sfxVolume;
            source.PlayOneShot(clip);
        }

        /// <summary>
        /// 월드 좌표 위치에서 3D 공간 음향으로 SFX를 재생한다.
        /// </summary>
        public void PlaySfxAt(AudioClip clip, Vector3 worldPosition, float volumeScale = 1f)
        {
            if (clip == null) return;

            var source = GetAvailableSfxSource();
            source.transform.position = worldPosition;
            source.spatialBlend = 1f; // 3D 재생
            source.volume = _sfxMuted ? 0f : _sfxVolume * Mathf.Clamp01(volumeScale);
            source.PlayOneShot(clip);
        }

        /// <summary>
        /// 풀에서 재생 중이 아닌 채널을 반환한다.
        /// 모두 사용 중이면 라운드로빈으로 다음 채널을 반환한다.
        /// </summary>
        private AudioSource GetAvailableSfxSource()
        {
            // 재생 중이지 않은 채널을 우선 탐색
            for (int i = 0; i < SfxPoolSize; i++)
            {
                if (!_sfxPool[i].isPlaying)
                    return _sfxPool[i];
            }

            // 모두 사용 중이면 라운드로빈
            var source = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % SfxPoolSize;
            return source;
        }

        // ─────────────────────────────────────────────
        // 뮤트
        // ─────────────────────────────────────────────

        /// <summary>
        /// BGM 뮤트 상태를 설정한다.
        /// </summary>
        public void MuteBgm(bool mute)
        {
            _bgmMuted = mute;
            if (_bgmSource != null)
                _bgmSource.volume = mute ? 0f : _bgmVolume;
        }

        /// <summary>
        /// SFX 뮤트 상태를 설정한다.
        /// </summary>
        public void MuteSfx(bool mute)
        {
            _sfxMuted = mute;
            ApplySfxVolumeToPool(mute ? 0f : _sfxVolume);
        }

        /// <summary>
        /// BGM과 SFX를 모두 뮤트/언뮤트한다.
        /// </summary>
        public void MuteAll(bool mute)
        {
            MuteBgm(mute);
            MuteSfx(mute);
        }

        /// <summary>
        /// SFX 풀 전체에 지정한 볼륨을 적용한다.
        /// </summary>
        private void ApplySfxVolumeToPool(float volume)
        {
            if (_sfxPool == null) return;
            foreach (var source in _sfxPool)
            {
                if (source != null)
                    source.volume = volume;
            }
        }

        // ─────────────────────────────────────────────
        // 코루틴 (페이드 로직)
        // ─────────────────────────────────────────────

        /// <summary>
        /// 현재 BGM을 페이드 아웃한 뒤 새 클립을 페이드 인한다.
        /// </summary>
        private IEnumerator CrossFadeBgmCoroutine(AudioClip newClip, float fadeDuration)
        {
            float halfDuration = fadeDuration * 0.5f;

            // 재생 중인 BGM이 있으면 페이드 아웃
            if (_bgmSource.isPlaying && halfDuration > 0f)
            {
                float startVolume = _bgmSource.volume;
                float elapsed = 0f;

                while (elapsed < halfDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / halfDuration);
                    yield return null;
                }
            }

            // 클립 교체 후 페이드 인
            _bgmSource.Stop();
            _bgmSource.clip = newClip;
            _bgmSource.volume = 0f;
            _bgmSource.Play();

            float targetVolume = _bgmMuted ? 0f : _bgmVolume;

            if (halfDuration > 0f)
            {
                float elapsed = 0f;

                while (elapsed < halfDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / halfDuration);
                    yield return null;
                }
            }

            _bgmSource.volume = targetVolume;
            _bgmFadeCoroutine = null;
        }

        /// <summary>
        /// BGM을 서서히 페이드 아웃한 뒤 정지한다.
        /// </summary>
        private IEnumerator FadeOutBgmCoroutine(float fadeDuration)
        {
            float startVolume = _bgmSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                yield return null;
            }

            _bgmSource.volume = 0f;
            _bgmSource.Stop();
            _bgmFadeCoroutine = null;
        }

        /// <summary>
        /// BGM 볼륨을 시작 값에서 목표 값으로 서서히 변경한다.
        /// </summary>
        private IEnumerator FadeBgmVolumeCoroutine(float from, float to, float fadeDuration)
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _bgmSource.volume = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }

            _bgmSource.volume = to;
            _bgmVolumeCoroutine = null;
        }

        // ─────────────────────────────────────────────
        // 내부 MonoBehaviour 헬퍼
        // ─────────────────────────────────────────────

        /// <summary>
        /// AudioManager의 코루틴 실행을 위한 전용 MonoBehaviour 헬퍼.
        /// AudioManager가 순수 클래스이므로 코루틴을 직접 실행할 수 없어 이 컴포넌트에 위임한다.
        /// </summary>
        private class AudioManagerBehaviour : MonoBehaviour { }
    }
}
