# AudioManager

BGM および SFX の再生、フェード、ミュート、3D 空間音響を統合的に管理するマネージャーです。初期化時に `DontDestroyOnLoad` の GameObject を自動生成し、BGM 用 AudioSource を 1 つと SFX 用 8 チャンネルプールを構成します。

## DI 登録

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

アクセスには `ServiceLocator` を使用します。

```csharp
using AchEngine.DI;
using AchEngine.Managers;

var audio = ServiceLocator.Resolve<AudioManager>();
```

## API

| メンバー | 説明 |
|---|---|
| `BgmVolume` | BGM 音量(0 〜 1)。ミュート状態で変更しても内部値のみが更新されます。 |
| `SfxVolume` | SFX 音量(0 〜 1)。ミュート状態で変更しても内部値のみが更新されます。 |
| `PlayBgm(clip)` | BGM を即座に再生します。同じクリップがすでに再生中であれば無視されます。 |
| `PlayBgm(clip, fadeDuration)` | 現在の BGM をフェードアウトしてから新しいクリップをフェードインします。`fadeDuration` は遷移全体の時間(秒)で、デフォルトは `0.5` です。 |
| `StopBgm()` | BGM を即座に停止します。 |
| `StopBgm(fadeDuration)` | BGM をフェードアウトしてから停止します。デフォルトは `0.5` 秒です。 |
| `SetBgmVolume(volume, fadeDuration)` | 指定時間をかけて BGM 音量を徐々に変更します。`fadeDuration` が `0` の場合は即座に適用されます。 |
| `MuteBgm(bool)` | BGM のミュート状態を設定します。ミュート解除時には以前の音量が復元されます。 |
| `MuteSfx(bool)` | SFX のミュート状態を設定します。ミュート解除時には以前の音量が復元されます。 |
| `MuteAll(bool)` | BGM と SFX をまとめてミュート/ミュート解除します。 |
| `PlaySfx(clip)` | SFX を 2D で再生します。 |
| `PlaySfxAt(clip, worldPosition, volumeScale)` | 指定したワールド座標で 3D 空間音響として SFX を再生します。`volumeScale` のデフォルトは `1.0` です。 |

## SFX チャンネルプール

SFX は内部的に 8 つの AudioSource チャンネルで構成されたプールを使用します。

- 再生リクエスト時に空きチャンネルを優先して探索します。
- すべてのチャンネルが使用中の場合、**ラウンドロビン**方式で最も古いチャンネルを上書きします。
- チャンネルプールはシーン遷移後も維持されます(`DontDestroyOnLoad`)。

## ミュート動作

`MuteBgm` / `MuteSfx` は内部音量値(`BgmVolume`、`SfxVolume`)を**変更しません**。AudioSource の実際の出力のみを 0 にするため、ミュート解除するとミュート前の音量に即座に復元されます。

```csharp
audio.BgmVolume = 0.8f;
audio.MuteBgm(true);   // 실제 출력: 0, 내부 값: 0.8
audio.MuteBgm(false);  // 실제 출력: 0.8 복원
```

## コード例

### BGM クロスフェード

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

### SFX 3D 再生

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// 폭발 위치에서 3D 사운드 재생
audio.PlaySfxAt(explosionClip, explosionObject.transform.position);

// 볼륨을 절반으로 줄여 재생
audio.PlaySfxAt(footstepClip, transform.position, volumeScale: 0.5f);

// 2D UI 효과음 재생
audio.PlaySfx(buttonClickClip);
```

### ミュートトグル(オプション画面)

```csharp
var audio = ServiceLocator.Resolve<AudioManager>();

// BGM만 음소거
audio.MuteBgm(true);

// SFX만 음소거
audio.MuteSfx(true);

// 전체 음소거 해제
audio.MuteAll(false);
```
