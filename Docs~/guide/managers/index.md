# 매니저 시스템

AchEngine의 매니저 시스템은 게임에서 반복적으로 필요한 기능들을 DI로 등록하고, `ServiceLocator`를 통해 어디서든 접근할 수 있게 합니다.

## 제공 매니저

| 매니저 | 역할 |
|---|---|
| `ConfigManager` | PlayerPrefs 기반 설정값 저장/불러오기 |
| `SoundManager` | BGM / SFX 재생 및 볼륨 제어 |
| `AchSceneManager` | 비동기 씬 전환, IScene 라이프사이클 |
| `InputManager` | 입력 활성화/비활성화 래퍼 |
| `TimeManager` | 네트워크 동기화 시간, 1초 이벤트 |
| `PoolManager` | 프리팹 기반 오브젝트 풀링 |
| `PlayerManager` | 플레이어 데이터 컨테이너 관리, QuickSave |
| `IAPManager` | Unity IAP 5.3.0 연동 stub |

## 빠른 시작

### 1. AchManagerInstaller 추가

`AchEngineScope`의 Installers 배열에 `AchManagerInstaller` 컴포넌트를 추가합니다.

```csharp
// 씬의 AchEngineScope GameObject에 AchManagerInstaller 컴포넌트를 추가하기만 하면 됩니다.
// 모든 매니저가 자동으로 DI 컨테이너에 싱글톤으로 등록됩니다.
```

선택적으로 등록하려면 `AchManagerInstaller`를 상속하세요:

```csharp
public class MyInstaller : AchManagerInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder
            .Register<ConfigManager>()
            .Register<SoundManager>();
        // 필요한 것만 등록
    }
}
```

### 2. 매니저 접근

```csharp
using AchEngine.DI;
using AchEngine.Managers;

var config = ServiceLocator.Get<ConfigManager>();
var sound  = ServiceLocator.Get<SoundManager>();
```

## IScene 라이프사이클

씬의 루트 GameObject에 `IScene`을 구현하는 MonoBehaviour를 붙이면 `AchSceneManager`가 씬 전환 시 자동으로 `OnSceneStart` / `OnSceneEnd`를 호출합니다.

```csharp
using AchEngine.Managers;

public class LobbyScene : MonoBehaviour, IScene
{
    public async Task OnSceneStart()
    {
        await LoadUserDataAsync();
    }

    public Task OnSceneEnd() => Task.CompletedTask;
}
```

> 런타임 async API는 `System.Threading.Tasks.Task`를 사용합니다.
