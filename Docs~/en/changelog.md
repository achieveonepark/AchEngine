# Changelog

## 1.1.0

**Breaking changes**
- Removed `SoundManager`. Replace all usages with the new `AudioManager`.

**New features**
- Added `AudioManager` — replaces `SoundManager` with BGM crossfade (`PlayBgm(clip, fadeDuration)`, `StopBgm(fadeDuration)`), BGM volume fade (`SetBgmVolume(volume, fadeDuration)`), per-channel mute (`MuteBgm`, `MuteSfx`, `MuteAll`), an 8-slot concurrent SFX channel pool, and 3D spatial audio (`PlaySfxAt(clip, worldPosition)`).
- Added `AchTimer` — async/await timer utility. `AchTimer.Wait(seconds)` and `AchTimer.WaitRealtime(seconds)` for simple waits; `AchTimer.Start(duration)` returns an `AchTimerHandle` with `Elapsed`, `Remaining`, `Progress` (0–1), `IsDone`, `Cancel()`, and direct `await` support. Supports `CancellationToken` and `useUnscaledTime`. The `AchTimerRunner` is auto-created at startup — no scene setup required.
- Added `UIAchTimer` component — connect an `AchTimerHandle` to a `Text` and/or `Slider` via `Bind(handle)` / `Unbind()` for real-time progress display.
- Added `AchButtonCooldown` component — disables a `Button` after each click for a configurable cooldown period, with an optional countdown `Text` label and `OnCooldownStart` / `OnCooldownEnd` Unity events. Exposes `StartCooldown()`, `ResetCooldown()`, and `IsCoolingDown`.
- Added `AchButtonHold` component — fires a repeated `UnityEvent` while a button is held, with configurable `InitialDelay` and `RepeatInterval`.
- Added `AchDebugConsole` — a native-UI debug overlay that captures `Application.logMessageReceivedThreaded` with no impact on Unity's render thread. On Android a draggable `WindowManager` overlay (requires `SYSTEM_ALERT_WINDOW`); on iOS a `UIWindow` at `UIWindowLevelAlert + 100`; in the Editor an IMGUI fallback via `DrawEditorGUI()`. API: `Show()`, `Hide()`, `Toggle()`, `Clear()`, `IsVisible`.
- Added `RedDot.ClearAll()` — resets every node's count to zero in one call.
- Added click-to-clear support to `RedDotBadge` — new `Clear On Click` (default `true`) and `Button` fields automatically invoke `RedDot.Clear(key)` when the assigned button is pressed.

**Documentation**
- Fixed 15 API discrepancies across all guides: `ServiceLocator.Get<T>()` → `Resolve<T>()`, corrected `UIView` lifecycle hook signatures (`object payload`), `CloseSelf()`, removed non-existent `Show<T>()` / `Close<T>()` / `CloseLayer()` overloads, fixed `AchEngineScope` ↔ `ServiceLocator` lifecycle diagram, corrected `IServiceBuilder` registration syntax, clarified `ISaveService.Configure()` ownership, removed `Rigidbody2D.MovePosition()` from pathfinding docs, corrected `Selectable<T>.mChanged` event name, and noted `Build()` supports GET and POST only.
- Added full Korean and English documentation for all new features: `AudioManager`, `AchTimer` + `UIAchTimer`, `AchButtonCooldown` + `AchButtonHold`, and `AchDebugConsole`.
- Updated `RedDot` docs with `ClearAll()` and click-to-clear badge usage.

## 1.0.3

- Added `SaveManager`, `ISaveService`, and `LocalSaveService` — a save abstraction layer that separates persistence logic from `PlayerManager`. Provides both synchronous and async APIs, and is designed to swap in cloud backends (Firestore, AWS, etc.) without touching game code.
- Removed save/load logic from `PlayerManager`; it now manages only typed data containers (`Add`, `Get`, `Remove`).
- Added `AchProjectile` — a unified straight/homing projectile component. No Rigidbody2D required.
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

- Added Table JSON to CSV export tools for Google Sheets import, with support for single files and folders.
