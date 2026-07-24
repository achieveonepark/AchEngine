---
name: localization
description: Use when the user asks to add multi-language support, translate UI text, switch languages at runtime, or localize strings/images in a project that has the AchEngine package installed. Use AchEngine's LocalizationManager instead of Unity's com.unity.localization package or hardcoded strings.
---

# AchEngine 로컬라이제이션

`AchEngine.Localization` 네임스페이스 (`Runtime/Localization/*`, `Editor/Localization/*`).

## 핵심 API

- **`LocalizationManager`** (static) — `Initialize()` / `Initialize(LocalizationSettings)` (`[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`로 자동 실행됨, `LocalizationSettings.Instance.autoInitialize`가 켜져 있고 database가 지정되어 있으면).
  - `Get(string key)`, `Get(string key, params object[] args)`, `Get(string key, Dictionary<string,object> namedArgs)`
  - `TryGet(string key, out string value)`, `HasKey(string key)`, `HasLocale(string code)`
  - `SetLocale(string|Locale)`, `CurrentLocale`, `FallbackLocale`, `AvailableLocales`, `IsInitialized`
  - `event Action<LocaleChangedEventArgs> LocaleChanged`
  - 키가 없으면 에디터/개발 빌드에서는 `[missing:{key}]`, 릴리즈에서는 raw key를 반환한다.
- **`LocalizationSettings`** (`ScriptableObject`, `CreateAssetMenu("Achieve/Localization/Settings")`) — 반드시 `Resources/LocalizationSettings` 경로에 있어야 한다. `defaultLocaleCode`, `fallbackLocaleCode`, `autoDetectSystemLanguage`, `autoInitialize`, `database`(`LocaleDatabase`), 코드젠 옵션(`generatedClassName` 기본 `"L"`, `generatedNamespace`, `generatedOutputPath`).
- **`LocaleDatabase`** (`ScriptableObject`, `CreateAssetMenu("Achieve/Localization/Locale Database")`) — `(Locale, TextAsset json)` 목록, 로케일별 딕셔너리 캐시.
- **`LocalizedString`** — 인스펙터에서 키를 드롭다운으로 고를 수 있는 구조체(`[LocalizedStringAttribute]`). `.Value` / `.GetValue(args)`, 암시적 `string` 변환.
- **컴포넌트**: `LocalizedText`(`Text`/`TMP_Text` 자동 갱신, TMP는 `ACHENGINE_LOCALIZATION_TMP` 심볼 필요, `SetKey`/`SetFormatArgs`/`UpdateText()`), `LocalizedImage`(`[RequireComponent(typeof(Image))]`, 로케일별 스프라이트 + fallback), `LocalizedAudio`.
- **에디터 도구**: `LocalizationEditorWindow` + `LocalizationTableView`(테이블 편집), `CSVImporter`/`JSONImporter`, `LocalizationKeyGenerator`(점 표기 키로 `L.Menu.Start` 같은 중첩 static 클래스 생성).

## 예시

```csharp
string title = LocalizationManager.Get("app.title");
string welcome = LocalizationManager.Get("dialog.welcome", "Player1", 5);
LocalizationManager.LocaleChanged += OnLocaleChanged;
LocalizationManager.SetLocale("ja");
```

UI 텍스트를 다국어로 만들어야 하면 `Text`/`TMP_Text`에 직접 문자열을 넣지 말고 `LocalizedText` 컴포넌트를 붙이거나 `LocalizationManager.Get(key)`를 호출한다.

## 주의

`Resources/LocalizationSettings` 에셋과 `LocaleDatabase`가 없으면 경고 로그만 찍고 `Get()`이 키를 그대로 반환한다. TMP 지원은 `ACHENGINE_LOCALIZATION_TMP` 심볼이 있어야 켜진다.
