# Editor Decorators

AchEngine adds a lightweight but practical decoration layer on top of Unity's stock editor windows: `Hierarchy`, `Project`, `Scene`, and `Game`.

The goal is not to replace Unity with a completely different editor. The goal is to make the everyday editor feel cleaner, faster to scan, and more helpful during production.

:::warning Requirements
The decorators rely on modern UI Toolkit APIs, so they only activate on **Unity 6000.3 or newer**. On older editors the source is excluded by `#if`, and Unity falls back to its default UI.
:::

## At a glance

| Area | Highlights |
|---|---|
| [Hierarchy](./hierarchy) | Stripes, section headers, component icons, active toggle, Tag / Layer / Static badges |
| [Project](./project) | Stripes, file size badges, folder item counts |
| [Scene & Game](./views) | Scene HUD overlay, Game view FPS / resolution / time scale overlay |

## Preferences

Open `Edit > Preferences > AchEngine > Decorators`.

- Every area has its own master toggle
- Most areas expose sub-toggles for finer control
- Changes repaint affected views immediately
- Values are stored in `EditorPrefs`, so they are per-user and per-machine

## How it works

| Decorator | Hook |
|---|---|
| Hierarchy | `EditorApplication.hierarchyWindowItemOnGUI` |
| Project | `EditorApplication.projectWindowItemOnGUI` |
| SceneView | `[Overlay(typeof(SceneView), ...)]` |
| GameView | UI Toolkit overlay injected into `GameView.rootVisualElement` |

All decorators live under `Editor/Decorators/` and register automatically through `[InitializeOnLoad]`.
