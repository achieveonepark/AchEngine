# AchScriptableObject

`AchScriptableObject` は `ScriptableObject` の基底クラスで、ランタイムロードとエディタでの自動生成を 1 つの API で処理します。
アセットは `Assets/AchEngine/Resources/Settings/` パスに保存されます。

## 使い方

`AchScriptableObject` を継承した設定クラスを作成します。

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

アセットが存在すればロードし、存在しなければエディタで自動生成します。

```csharp
var config = AchScriptableObject.GetOrAdd<GameConfig>();
Debug.Log(config.MaxLevel);
```

> ランタイムからも呼び出せます。アセットが存在しない場合は `null` を返します（生成はエディタ専用）。

## Add（エディタ専用）

強制的に新しいアセットを生成します。

```csharp
#if UNITY_EDITOR
var config = AchScriptableObject.Add<GameConfig>();
#endif
```

## PingAsset（エディタ専用）

プロジェクトウィンドウでアセットをハイライト（Ping）し、Selection に設定します。

```csharp
#if UNITY_EDITOR
AchScriptableObject.PingAsset<GameConfig>();
#endif
```

## アセットパス

| パス | 説明 |
|---|---|
| `Assets/AchEngine/Resources/Settings/` | アセット保存パス |
| `Settings/` | `Resources.Load<T>()` のパス（ランタイムロード基準） |
