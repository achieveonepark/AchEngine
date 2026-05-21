# 管理器系统

AchEngine 的管理器系统通过 DI 注册游戏中反复需要的功能,并通过 `ServiceLocator` 在任意位置进行访问。

## 提供的管理器

| 管理器 | 职责 |
|---|---|
| `ConfigManager` | 基于 PlayerPrefs 的设置值保存/读取 |
| `AudioManager` | BGM / SFX 播放、淡入淡出、静音、3D 空间音效 |
| `AchSceneManager` | 异步场景切换,IScene 生命周期 |
| `InputManager` | 输入启用/禁用封装 |
| `TimeManager` | 网络同步时间,每秒事件 |
| `PoolManager` | 基于预制体的对象池 |
| `PlayerManager` | 玩家数据容器管理(`Add` / `Get` / `Remove`) |
| `SaveManager` | 保存・读取・删除 — 基于 `ISaveService` 的抽象(需要单独注册 DI) |
| `IAPManager` | Unity IAP 5.3.0 集成 stub(需要单独注册 DI) |

## 快速开始

### 1. 添加 AchManagerInstaller

将 `AchManagerInstaller` 组件添加到 `AchEngineScope` 的 Installers 数组中。

```csharp
// 씬의 AchEngineScope GameObject에 AchManagerInstaller 컴포넌트를 추가하기만 하면 됩니다.
// 모든 매니저가 자동으로 DI 컨테이너에 싱글톤으로 등록됩니다.
```

如需选择性注册,请继承 `AchManagerInstaller`:

```csharp
public class MyInstaller : AchManagerInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder
            .Register<ConfigManager>()
            .Register<AudioManager>();
        // 필요한 것만 등록
    }
}
```

### 2. 访问管理器

```csharp
using AchEngine.DI;
using AchEngine.Managers;

var config = ServiceLocator.Resolve<ConfigManager>();
var audio  = ServiceLocator.Resolve<AudioManager>();
```

## IScene 生命周期

在场景的根 GameObject 上挂载实现 `IScene` 的 MonoBehaviour 后,`AchSceneManager` 会在场景切换时自动调用 `OnSceneStart` / `OnSceneEnd`。

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

> 运行时异步 API 使用 `System.Threading.Tasks.Task`。
