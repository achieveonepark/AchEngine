# AchEngine Quick Start

## Sample Structure

```text
QuickStart/
|- Data/
|  |- MonsterData.cs
|- Views/
|  |- MainMenuView.cs
|  |- SettingsView.cs
|  |- GameHUDView.cs
|  |- LoadingView.cs
|  |- MonsterDetailPopup.cs
|- Bootstrap/
   |- GameBootstrapWithDI.cs
   |- GameBootstrapNoDI.cs
   |- TableDataLoader.cs
```

## With VContainer

1. Add `AchEngineScope` to your scene.
2. Assign a `UIViewCatalog`.
3. Add `SampleGameScope` to the scene.

```csharp
public class GameBootstrap : IStartable
{
    [Inject] readonly ITableService tableService;
    [Inject] readonly IUIService uiService;

    public void Start()
    {
        TableLoaderGenerated.LoadAll(tableService);
        uiService.Show("MainMenu");
    }
}
```

## Without VContainer

1. Add `UIBootstrapper` and `UIRoot` to your scene.
2. Assign a `UIViewCatalog`.
3. Add `GameBootstrapNoDI` to the scene.

```csharp
TableLoaderGenerated.LoadAll();
var monster = TableManager.Get<MonsterData>(1);

UI.Show("MonsterDetail", 1);
UI.Close("MainMenu");
UI.CloseTopmost();
```

## Suggested Catalog Entries

| View ID | Layer | Pooled | Single Instance |
|---|---|---|---|
| MainMenu | Screen | Yes | Yes |
| Settings | Popup | Yes | Yes |
| GameHUD | Overlay | Yes | Yes |
| Loading | Overlay | Yes | No |
| MonsterDetail | Popup | Yes | No |
