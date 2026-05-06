# Setup & Database

## 1. Create `LocalizationSettings`

Open **Project Settings › AchEngine › Localization**.
If no settings asset exists, AchEngine automatically creates `Assets/Resources/LocalizationSettings.asset`.

## 2. Create `LocaleDatabase`

Click **Create Database** and choose where to save the asset.

```
Assets/
└── GameData/
    └── LocaleDatabase.asset
```

## 3. Add JSON Files

Register JSON files for each locale in `LocaleDatabase`.

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

```json
// en.json - English
{
  "menu.start": "Start Game",
  "menu.settings": "Settings",
  "dialog.confirm": "OK",
  "item.sword.name": "Iron Sword",
  "item.sword.desc": "A plain iron sword."
}
```

Write JSON keys as a flat list using **dot notation**, without nested objects.

## 4. Configure Locales

In **Project Settings › AchEngine › Localization › Locale Settings**:

| Item | Description |
|---|---|
| **Default Locale** | Locale used on the first app launch |
| **Fallback Locale** | Locale used when the current locale is missing a key |
| **Auto Detect System Language** | Automatically choose the locale that matches the device language |
| **Auto Initialize on App Start** | Calls `LocalizationManager.Initialize()` automatically in `Awake` |

## Runtime Initialization

If auto-initialize is disabled, initialize manually.

```csharp
private async void Start()
{
    await LocalizationManager.InitializeAsync();
    Debug.Log("Localization is ready");
}
```

## Editor Window

Click **Open Editor** to open `LocalizationEditorWindow`.
You can edit translations for every locale in a table view.

| Feature | Description |
|---|---|
| Add / Delete Keys | Adding a key creates an empty value in every locale |
| Import CSV | Import translation work from a CSV file |
| Import / Export JSON | Import or export locale JSON files directly |

