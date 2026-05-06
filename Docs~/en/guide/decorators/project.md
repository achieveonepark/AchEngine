# Project decorator

`AchEngineProjectDecorator` paints helper UI on Project window rows in the **single‑column list view**. It deliberately does nothing in the icon grid view.

## Features

### Row stripes
The same alternating background as the Hierarchy decorator, so long folder listings stay readable.

### File size badge
Every non‑folder asset gets a small black pill on the right showing its on‑disk size in `B / KB / MB / GB`.

### Folder item count
Folder rows display a blue pill with the number of items inside.

- `.meta` files are ignored.
- Folder counts are cached for 5 seconds, so the decorator never re‑scans the disk on every repaint.
- The cache is cleared whenever you flip a decorator setting.

## Toggles

| Key | Default |
|---|---|
| Enabled | ✅ |
| Row stripes | ✅ |
| File size badge | ✅ |
| Folder item count | ✅ |

## Source

`Editor/Decorators/AchEngineProjectDecorator.cs`

Hook: `EditorApplication.projectWindowItemOnGUI(string guid, Rect rect)`. The decorator inspects `rect.height` to make sure it only runs in the single‑column list view.
