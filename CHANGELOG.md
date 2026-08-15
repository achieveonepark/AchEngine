# Changelog

## 1.3.0

**New features**
- Added a Unity 6000.3+ `AchEngine/Scene Navigator` main-toolbar control that lists Build Settings scenes, tracks the active edit scene, and opens the selected scene after checking unsaved changes.
- Automatically added `UISafeAreaFitter` to the `Screen`, `Popup`, and `Tooltip` UI layers. `Background` and `Overlay` remain full-screen layers.

**Improvements**
- Made Addressables asset and location caching type-aware so a cached handle is never reused for an incompatible requested asset type.
- Released temporary Addressables operation handles after initialization, download-size checks, dependency downloads, catalog checks, and catalog updates.
- Prevented duplicate debug-console log callbacks when entering Play Mode with domain reload disabled.
- Updated runtime object discovery to use `FindFirstObjectByType` and avoided duplicate `DontDestroyOnLoad` calls for child UI roots.
- Treated `IScene` as an optional scene lifecycle hook without warning for scenes that do not implement it.

**Documentation**
- Added the 1.2.0 and 1.3.0 release notes and updated the Addressables, UI safe-area, Scene Navigator, and AI Assistant skill documentation.

## 1.2.0

**New features**
- Reworked Addressables around a static `AddressableManager` with `AchTask`-based asset, scene, instantiation, release, remote download, and catalog APIs.
- Added location metadata and asset handle caching, explicit scene handle tracking, Addressables editor settings, watched-folder tooling, and updated Addressables samples.

**Documentation**
- Updated the Addressables guides, integration guide, and samples for the static manager and explicit cache-release flow.

## 1.1.2

**Removed**
- Removed the iOS native debug console (`AchDebugConsolePlugin.mm` and the `AchDebugConsole` UNITY_IOS bindings). On iOS device builds, `AchDebugConsole.Show/Hide/Clear` now only toggle internal state with no visual overlay; the Android overlay and editor IMGUI overlay are unaffected.

## 1.1.1

**New features**
- Replaced `HttpIAPReceiptValidator` with the `IIAPReceiptValidator` integration point (and the `IAPReceiptValidatorBehaviour` sample base class) so each game can own its backend transport, receipt validation, player authentication, and idempotent reward persistence before confirming an order.
- Added Apple StoreKit 2 JWS and app-account-token data to `IAPPurchase` and the receipt-validation request contract.
- Added `RestoreTransactionsAsync()` for Apple-compliant manual restoration and `RetryPendingFulfillmentsAsync()` for retrying unfulfilled orders in the same app session.
- Prevented a pending order from completing a different purchase request for the same product by associating purchase results with their transaction IDs.
- Added an `AchEngine.IAP.Tests` assembly for `IAPManager` unit tests.

**Fixes**
- Fixed an Xcode build failure in `AchDebugConsolePlugin.mm` caused by an invalid property call and a language-linkage mismatch on `_AchConsole_Hide`.

**Documentation**
- Updated the Full Sample purchase popup and IAP setup guide with the game-owned receipt-validator integration point, Apple restoration, and retry flow.

## 1.1.0

**New features**
- Added Unity IAP 5.4.1 support through `IAPManager`: store connection, product fetching, purchase requests, deferred purchase handling, purchase restoration, and explicit order confirmation after fulfillment.
- Added `HttpIAPReceiptValidator`, a `HttpLink`-based receipt-validation client. It posts the transaction ID, Unity IAP receipt, and product list to a game backend, and confirms an order only after the backend reports both receipt validity and idempotent reward persistence.
- Added `IAPManager` to `AchManagerInstaller` and declared the `com.unity.purchasing` 5.4.1 dependency.

**Documentation**
- Updated the Full Sample purchase popup and IAP setup guide with the server receipt-validation flow.

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
