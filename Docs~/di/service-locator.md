# ServiceLocator

`ServiceLocator`는 런타임에 등록된 서비스를 타입으로 조회하는 정적 파사드입니다.
DI 컨테이너(`AchEngineScope`)가 초기화되면 자동으로 연결됩니다.

## API

```csharp
namespace AchEngine.DI
{
    public static class ServiceLocator
    {
        // 컨테이너가 준비되었는지 여부
        public static bool IsReady { get; }

        // 서비스 조회 (없으면 InvalidOperationException)
        public static T Resolve<T>();

        // 안전한 서비스 조회 (없으면 false 반환)
        public static bool TryResolve<T>(out T result);
    }
}
```

## 사용 예시

```csharp
// 기본 조회
var ui = ServiceLocator.Resolve<IUIService>();
ui.Show<MainMenuView>();

// 안전한 조회
if (ServiceLocator.TryResolve<IAudioService>(out var audio))
{
    audio.PlayBGM("main_theme");
}

// 준비 여부 확인
if (!ServiceLocator.IsReady)
{
    Debug.LogWarning("서비스 컨테이너가 아직 초기화되지 않았습니다.");
    return;
}
```

## `[Inject]` vs `ServiceLocator`

| | `[Inject]` | `ServiceLocator` |
|---|---|---|
| VContainer 필요 | ✅ | ❌ |
| 사용 위치 | DI 컨테이너가 생성한 객체 | 어디서든 |
| 권장 상황 | 일반 서비스·View | MonoBehaviour, 씬 전환 중 |
| 테스트 용이성 | 높음 | 중간 |

:::tip 권장 패턴
가능하면 `[Inject]`를 사용하고, MonoBehaviour처럼 DI 컨테이너가 직접 생성하지 않는
객체에서만 `ServiceLocator`를 사용하세요.
:::

## 수동 초기화 (VContainer 없는 경우)

VContainer 없이 `ServiceLocator`를 사용하려면 직접 리졸버를 설정합니다.

```csharp
// 부트스트랩 코드
var container = new Dictionary<Type, object>();
container[typeof(IGameService)] = new GameService();

ServiceLocator.Setup(type =>
{
    container.TryGetValue(type, out var obj);
    return obj;
});
```
