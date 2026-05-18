# AudioManager

A unified manager for BGM and SFX playback, fade transitions, mute control, and 3D spatial audio. On initialization it automatically creates a `DontDestroyOnLoad` GameObject with one BGM `AudioSource` and an 8-channel SFX pool.

## DI Registration

```csharp
// AchManagerInstaller registers AudioManager automatically.
// For selective registration:
public class MyInstaller : AchManagerInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder.Register<AudioManager>();
    }
}
```

Access the manager anywhere via `ServiceLocator`:

```csharp
using AchEngine.DI;
using AchEngine.Managers;

var audio = ServiceLocator.Resolve<AudioManager>();
```

## API

| Member | Description |
|---|---|
| `BgmVolume` | BGM volume (0 – 1). When muted, only the internal value is updated. |
| `SfxVolume` | SFX volume (0 – 1). When muted, only the internal value is updated. |
| `PlayBgm(clip)` | Plays a BGM clip immediately. Ignored if the same clip is already playing. |
| `PlayBgm(clip, fadeDuration)` | Fades out the current BGM then fades in the new clip. `fadeDuration` is the total crossfade time in seconds (default `0.5`). |
| `StopBgm()` | Stops the BGM immediately. |
| `StopBgm(fadeDuration)` | Fades out the BGM then stops it. Default fade is `0.5` seconds. |
| `SetBgmVolume(volume, fadeDuration)` | Gradually changes the BGM volume over `fadeDuration` seconds. Pass `0` for an instant change. |
| `MuteBgm(bool)` | Mutes or unmutes BGM. Unmuting restores the previous volume. |
| `MuteSfx(bool)` | Mutes or unmutes SFX. Unmuting restores the previous volume. |
| `MuteAll(bool)` | Mutes or unmutes both BGM and SFX. |
| `PlaySfx(clip)` | Plays a 2D SFX clip. |
| `PlaySfxAt(clip, worldPosition, volumeScale)` | Plays a 3D spatial SFX clip at the given world position. `volumeScale` defaults to `1.0`. |

## SFX Channel Pool

SFX playback uses an internal pool of 8 `AudioSource` channels.

- On each play request, the manager first looks for a channel that is not currently playing.
- If all 8 channels are busy, the oldest channel is reused via **round-robin**.
- The pool persists across scene loads (`DontDestroyOnLoad`).

## Mute Behavior

`MuteBgm` / `MuteSfx` do **not** modify the internal volume values (`BgmVolume`, `SfxVolume`). They set the `AudioSource` output to `0` without touching the stored value, so unmuting instantly restores the original volume.

```csharp
audio.BgmVolume = 0.8f;
audio.MuteBgm(true);   // audible output: 0, internal value: 0.8
audio.MuteBgm(false);  // audible output restored to 0.8
```

## Code Examples

### BGM Crossfade

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// Transition to lobby music with a 1-second crossfade
audio.PlayBgm(lobbyBgmClip, fadeDuration: 1f);

// Quick switch to boss music (0.3 s)
audio.PlayBgm(bossBgmClip, fadeDuration: 0.3f);

// Slowly lower volume over 2 seconds (e.g. cutscene)
audio.SetBgmVolume(0.3f, fadeDuration: 2f);

// Fade out when leaving the scene
audio.StopBgm(fadeDuration: 1.5f);
```

### 3D Spatial SFX

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// Play explosion sound at the impact position
audio.PlaySfxAt(explosionClip, explosionObject.transform.position);

// Play at half volume
audio.PlaySfxAt(footstepClip, transform.position, volumeScale: 0.5f);

// Play a 2D UI sound effect
audio.PlaySfx(buttonClickClip);
```

### Mute Toggle (Options Screen)

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// Mute BGM only
audio.MuteBgm(true);

// Mute SFX only
audio.MuteSfx(true);

// Unmute everything
audio.MuteAll(false);
```
