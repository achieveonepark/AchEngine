# Localization — 概述

AchEngine Localization 是一套**基于 JSON/CSV** 的多语言系统。
支持区域设置切换、回退、系统语言自动检测以及类型安全的键常量代码生成。

## 核心组件

| 类 | 作用 |
|---|---|
| `LocalizationManager` | 区域设置切换、文本查询的外观入口 |
| `LocalizationSettings` | 配置 ScriptableObject (放置于 Resources) |
| `LocaleDatabase` | 区域设置列表及 JSON 文件映射 |
| `LocalizedString` | 运行时多语言文本包装器 |
| `L` (生成类) | 类型安全的键常量 (代码生成结果) |

## 基本用法

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

## JSON 格式

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

JSON 键采用**点记法**，不嵌套，扁平化书写。

## TMP 组件自动刷新

将 `LocalizedText` 组件添加到 TextMeshPro 对象后，
当区域设置变更时会自动刷新文本。

```
[TextMeshProUGUI]
  └── LocalizedText  ← 키: "menu.start"
```

:::info TMP 支持
当安装了 TextMeshPro (`com.unity.textmeshpro`) 时，`LocalizedText` 组件会被启用。
:::

## FontAsset Maker

在 **AchEngine › Localization › FontAsset Maker** 中放入一个字体并点击按钮后，
会收集 LocaleDatabase 中实际的翻译字符串、项目中的 CSV/JSON/TXT TextAsset 以及基本 ASCII，生成静态 TMP FontAsset。
如果在运行时需要组合新的韩文字符串，可以打开 `Include Korean glyph preset`，将整套韩文预设一并烘焙。

生成的 FontAsset 保存在 `Assets/Fonts/Generated`。

## 下一步

- [配置 & 数据库](/zh/guide/localization/setup)
- [键常量代码生成](/zh/guide/localization/codegen)
