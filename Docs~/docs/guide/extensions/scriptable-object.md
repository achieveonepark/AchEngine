# AchScriptableObject

`AchScriptableObject`는 `ScriptableObject`의 기본 클래스로, 런타임 로드와 에디터 자동 생성을 하나의 API로 처리합니다.
에셋은 `Assets/AchEngine/Resources/Settings/` 경로에 저장됩니다.

## 사용법

`AchScriptableObject`를 상속한 설정 클래스를 만드세요.

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

에셋이 있으면 로드하고, 없으면 에디터에서 자동 생성합니다.

```csharp
var config = AchScriptableObject.GetOrAdd<GameConfig>();
Debug.Log(config.MaxLevel);
```

> 런타임에서도 호출 가능합니다. 에셋이 없으면 `null`을 반환합니다 (에디터 전용 생성).

## Add (에디터 전용)

강제로 새 에셋을 생성합니다.

```csharp
#if UNITY_EDITOR
var config = AchScriptableObject.Add<GameConfig>();
#endif
```

## PingAsset (에디터 전용)

프로젝트 창에서 에셋을 강조(Ping)하고 Selection에 설정합니다.

```csharp
#if UNITY_EDITOR
AchScriptableObject.PingAsset<GameConfig>();
#endif
```

## 에셋 경로

| 경로 | 설명 |
|---|---|
| `Assets/AchEngine/Resources/Settings/` | 에셋 저장 경로 |
| `Settings/` | `Resources.Load<T>()` 경로 (런타임 로드 기준) |
