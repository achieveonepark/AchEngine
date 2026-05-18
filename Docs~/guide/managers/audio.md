# AudioManager

BGM 및 SFX 재생, 페이드, 뮤트, 3D 공간 음향을 통합 관리하는 매니저입니다. 초기화 시 `DontDestroyOnLoad` GameObject를 자동으로 생성하고 BGM용 AudioSource 1개와 SFX용 8채널 풀을 구성합니다.

## DI 등록

```csharp
// AchManagerInstaller가 자동으로 등록합니다.
// 선택 등록이 필요한 경우:
public class MyInstaller : AchManagerInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder.Register<AudioManager>();
    }
}
```

접근할 때는 `ServiceLocator`를 사용합니다.

```csharp
using AchEngine.DI;
using AchEngine.Managers;

var audio = ServiceLocator.Resolve<AudioManager>();
```

## API

| 멤버 | 설명 |
|---|---|
| `BgmVolume` | BGM 볼륨 (0 ~ 1). 뮤트 상태에서 변경해도 내부 값만 갱신됩니다. |
| `SfxVolume` | SFX 볼륨 (0 ~ 1). 뮤트 상태에서 변경해도 내부 값만 갱신됩니다. |
| `PlayBgm(clip)` | BGM을 즉시 재생합니다. 동일 클립이 이미 재생 중이면 무시합니다. |
| `PlayBgm(clip, fadeDuration)` | 현재 BGM을 페이드 아웃한 뒤 새 클립을 페이드 인합니다. `fadeDuration`은 전체 전환 시간(초)이며 기본값은 `0.5`입니다. |
| `StopBgm()` | BGM을 즉시 정지합니다. |
| `StopBgm(fadeDuration)` | BGM을 페이드 아웃 후 정지합니다. 기본값은 `0.5`초입니다. |
| `SetBgmVolume(volume, fadeDuration)` | BGM 볼륨을 지정한 시간 동안 서서히 변경합니다. `fadeDuration`이 `0`이면 즉시 적용됩니다. |
| `MuteBgm(bool)` | BGM 뮤트 상태를 설정합니다. 언뮤트 시 이전 볼륨이 복원됩니다. |
| `MuteSfx(bool)` | SFX 뮤트 상태를 설정합니다. 언뮤트 시 이전 볼륨이 복원됩니다. |
| `MuteAll(bool)` | BGM과 SFX를 모두 뮤트/언뮤트합니다. |
| `PlaySfx(clip)` | SFX를 2D로 재생합니다. |
| `PlaySfxAt(clip, worldPosition, volumeScale)` | 지정한 월드 좌표에서 3D 공간 음향으로 SFX를 재생합니다. `volumeScale` 기본값은 `1.0`입니다. |

## SFX 채널 풀

SFX는 내부적으로 8개의 AudioSource 채널로 구성된 풀을 사용합니다.

- 재생 요청 시 비어 있는 채널을 우선 탐색합니다.
- 모든 채널이 사용 중이면 **라운드로빈** 방식으로 가장 오래된 채널을 덮어씁니다.
- 채널 풀은 씬 전환 후에도 유지됩니다(`DontDestroyOnLoad`).

## 뮤트 동작

`MuteBgm` / `MuteSfx`는 내부 볼륨 값(`BgmVolume`, `SfxVolume`)을 **변경하지 않습니다**. AudioSource의 실제 출력만 0으로 만들기 때문에 언뮤트하면 뮤트 이전 볼륨으로 즉시 복원됩니다.

```csharp
audio.BgmVolume = 0.8f;
audio.MuteBgm(true);   // 실제 출력: 0, 내부 값: 0.8
audio.MuteBgm(false);  // 실제 출력: 0.8 복원
```

## 코드 예시

### BGM 크로스페이드

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// 씬 진입 시 1초 페이드로 BGM 전환
audio.PlayBgm(lobbyBgmClip, fadeDuration: 1f);

// 보스 전투 구간 — 빠른 전환 (0.3초)
audio.PlayBgm(bossBgmClip, fadeDuration: 0.3f);

// 2초에 걸쳐 볼륨을 낮춤 (컷씬 연출 등)
audio.SetBgmVolume(0.3f, fadeDuration: 2f);

// 씬 종료 시 페이드 아웃
audio.StopBgm(fadeDuration: 1.5f);
```

### SFX 3D 재생

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// 폭발 위치에서 3D 사운드 재생
audio.PlaySfxAt(explosionClip, explosionObject.transform.position);

// 볼륨을 절반으로 줄여 재생
audio.PlaySfxAt(footstepClip, transform.position, volumeScale: 0.5f);

// 2D UI 효과음 재생
audio.PlaySfx(buttonClickClip);
```

### 뮤트 토글 (옵션 화면)

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// BGM만 음소거
audio.MuteBgm(true);

// SFX만 음소거
audio.MuteSfx(true);

// 전체 음소거 해제
audio.MuteAll(false);
```
