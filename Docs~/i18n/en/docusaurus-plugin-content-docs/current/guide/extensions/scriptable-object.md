# AchScriptableObject

`AchScriptableObject` is a base class for `ScriptableObject` that unifies runtime loading and editor auto-creation into a single API.
Assets are stored at `Assets/AchEngine/Resources/Settings/`.

## Usage

Create a settings class that inherits from `AchScriptableObject`.

```csharp
using AchEngine;

[CreateAssetMenu(menuName = "AchEngine/GameConfig")]
public class GameConfig : AchScriptableObject
{
    public int MaxLevel = 100;
    public float GravityScale = 1.0f;
}
```

## GetOrAdd

Loads the asset if it exists; creates it automatically in the editor if it does not.

```csharp
var config = AchScriptableObject.GetOrAdd<GameConfig>();
Debug.Log(config.MaxLevel);
```

> Callable at runtime. Returns `null` at runtime if the asset does not exist (creation is editor-only).

## Add (Editor Only)

Force-creates a new asset.

```csharp
#if UNITY_EDITOR
var config = AchScriptableObject.Add<GameConfig>();
#endif
```

## PingAsset (Editor Only)

Highlights (pings) the asset in the Project window and sets it as the active selection.

```csharp
#if UNITY_EDITOR
AchScriptableObject.PingAsset<GameConfig>();
#endif
```

## Asset Paths

| Path | Description |
|---|---|
| `Assets/AchEngine/Resources/Settings/` | Where assets are saved |
| `Settings/` | Path used by `Resources.Load<T>()` at runtime |
