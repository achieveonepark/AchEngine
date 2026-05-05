# 설정 & 데이터베이스

## 1. LocalizationSettings 생성

**Project Settings › AchEngine › Localization** 을 열면
설정이 없을 때 `Assets/Resources/LocalizationSettings.asset`을 자동 생성합니다.

## 2. LocaleDatabase 생성

**Database 생성** 버튼을 클릭하고 저장 위치를 선택합니다.

```
Assets/
└── GameData/
    └── LocaleDatabase.asset
```

## 3. JSON 파일 추가

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

## 4. 로케일 설정

**Project Settings › AchEngine › Localization › 로케일 설정** 에서:

| 항목 | 설명 |
|---|---|
| **기본 로케일** | 앱 최초 실행 시 사용할 로케일 |
| **폴백 로케일** | 현재 로케일에 키가 없을 때 사용할 로케일 |
| **시스템 언어 자동 감지** | 기기 언어와 일치하는 로케일로 자동 설정 |
| **앱 시작 시 자동 초기화** | `LocalizationManager.Initialize()`를 Awake 시 자동 호출 |

## 런타임 초기화

자동 초기화가 꺼진 경우 직접 초기화합니다.

```csharp
private async void Start()
{
    await LocalizationManager.InitializeAsync();
    Debug.Log("Localization 준비 완료");
}
```

## 편집기 창

**편집기 열기** 버튼을 클릭하면 `LocalizationEditorWindow`가 열립니다.
모든 로케일의 번역을 테이블 형식으로 편집할 수 있습니다.

| 기능 | 설명 |
|---|---|
| 키 추가/삭제 | 키를 추가하면 모든 로케일에 빈 값이 생성됨 |
| CSV 가져오기 | 번역 작업 결과 CSV를 임포트 |
| JSON 내보내기/가져오기 | 로케일 JSON 파일 직접 가져오기/내보내기 |
