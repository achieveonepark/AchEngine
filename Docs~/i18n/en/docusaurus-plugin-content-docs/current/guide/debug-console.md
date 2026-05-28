# AchDebugConsole

`AchDebugConsole` is a debug console that renders on the native UI layer of Android and iOS.
It is completely independent of the Unity main thread, making it safe to use during performance profiling.
In the Editor it falls back to an IMGUI overlay.

## Overview

All Unity log messages (`Log`, `Warning`, `Error`, `Exception`, `Assert`) are captured automatically via `Application.logMessageReceivedThreaded`.
No manual initialization is required — the system registers itself with `[RuntimeInitializeOnLoadMethod]` and begins collecting logs as soon as the game starts.

| Item | Details |
|---|---|
| Max log entries | 500 (ring buffer) |
| Log colors | Error·Exception·Assert → red / Warning → yellow / Log → white |
| Thread safety | All platforms dispatch to the UI thread internally |

## Platform Behavior

### Android

Displayed as a floating overlay window using `WindowManager`.

- Rendered independently above Unity output — no impact on the game loop.
- Drag the toolbar to reposition the window freely.
- Displayed at 95% screen width and 50% screen height, anchored to the top.
- **Requires the `SYSTEM_ALERT_WINDOW` permission.** (See [Permissions](#android-permission))

### iOS

Displayed as a separate `UIWindow` at `UIWindowLevelAlert + 100`.

- Operates independently of Unity's key window and does not affect Unity rendering.
- Drag the handle bar to reposition the window.
- Displayed at 95% screen width and 45% screen height.
- UITableView-based rendering handles large log volumes with smooth scrolling.
- No additional permissions required.

### Editor

The Unity Editor already has a Console window, so `Show()`/`Hide()` only toggle an internal flag.
To display an overlay in the Game View, see the [Editor Fallback](#editor-fallback) section.

## API

```csharp
namespace AchEngine
{
    public static class AchDebugConsole
    {
        // Whether the console is currently visible
        public static bool IsVisible { get; }

        // Show the console
        public static void Show();

        // Hide the console
        public static void Hide();

        // Toggle show/hide
        public static void Toggle();

        // Clear all log entries
        public static void Clear();
    }
}
```

## Quick Start

Example: open and close the console with a four-finger tap.

```csharp
using UnityEngine;
using AchEngine;

public class DebugConsoleTrigger : MonoBehaviour
{
    private void Update()
    {
        // Detect four-finger tap
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

Attach this component to any empty GameObject in the scene.
For a keyboard shortcut, replace the touch check with `Input.GetKeyDown(KeyCode.BackQuote)` or similar.

## Editor Fallback

To display an overlay in the Game View while in the Editor, call `DrawEditorGUI()` from a MonoBehaviour's `OnGUI()`.

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

Attach this component to any GameObject in the scene and the console will appear as a Game View overlay during Editor play.

:::warning Android Permission
Displaying an overlay window on Android requires the `SYSTEM_ALERT_WINDOW` permission.
`AchDebugConsoleManifest.xml` is included in the package and declares this permission automatically — no manual `AndroidManifest.xml` edits needed.

On Android 6.0 (API 23) and above, however, a **runtime permission request** is still required.
Check and request the permission before calling `Show()`:

```csharp
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;

if (!Permission.HasUserAuthorizedPermission("android.permission.SYSTEM_ALERT_WINDOW"))
{
    Permission.RequestUserPermission("android.permission.SYSTEM_ALERT_WINDOW");
    // Call Show() after the user grants permission
}
else
{
    AchDebugConsole.Show();
}
#endif
```
:::

:::info Performance
On both Android and iOS, rendering happens entirely on the native UI thread.
There is no added load on the Unity main thread or render thread, so it is safe to leave the console open during performance profiling.
:::

## Related Docs

- [UI System Overview](/guide/ui/)
