# Localization - Overview

AchEngine Localization is a **JSON / CSV based** multilingual system.
It supports locale switching, fallback locales, automatic system language detection, and type-safe key constant generation.

## Core Components

| Class | Role |
|---|---|
| `LocalizationManager` | Facade for locale switching and text lookup |
| `LocalizationSettings` | Settings `ScriptableObject` placed in `Resources` |
| `LocaleDatabase` | Maps locale lists to JSON files |
| `LocalizedString` | Runtime wrapper for multilingual text |
| `L` (generated class) | Type-safe key constants produced by code generation |

## Basic Usage

```csharp
using AchEngine.Localization;

// Lookup text in the current locale
string text = LocalizationManager.Get("menu.start");

// Type-safe key (after code generation)
string text2 = LocalizationManager.Get(L.Menu.Start);

// Change locale
LocalizationManager.SetLocale("ja");

// Subscribe to locale changes
LocalizationManager.OnLocaleChanged += OnLocaleChanged;
```

## JSON Format

```json
// ko.json - Korean
{
  "menu.start": "게임 시작",
  "menu.settings": "설정",
  "dialog.confirm": "확인",
  "item.sword.name": "철 검",
  "item.sword.desc": "평범한 철 검입니다."
}
```

Write JSON keys as a flat list using **dot notation**, without nested objects.

## Automatic TMP Component Refresh

When you add `LocalizedText` to a TextMeshPro object,
its text updates automatically whenever the locale changes.

```
[TextMeshProUGUI]
  └── LocalizedText  ← Key: "menu.start"
```

:::info TMP Support
The `LocalizedText` component is enabled when TextMeshPro (`com.unity.textmeshpro`) is installed.
:::

## Next Steps

- [Setup & Database](/en/guide/localization/setup)
- [Key Constant Code Generation](/en/guide/localization/codegen)

