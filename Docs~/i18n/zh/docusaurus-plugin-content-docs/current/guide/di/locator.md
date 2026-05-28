# ServiceLocator

:::warning 仅限未安装 VContainer 时使用
在定义了 `ACHENGINE_VCONTAINER` 符号的环境下（已安装 VContainer），`ServiceLocator` 不会被编译。
在 VContainer 项目中，请使用 `[Inject]`。
:::

`ServiceLocator` 是在不使用 VContainer 的情况下，在运行时按类型查询服务的静态门面。

## API

```csharp
namespace AchEngine.DI
{
    public static class ServiceLocator
    {
        // 컨테이너가 준비되었는지 여부
        public static bool IsReady { get; }

        // 서비스 조회 (없으면 InvalidOperationException)
        public static T Resolve<T>();

        // 안전한 서비스 조회 (없으면 false 반환)
        public static bool TryResolve<T>(out T result);
    }
}
```

## 使用示例

```csharp
// 기본 조회
var ui = ServiceLocator.Resolve<IUIService>();
ui.Show<MainMenuView>();

// 안전한 조회
if (ServiceLocator.TryResolve<IAudioService>(out var audio))
{
    audio.PlayBGM("main_theme");
}

// 준비 여부 확인
if (!ServiceLocator.IsReady)
{
    Debug.LogWarning("서비스 컨테이너가 아직 초기화되지 않았습니다.");
    return;
}
```

## `[Inject]` vs `ServiceLocator`

| | `[Inject]` | `ServiceLocator` |
|---|---|---|
| VContainer | ✅ 必需 | ❌ 仅在未安装 VContainer 时可用 |
| 使用位置 | 由 DI 容器创建的对象 | 任意位置（不使用 VContainer 的环境） |
| 推荐场景 | 所有服务和 View | 不使用 VContainer 的 MonoBehaviour |
| 测试便利性 | 高 | 中 |

## 手动初始化（无 VContainer 时）

若要在没有 VContainer 的情况下使用 `ServiceLocator`，引擎内部会调用 `Setup()`。
未提供可直接调用的公共 API，由 `AchEngineScope` 等内部引导代码
在容器就绪时自动建立连接。

:::info 非 VContainer 环境
在不定义 `ACHENGINE_VCONTAINER` 符号的情况下构建时，可实现自定义 `AchEngineInstaller`，
将服务注册到 `IServiceBuilder`，便可在运行时通过 `ServiceLocator.Resolve<T>()` 访问。
:::

## 相关文档

- [DI 系统概述](/guide/di/index)
- [DI 生命周期设计](/guide/di/lifecycle)
