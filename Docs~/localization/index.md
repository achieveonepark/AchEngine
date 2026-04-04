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

## 기본 사용

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

## JSON 파일 형식

각 로케일마다 하나의 JSON 파일을 사용합니다.
Dot-notation 키를 중첩 구조 없이 평탄하게 작성합니다.

```json
{
  "menu.start": "게임 시작",
  "menu.settings": "설정",
  "menu.quit": "종료",
  "dialog.confirm": "확인",
  "dialog.cancel": "취소",
  "item.sword.name": "철 검",
  "item.sword.desc": "평범한 철 검입니다."
}
```

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

## 설정 위치

**Project Settings › AchEngine › Localization** 에서:
- Locale Database 연결
- 기본 로케일 / 폴백 로케일 설정
- 시스템 언어 자동 감지 토글
- 키 상수 코드 생성
