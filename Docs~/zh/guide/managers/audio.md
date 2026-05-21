# AudioManager

统一管理 BGM 与 SFX 播放、淡入淡出、静音以及 3D 空间音效的管理器。初始化时会自动创建 `DontDestroyOnLoad` 的 GameObject,并配置 1 个 BGM 用 AudioSource 和 SFX 用 8 通道池。

## DI 注册

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

访问时使用 `ServiceLocator`。

```csharp
using AchEngine.DI;
using AchEngine.Managers;

var audio = ServiceLocator.Resolve<AudioManager>();
```

## API

| 成员 | 说明 |
|---|---|
| `BgmVolume` | BGM 音量(0 ~ 1)。在静音状态下修改时仅更新内部值。 |
| `SfxVolume` | SFX 音量(0 ~ 1)。在静音状态下修改时仅更新内部值。 |
| `PlayBgm(clip)` | 立即播放 BGM。如果相同片段已在播放则忽略。 |
| `PlayBgm(clip, fadeDuration)` | 将当前 BGM 淡出,然后淡入新片段。`fadeDuration` 为整个切换时间(秒),默认值为 `0.5`。 |
| `StopBgm()` | 立即停止 BGM。 |
| `StopBgm(fadeDuration)` | 将 BGM 淡出后停止。默认值为 `0.5` 秒。 |
| `SetBgmVolume(volume, fadeDuration)` | 在指定时间内逐渐改变 BGM 音量。`fadeDuration` 为 `0` 时立即生效。 |
| `MuteBgm(bool)` | 设置 BGM 的静音状态。取消静音时恢复之前的音量。 |
| `MuteSfx(bool)` | 设置 SFX 的静音状态。取消静音时恢复之前的音量。 |
| `MuteAll(bool)` | 同时对 BGM 与 SFX 进行静音/取消静音。 |
| `PlaySfx(clip)` | 以 2D 形式播放 SFX。 |
| `PlaySfxAt(clip, worldPosition, volumeScale)` | 在指定世界坐标处以 3D 空间音效播放 SFX。`volumeScale` 默认值为 `1.0`。 |

## SFX 通道池

SFX 内部使用由 8 个 AudioSource 通道组成的池。

- 在播放请求时优先查找空闲通道。
- 当所有通道都在使用中时,以**轮询(round-robin)**方式覆盖最旧的通道。
- 通道池在场景切换后依然保持(`DontDestroyOnLoad`)。

## 静音行为

`MuteBgm` / `MuteSfx` **不会**修改内部音量值(`BgmVolume`、`SfxVolume`)。它们仅将 AudioSource 的实际输出设为 0,因此取消静音后会立即恢复到静音前的音量。

```csharp
audio.BgmVolume = 0.8f;
audio.MuteBgm(true);   // 실제 출력: 0, 내부 값: 0.8
audio.MuteBgm(false);  // 실제 출력: 0.8 복원
```

## 代码示例

### BGM 交叉淡变

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

### SFX 3D 播放

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// 폭발 위치에서 3D 사운드 재생
audio.PlaySfxAt(explosionClip, explosionObject.transform.position);

// 볼륨을 절반으로 줄여 재생
audio.PlaySfxAt(footstepClip, transform.position, volumeScale: 0.5f);

// 2D UI 효과음 재생
audio.PlaySfx(buttonClickClip);
```

### 静音切换(选项界面)

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// BGM만 음소거
audio.MuteBgm(true);

// SFX만 음소거
audio.MuteSfx(true);

// 전체 음소거 해제
audio.MuteAll(false);
```
