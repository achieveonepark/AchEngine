# AchEngine

**Unity 통합 개발 툴킷** — VContainer 기반 DI, 레이어 UI 시스템, Google Sheets Table Loader, Addressables 래퍼, Localization을 하나의 패키지로 제공합니다.

📖 **[공식 문서](https://achieveonepark.github.io/AchEngine/)** | [GitHub](https://github.com/achieveonepark/AchEngine)

---

## 포함 모듈

| 모듈 | 설명 | 선택적 의존성 |
|---|---|---|
| **DI 시스템** | VContainer 래핑 · AchEngineInstaller · ServiceLocator | VContainer |
| **UI System** | 레이어 Canvas · UIView 수명 주기 · Pool · 트랜지션 | — |
| **Table Loader** | Google Sheets → CSV → C# 코드 생성 → MemoryPack 베이크 | MemoryPack |
| **Addressables** | 참조 카운팅 캐싱 · 감시 폴더 자동 그룹화 · 원격 콘텐츠 | Unity Addressables |
| **Localization** | JSON 다국어 · 타입-세이프 키 코드 생성 · LocalizedText | TextMeshPro |
| **Editor Decorators** | Hierarchy / Project / Scene / Game 뷰 꾸미기 (Unity 6000.3+) | — |

---

## 설치

### UPM (Git URL)

**Window › Package Manager › + › Add package from git URL...**

```
https://github.com/achieveonepark/AchEngine.git
```

### manifest.json

```json
{
  "dependencies": {
    "com.engine.achieve": "https://github.com/achieveonepark/AchEngine.git"
  }
}
```

### 선택적 패키지

| 기능 | 패키지 |
|---|---|
| DI 컨테이너 | `jp.hadashikick.vcontainer` — [GitHub](https://github.com/hadashiA/VContainer) |
| 이진 직렬화 | `com.cysharp.memorypack` — [GitHub](https://github.com/Cysharp/MemoryPack) |
| 에셋 관리 | `com.unity.addressables` — Package Manager에서 설치 |
| UI 텍스트 | `com.unity.textmeshpro` — Package Manager에서 설치 |

---

## 빠른 시작

### 1. DI 스코프 설정

씬에 빈 GameObject를 생성하고 `AchEngineScope` 컴포넌트를 추가한 뒤,
`AchEngineInstaller`를 상속한 Installer를 작성합니다.

```csharp
using AchEngine.DI;

public class GameInstaller : AchEngineInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder
            .Register<IGameService, GameService>()
            .Register<IUIService, UIService>()
            .RegisterInstance<IConfig>(myConfig);
    }
}
```

### 2. UI View 정의

```csharp
using AchEngine.UI;

public class MainMenuView : UIView
{
    [SerializeField] private Button _playButton;

    protected override void OnInitialize()
    {
        _playButton.onClick.AddListener(() =>
            ServiceLocator.Resolve<ISceneService>().LoadInGame(1));
    }
}
```

### 3. View 표시

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

ui.Show<MainMenuView>();                              // 타입
ui.Show("MainMenu");                                 // 문자열 ID
ui.Show<ItemDetailView>(v => v.SetItem(selectedItem)); // 데이터 전달
ui.CloseAll();
```

### 4. Table 데이터 로드

```csharp
// Google Sheets에서 자동 생성된 타입
var item = TableManager.Get<ItemTable>().Get(101);
Debug.Log($"{item.Name}: {item.Price}G");
```

### 5. Localization

```csharp
using AchEngine.Localization;

string text = LocalizationManager.Get(L.Menu.Start);  // 타입-세이프 키
LocalizationManager.SetLocale("en");
```

### 6. Addressables

```csharp
using AchEngine.Assets;

var handle = await AddressableManager.LoadAsync<Sprite>("icon_sword");
spriteRenderer.sprite = handle.Result;
AddressableManager.Release("icon_sword");
```

---

## 아키텍처 개요

```
Bootstrap 씬 (항상 유지)
 ├── AchEngineScope ─ GlobalInstaller ─ 전역 서비스 등록
 └── UIRoot         ─ 레이어 Canvas 관리

Lobby / InGame 씬 (additive 로드)
 └── AchEngineScope ─ LobbyInstaller / GameInstaller ─ 씬 전용 서비스
```

서비스는 씬 언로드 시 자동으로 컨테이너에서 해제됩니다.

---

## 요구 사항

- Unity **6000.3** 이상
- .NET Standard 2.1

---

## 라이선스

MIT License — 자세한 내용은 [LICENSE](LICENSE) 파일을 참고하세요.
