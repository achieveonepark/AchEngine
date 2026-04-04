# Localization

AchEngine Localization은 **JSON/CSV 기반** 다국어 시스템입니다.
로케일 전환, 폴백, 시스템 언어 자동 감지, 타입-세이프 키 상수 코드 생성을 지원합니다.

## 핵심 구성 요소

| 클래스 | 역할 |
|---|---|
| `LocalizationManager` | 로케일 전환, 텍스트 조회 파사드 |
| `LocalizationSettings` | 설정 ScriptableObject (Resources 배치) |
| `LocaleDatabase` | 로케일 목록 및 JSON 파일 매핑 |
| `LocalizedString` | 런타임 다국어 텍스트 래퍼 |
| `L` (생성 클래스) | 타입-세이프 키 상수 (코드 생성 결과) |

---

## 초기 설정

### 1. LocalizationSettings 생성

**Project Settings › AchEngine › Localization** 을 열면
설정이 없을 때 `Assets/Resources/LocalizationSettings.asset`을 자동 생성합니다.

### 2. LocaleDatabase 생성

**Database 생성** 버튼을 클릭하고 저장 위치를 선택합니다.

```
Assets/
└── GameData/
    └── LocaleDatabase.asset
```

### 3. JSON 파일 추가

각 로케일의 JSON 파일을 `LocaleDatabase`에 등록합니다.

```json
// ko.json — 한국어
{
  "menu.start": "게임 시작",
  "menu.settings": "설정",
  "dialog.confirm": "확인",
  "item.sword.name": "철 검",
  "item.sword.desc": "평범한 철 검입니다."
}
```

```json
// en.json — English
{
  "menu.start": "Start Game",
  "menu.settings": "Settings",
  "dialog.confirm": "OK",
  "item.sword.name": "Iron Sword",
  "item.sword.desc": "A plain iron sword."
}
```

JSON 키는 **dot-notation**으로 중첩 없이 평탄하게 작성합니다.

### 4. 로케일 설정

**Project Settings › AchEngine › Localization › 로케일 설정** 에서:

| 항목 | 설명 |
|---|---|
| **기본 로케일** | 앱 최초 실행 시 사용할 로케일 |
| **폴백 로케일** | 현재 로케일에 키가 없을 때 사용할 로케일 |
| **시스템 언어 자동 감지** | 기기 언어와 일치하는 로케일로 자동 설정 |
| **앱 시작 시 자동 초기화** | `LocalizationManager.Initialize()`를 Awake 시 자동 호출 |

---

## 런타임 사용

### 초기화

자동 초기화가 꺼진 경우 직접 초기화합니다.

```csharp
private async void Start()
{
    await LocalizationManager.InitializeAsync();
    Debug.Log("Localization 준비 완료");
}
```

### 텍스트 조회

```csharp
using AchEngine.Localization;

// 현재 로케일의 텍스트 조회
string text = LocalizationManager.Get("menu.start");

// 타입-세이프 키 (코드 생성 후)
string text2 = LocalizationManager.Get(L.Menu.Start);

// 로케일 변경
LocalizationManager.SetLocale("ja");

// 로케일 변경 이벤트 구독
LocalizationManager.OnLocaleChanged += OnLocaleChanged;
```

---

## TMP 컴포넌트 자동 갱신

`LocalizedText` 컴포넌트를 TextMeshPro 오브젝트에 추가하면
로케일이 바뀔 때 자동으로 텍스트를 갱신합니다.

```
[TextMeshProUGUI]
  └── LocalizedText  ← 키: "menu.start"
```

:::info TMP 지원
TextMeshPro(`com.unity.textmeshpro`)가 설치된 경우 `LocalizedText` 컴포넌트가 활성화됩니다.
:::

---

## 키 상수 코드 생성

문자열 키를 하드코딩하면 오타가 발생하기 쉽습니다.
AchEngine은 JSON 키를 **타입-세이프 중첩 클래스**로 변환하는 코드 생성기를 제공합니다.

### 변환 예시

JSON 키:
```json
{
  "menu.start": "게임 시작",
  "menu.settings": "설정",
  "dialog.confirm": "확인",
  "item.sword.name": "철 검"
}
```

생성된 C# 클래스:
```csharp
// 자동 생성 — 직접 수정하지 마세요
public static class L
{
    public static class Menu
    {
        public const string Start    = "menu.start";
        public const string Settings = "menu.settings";
    }

    public static class Dialog
    {
        public const string Confirm = "dialog.confirm";
    }

    public static class Item
    {
        public static class Sword
        {
            public const string Name = "item.sword.name";
        }
    }
}
```

### 코드 생성 설정

**Project Settings › AchEngine › Localization › 키 상수 코드 생성** 에서:

| 항목 | 기본값 |
|---|---|
| **클래스 이름** | `L` |
| **네임스페이스** | (비어있으면 전역 네임스페이스) |
| **출력 경로** | `Assets/Generated/` |

**키 상수 생성** 버튼을 클릭하면 `{출력경로}/{클래스명}.cs` 파일이 생성됩니다.

---

## LocalizedString 컴포넌트

Inspector에서 키를 지정할 때는 `LocalizedString` 타입을 사용합니다.

```csharp
public class ItemNameDisplay : MonoBehaviour
{
    [SerializeField] private LocalizedString _nameKey;

    private void Start()
    {
        GetComponent<Text>().text = _nameKey.Value;
    }
}
```

Inspector에서 `_nameKey` 필드에 키를 입력하면 커스텀 PropertyDrawer가
현재 로케일의 번역 미리보기를 보여줍니다.

---

## 편집기 창

**편집기 열기** 버튼을 클릭하면 `LocalizationEditorWindow`가 열립니다.
모든 로케일의 번역을 테이블 형식으로 편집할 수 있습니다.

| 기능 | 설명 |
|---|---|
| 키 추가/삭제 | 키를 추가하면 모든 로케일에 빈 값이 생성됨 |
| CSV 가져오기 | 번역 작업 결과 CSV를 임포트 |
| JSON 내보내기/가져오기 | 로케일 JSON 파일 직접 가져오기/내보내기 |
