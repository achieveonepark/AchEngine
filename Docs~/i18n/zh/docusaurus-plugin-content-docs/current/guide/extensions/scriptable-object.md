# AchScriptableObject

`AchScriptableObject` 是 `ScriptableObject` 的基类，通过单一 API 处理运行时加载和编辑器自动生成。
资源保存在 `Assets/AchEngine/Resources/Settings/` 路径下。

## 用法

创建继承 `AchScriptableObject` 的配置类。

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

如果资源存在则加载，不存在则在编辑器中自动生成。

```csharp
var config = AchScriptableObject.GetOrAdd<GameConfig>();
Debug.Log(config.MaxLevel);
```

> 也可在运行时调用。资源不存在时返回 `null`（生成功能仅限编辑器）。

## Add（仅编辑器）

强制生成新资源。

```csharp
#if UNITY_EDITOR
var config = AchScriptableObject.Add<GameConfig>();
#endif
```

## PingAsset（仅编辑器）

在项目窗口中高亮（Ping）资源并设置到 Selection。

```csharp
#if UNITY_EDITOR
AchScriptableObject.PingAsset<GameConfig>();
#endif
```

## 资源路径

| 路径 | 说明 |
|---|---|
| `Assets/AchEngine/Resources/Settings/` | 资源保存路径 |
| `Settings/` | `Resources.Load<T>()` 路径（运行时加载基准） |
