---
name: singleton
description: Use when the user asks to create a singleton, a globally-accessible manager/class, or a "there should only be one of this" object in a project that has the AchEngine package installed. Use AchEngine's Singleton/MonoSingleton/PersistentMonoSingleton base classes instead of writing a new static Instance pattern from scratch.
---

# 프로젝트 내부 Singleton 사용

`AchEngine` 루트 네임스페이스 (`Runtime/Singleton/*`). 새 static Instance 패턴을 직접 작성하지 말고 아래 베이스 클래스 중 하나를 상속한다. 모두 `ISingleton`(`InitializeSingleton()`, `ClearSingleton()`)을 구현한다.

## 어떤 걸 골라야 하는가

- **`Singleton<T>` where `T : Singleton<T>, new()`** — MonoBehaviour가 아닌 순수 C# 클래스용. 더블 체크 락킹 기반 thread-safe. `Instance`가 lazy 생성. `CreateInstance()`/`DestroyInstance()`로 수동 제어 가능. `OnInitialized()` 오버라이드로 초기화 로직 작성.
- **`MonoSingleton<T>` where `T : MonoSingleton<T>`** — `MonoBehaviour` 싱글톤. `Instance`가 없으면 `FindObjectOfType<T>()` 하거나 새 GameObject를 만들어 자동 생성. 중복 인스턴스는 `Awake()`에서 자동 파괴. `OnMonoSingletonCreated()`(자동 생성됐을 때만), `OnInitialized()` 오버라이드 가능.
- **`PersistentMonoSingleton<T>` where `T : MonoSingleton<T>`** — `MonoSingleton<T>`에 `DontDestroyOnLoad(gameObject)`가 추가된 버전. 씬 전환 후에도 유지되어야 하는 매니저는 이걸 쓴다.

## 예시

```csharp
public class MyManager : PersistentMonoSingleton<MyManager>
{
    protected override void OnInitialized()
    {
        base.OnInitialized();
        // 초기화 로직
    }
}

// 사용처
MyManager.Instance.DoThing();
```

## 주의 — 다른 시스템과의 관계

이 패턴은 AchEngine의 `IManager` + DI(`AchManagerInstaller`/`ServiceLocator`, `imanager-managers`/`vcontainer-di` 스킬 참고) 매니저 세트와는 별개다. `AudioManager`, `PlayerManager` 등 프레임워크 자체 매니저는 DI/`ServiceLocator`로 접근하고, 사용자가 즉석에서 만드는 유틸리티성 전역 객체에는 이 `Singleton`/`MonoSingleton` 계열을 사용한다. 두 방식을 섞어 같은 클래스에 이중으로 적용하지 않는다.
