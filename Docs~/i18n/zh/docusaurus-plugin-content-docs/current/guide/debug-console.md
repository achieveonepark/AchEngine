# AchDebugConsole

`AchDebugConsole` 是渲染在 Android、iOS 原生 UI 层上的调试控制台。
它与 Unity 主线程完全独立,因此即使在性能分析过程中也可以安全使用。
在编辑器中则以 IMGUI 回退方式运行。

## 概述

通过 `Application.logMessageReceivedThreaded` 自动接收 Unity 的所有日志(`Log`、`Warning`、`Error`、`Exception`、`Assert`)。
无需额外的初始化代码,通过 `[RuntimeInitializeOnLoadMethod]` 自动注册,
游戏启动的同时即开始收集日志。

| 项目 | 内容 |
|---|---|
| 最大日志保留数 | 500 条(环形缓冲区) |
| 日志颜色 | Error·Exception·Assert → 红色 / Warning → 黄色 / Log → 白色 |
| 线程安全 | 所有平台均内部分派至 UI 线程处理 |

## 各平台行为

### Android

使用 `WindowManager` 作为浮动悬浮窗显示。

- 独立绘制在 Unity 渲染之上,不影响游戏循环。
- 通过拖动工具栏可自由移动窗口位置。
- 以屏幕 95% 宽度、50% 高度显示在顶部。
- **需要 `SYSTEM_ALERT_WINDOW` 权限。**(参见[权限部分](#android-权限))

### iOS

作为独立的 `UIWindow` 以 `UIWindowLevelAlert + 100` 层级显示。

- 与 Unity 的 key window 独立运行,不影响 Unity 渲染。
- 通过拖动手势可移动窗口位置。
- 以屏幕 95% 宽度、45% 高度显示。
- 基于 UITableView 显示日志,因此大量日志也能流畅滚动。
- 无需额外权限。

### Editor

由于 Unity 编辑器已有 Console 窗口,因此 `Show()`/`Hide()` 仅切换内部标志。
若要在 Game 视图中绘制覆盖层,请参阅[编辑器回退](#编辑器回退)部分。

## API

```csharp
namespace AchEngine
{
    public static class AchDebugConsole
    {
        // 콘솔이 현재 표시 중인지 여부
        public static bool IsVisible { get; }

        // 콘솔 표시
        public static void Show();

        // 콘솔 숨기기
        public static void Hide();

        // 표시/숨김 토글
        public static void Toggle();

        // 로그 항목 전체 삭제
        public static void Clear();
    }
}
```

## 快速开始

通过四指点击打开和关闭控制台的示例。

```csharp
using UnityEngine;
using AchEngine;

public class DebugConsoleTrigger : MonoBehaviour
{
    private void Update()
    {
        // 4손가락 탭 감지
        if (Input.touchCount == 4)
        {
            bool allBegan = true;
            for (int i = 0; i < 4; i++)
            {
                if (Input.GetTouch(i).phase != TouchPhase.Began)
                {
                    allBegan = false;
                    break;
                }
            }

            if (allBegan)
                AchDebugConsole.Toggle();
        }
    }
}
```

在场景中创建一个空的 GameObject 并附加该组件即可。
若需要键盘快捷键,请将其替换为 `Input.GetKeyDown(KeyCode.BackQuote)` 等。

## 编辑器回退

要在编辑器的 Game 视图中显示覆盖层,需要一个在 `OnGUI()` 中调用 `DrawEditorGUI()` 的 MonoBehaviour。

```csharp
using UnityEngine;
using AchEngine;

public class AchDebugConsoleEditorOverlay : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnGUI()
    {
        AchDebugConsole.DrawEditorGUI();
    }
#endif
}
```

将该组件附加到场景中的任意 GameObject 上后,即可在编辑器播放过程中通过 Game 视图覆盖层查看控制台。

:::warning Android 权限
要在 Android 上显示悬浮窗,需要 `SYSTEM_ALERT_WINDOW` 权限。
由于包中已包含 `AchDebugConsoleManifest.xml`,无需手动编辑 `AndroidManifest.xml`,权限会自动声明。

但在 Android 6.0(API 23)及以上版本中,仍需向用户**运行时请求权限**。
请在调用 `Show()` 之前按以下方式检查并请求权限。

```csharp
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;

if (!Permission.HasUserAuthorizedPermission("android.permission.SYSTEM_ALERT_WINDOW"))
{
    Permission.RequestUserPermission("android.permission.SYSTEM_ALERT_WINDOW");
    // 권한 승인 후 Show() 호출
}
else
{
    AchDebugConsole.Show();
}
#endif
```
:::

:::info 性能
在 Android 和 iOS 上均完全在原生 UI 线程上进行渲染。
不会给 Unity 主线程或渲染线程带来额外负担,
因此在性能分析过程中保持控制台开启也是安全的。
:::

## 相关文档

- [UI 系统概述](/guide/ui/)
