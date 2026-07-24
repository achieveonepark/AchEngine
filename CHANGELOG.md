# Changelog

## 1.0.8

**New features**
- Added `AIAssistantSkills/` — Unity AI Assistant (MCP) skill definitions shipped with the package so Assistant can discover and use AchEngine's built-in systems (`AchMover` movement, `AchTask`, ECS helpers, Localization, A* pathfinding, `Singleton`/`MonoSingleton`, `HttpLink`, the UI view catalog, `AchTimer`, `IManager` managers, VContainer DI, Addressables) instead of reimplementing them or defaulting to Unity's raw APIs.

## 1.0.7

- Fixed Addressables editor compilation when `com.unity.addressables` is not installed.
- Added missing `AGENTS.md.meta`.

## 1.0.6

**Breaking changes**
- Removed the `AchEngine.Extensions` assembly and the `Runtime/Extensions` extension method source files. The related extension method guide pages were also removed from the documentation.

## 1.0.5

**New features**
- Added `AchTask` / `AchTask<T>` — an async wrapper that unifies UniTask and `System.Threading.Tasks.Task` under a single API. The `ENABLE_UNITASK` symbol is auto-defined via `versionDefines` when `com.cysharp.unitask` is installed; falls back to `Task` otherwise. Provides `Delay`, `DelayRealtime`, `WaitUntil`, `WhenAll`, `WhenAny`, `CompletedTask`, and implicit conversions to/from the underlying type.

**Documentation**
- Added Korean and English docs for `AchTask` (`guide/async`).

## 1.0.4

**Breaking changes**
- Removed `SoundManager`. Replace all usages with the new `AudioManager`.

**New features**
- Added `AudioManager` — replaces `SoundManager` with BGM crossfade (`PlayBgm(clip, fadeDuration)`, `StopBgm(fadeDuration)`), BGM volume fade (`SetBgmVolume(volume, fadeDuration)`), per-channel mute (`MuteBgm`, `MuteSfx`, `MuteAll`), an 8-slot concurrent SFX channel pool, and 3D spatial audio (`PlaySfxAt(clip, worldPosition)`).
- Added `AchTimer` — async/await timer utility. `AchTimer.Wait(seconds)` and `AchTimer.WaitRealtime(seconds)` for fire-and-forget waits; `AchTimer.Start(duration)` returns an `AchTimerHandle` that exposes `Elapsed`, `Remaining`, `Progress` (0–1), `IsDone`, `Cancel()`, and is directly `await`-able. Supports `CancellationToken` and `useUnscaledTime`. The internal `AchTimerRunner` is auto-created at startup — no scene setup required.
- Added `UIAchTimer` component — bind an `AchTimerHandle` to a `Text` and/or `Slider` for real-time display with `Bind(handle)` / `Unbind()`.
- Added `AchButtonCooldown` component — disables a `Button` after a click for a configurable cooldown period, with an optional countdown `Text` label and `OnCooldownStart` / `OnCooldownEnd` Unity events. Exposes `StartCooldown()`, `ResetCooldown()`, and `IsCoolingDown`.
- Added `AchButtonHold` component — fires a repeated `UnityEvent` while a button is held, with configurable `InitialDelay` and `RepeatInterval`.
- Added `AchDebugConsole` — a native-UI debug overlay that intercepts `Application.logMessageReceivedThreaded` with no impact on Unity's render thread. On Android it renders a draggable `WindowManager` overlay (requires `SYSTEM_ALERT_WINDOW`); on iOS a `UIWindow` at `UIWindowLevelAlert + 100`; in the Editor an IMGUI fallback via `DrawEditorGUI()`. API: `Show()`, `Hide()`, `Toggle()`, `Clear()`, `IsVisible`.
- Added `RedDot.ClearAll()` — resets every node's count to zero in one call.
- Added click-to-clear support to `RedDotBadge` — new `Clear On Click` (default `true`) and `Button` fields automatically call `RedDot.Clear(key)` when the assigned button is pressed.

**Documentation**
- Fixed 15 API discrepancies across all guides: `ServiceLocator.Get<T>()` → `Resolve<T>()`, corrected `UIView` lifecycle hook signatures (`object payload`), `CloseSelf()`, removed non-existent `Show<T>()` / `Close<T>()` / `CloseLayer()` overloads, fixed `AchEngineScope` ↔ `ServiceLocator` lifecycle diagram, corrected `IServiceBuilder` registration syntax, clarified `ISaveService.Configure()` ownership, removed `Rigidbody2D.MovePosition()` from pathfinding docs, corrected `Selectable<T>.mChanged` event name, and noted `Build()` only supports GET/POST.
- Added full Korean and English documentation for all new features: `AudioManager`, `AchTimer` + `UIAchTimer`, `AchButtonCooldown` + `AchButtonHold`, and `AchDebugConsole`.
- Updated `RedDot` docs with `ClearAll()` and click-to-clear badge usage.

## 1.0.3
- Added `SaveManager`, `ISaveService`, and `LocalSaveService` — a save abstraction layer that decouples persistence logic from `PlayerManager`. Supports both synchronous and async APIs, and is designed for future cloud backend (Firestore, AWS, etc.) swappability.
- Removed save/load logic from `PlayerManager`; it now manages only typed data containers (`Add`, `Get`, `Remove`).
- Added `AchProjectile` — a unified straight/homing projectile component that requires no Rigidbody2D.
- Refactored `AchFollower` to be fully standalone with no dependency on `AchMover`.
- Added multi-language FontAsset baking to FontAsset Maker (Korean / English / Japanese); each language produces a separate `*_TMP.asset` file.
- All runtime async APIs now use `System.Threading.Tasks.Task` directly; removed the intermediate `AchTask` abstraction.

## 1.0.2
- Added optional ECS helpers for Unity Entities, including world, command buffer, baker, system, and DI wrappers.
- Added game framework runtime modules for managers, singleton patterns, logging, web requests, player data, and QuickSave.
- Added a broad runtime extensions assembly covering Unity objects, UI components, collections, strings, delegates, tasks, and common utility helpers.
- Added A* pathfinding utilities with grid baking support.
- Added AchMover movement helpers.
- Added RedDot notification badge runtime support.
- Added UI component helpers for dragging, object touch handling, binding, open buttons, and close buttons.
- Added a full three-scene sample project that demonstrates AchEngine systems together.
- Improved Addressables, DI, Localization, Table, UI, and documentation coverage across Korean and English guides.
- Added play mode reset handling for static state when domain reload is disabled.
- Fixed documentation site issues, Mermaid diagrams, cross-links, and JSON handling.
- Removed Editor Decorators from the package and documentation.
- Simplified the root README into a documentation landing page.

## 1.0.1
- Added Table JSON to CSV export tools for single files and folders.
