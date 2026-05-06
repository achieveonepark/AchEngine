# Localization — 개요

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

## JSON 형식

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

JSON 키는 **dot-notation**으로 중첩 없이 평탄하게 작성합니다.

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

## 다음 단계

- [설정 & 데이터베이스](/guide/localization/setup)
- [키 상수 코드 생성](/guide/localization/codegen)
