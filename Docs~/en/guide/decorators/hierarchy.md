# Hierarchy decorator

`AchEngineHierarchyDecorator` paints helper UI on top of every Unity Hierarchy row. Every feature can be toggled under `Preferences > AchEngine > Decorators > Hierarchy`.

## Features

### Row stripes
A very subtle alternating background helps the eye follow long sibling lists at the same depth.

### Section headers
GameObjects whose name starts with one of these prefixes become a full‑width section header row:

```
--- Lighting
### UI Layer
>>> Spawners
```

- Prefixes and whitespace are trimmed and the result is uppercased.
- Section header rows skip component icons, badges, and toggles to keep them visually clean.

### Component icons
Up to five mini‑thumbnails of the GameObject's components are drawn on the right side of the row.

- `Transform` is always skipped.
- Left‑clicking an icon pings the matching component.

### Active toggle
A small checkbox at the very right of the row binds to `GameObject.activeSelf`. Toggling it goes through `Undo`, so `Ctrl+Z` works as expected.

### Tag · Layer · Static badges
- **Tag** — green pill, only when not `Untagged`
- **Layer** — blue pill, only when not the default `0` layer
- **Static** — amber `S` badge when `isStatic == true`

## Toggles

| Key | Default |
|---|---|
| Enabled | ✅ |
| Row stripes | ✅ |
| Section header rows  (`---`, `###`, `>>>`) | ✅ |
| Component icons | ✅ |
| Active toggle | ✅ |
| Tag / Layer / Static badges | ✅ |

## Source

`Editor/Decorators/AchEngineHierarchyDecorator.cs`

The decorator draws via the `EditorApplication.hierarchyWindowItemOnGUI` callback (one IMGUI pass per row), and reuses a single `List<Component>` buffer to avoid GC during the per‑row component query.
