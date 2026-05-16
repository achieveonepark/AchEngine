# Changelog

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
