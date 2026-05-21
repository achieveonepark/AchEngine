# 配置 & 数据库

## 1. 创建 LocalizationSettings

打开 **Project Settings › AchEngine › Localization** 后，
若不存在配置，会自动生成 `Assets/Resources/LocalizationSettings.asset`。

## 2. 创建 LocaleDatabase

点击 **生成 Database** 按钮并选择保存位置。

```
Assets/
└── GameData/
    └── LocaleDatabase.asset
```

## 3. 添加 JSON 文件

将各区域设置的 JSON 文件注册到 `LocaleDatabase` 中。

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

JSON 键采用**点记法**，不嵌套，扁平化书写。

## 4. 区域设置

在 **Project Settings › AchEngine › Localization › 区域设置** 中:

| 项目 | 说明 |
|---|---|
| **默认区域设置** | 应用首次启动时使用的区域设置 |
| **回退区域设置** | 当前区域设置中缺少键时使用的区域设置 |
| **系统语言自动检测** | 自动设置为与设备语言匹配的区域设置 |
| **应用启动时自动初始化** | 在 Awake 时自动调用 `LocalizationManager.Initialize()` |

## 运行时初始化

当自动初始化关闭时，需手动初始化。

```csharp
private async void Start()
{
    await LocalizationManager.InitializeAsync();
    Debug.Log("Localization 준비 완료");
}
```

## 编辑器窗口

点击 **打开编辑器** 按钮会打开 `LocalizationEditorWindow`。
可以以表格形式编辑所有区域设置的翻译。

| 功能 | 说明 |
|---|---|
| 添加/删除键 | 添加键后，会在所有区域设置中生成空值 |
| 导入 CSV | 导入翻译工作结果的 CSV |
| 导出/导入 JSON | 直接导入/导出区域设置 JSON 文件 |
