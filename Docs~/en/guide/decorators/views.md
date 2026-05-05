# Scene & Game view overlays

## SceneView HUD

`AchEngineSceneViewOverlay` is registered through Unity's standard `[Overlay(typeof(SceneView), …)]` attribute, so it shows up in the SceneView overlay menu (`☰`) alongside the built‑in overlays as **AchEngine HUD** and can be docked or floated freely.

### What it shows

| Label | Value |
|---|---|
| **Scene** | Active scene name (`SceneManager.GetActiveScene().name`) |
| **Selection** | Selected GameObject name + component count |
| **Position** | World position of the selected GameObject (2 decimals) |
| **Stats** | Active scene root count, current `Time.timeScale` |

- It subscribes to `Selection.selectionChanged` and `EditorApplication.hierarchyChanged` for instant updates and additionally polls every 250 ms so position changes during gizmo drags are reflected.
- When `Preferences > AchEngine > Decorators > Scene HUD overlay` is off, the overlay hides itself.

## GameView HUD

`AchEngineGameViewOverlay` injects a small UI Toolkit panel in the top‑left corner of every GameView's `rootVisualElement`.

### What it shows

| Label | Value |
|---|---|
| **fps** | Smoothed FPS computed from `EditorApplication.update` (Lerp 0.1) |
| **res** | Width × height of the GameView window |
| **ts**  | `Time.timeScale` |

- The panel uses `pickingMode = Ignore` so it never intercepts game input.
- A 0.5 s throttled tick locates GameView instances and keeps them in sync, so newly opened GameViews are picked up automatically.
- Disabling the toggle removes the overlay from every GameView on the next tick.

## Toggles

| Key | Default |
|---|---|
| Scene HUD overlay | ✅ |
| Game HUD overlay | ✅ |

## Source

- `Editor/Decorators/AchEngineSceneViewOverlay.cs`
- `Editor/Decorators/AchEngineGameViewOverlay.cs`
- USS classes: `.ach-scene-overlay`, `.ach-game-overlay` (in `AchEngineInspectorTheme.uss`)
