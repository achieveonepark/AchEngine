# SoundManager

BGM과 SFX를 관리하는 매니저입니다. 초기화 시 `DontDestroyOnLoad` GameObject를 자동으로 생성하고 AudioSource를 붙입니다.

## API

```csharp
var sound = ServiceLocator.Get<SoundManager>();

// 볼륨 설정 (0 ~ 1)
sound.BgmVolume = 0.8f;
sound.SfxVolume = 1.0f;

// BGM 재생 / 정지
sound.PlayBgm(bgmClip);
sound.StopBgm();

// SFX 재생 (PlayOneShot)
sound.PlaySfx(clickClip);
```
