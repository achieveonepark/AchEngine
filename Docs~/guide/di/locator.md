# ServiceLocator

:::warning VContainer 미설치 전용
`ACHENGINE_VCONTAINER` 심볼이 정의된 환경(VContainer 설치 시)에서는 `ServiceLocator`가 컴파일되지 않습니다.
VContainer 프로젝트에서는 `[Inject]`를 사용하세요.
:::

`ServiceLocator`는 VContainer 없이 런타임에 서비스를 타입으로 조회하는 정적 파사드입니다.

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
| VContainer | ✅ 필요 | ❌ VContainer 미설치 시만 사용 가능 |
| 사용 위치 | DI 컨테이너가 생성한 객체 | 어디서든 (VContainer 없는 환경) |
| 권장 상황 | 모든 서비스·View | VContainer 없는 MonoBehaviour |
| 테스트 용이성 | 높음 | 중간 |

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

## 관련 문서

- [DI 시스템 개요](/guide/di/index)
- [DI 라이프사이클 설계](/guide/di/lifecycle)
