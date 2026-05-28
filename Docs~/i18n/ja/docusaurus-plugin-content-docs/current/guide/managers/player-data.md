# PlayerManager とプレイヤーデータ

`PlayerManager` は `PlayerDataContainerBase<TKey, TValue>` を継承したコンテナを型ごとに保持します。

## コンテナの作成

```csharp
using AchEngine.Player;

public class InventoryContainer : PlayerDataContainerBase<int, ItemData>
{
    public InventoryContainer()
    {
        DataKey = nameof(InventoryContainer);
        _dataDic = new Dictionary<int, ItemData>();
    }
}
```

## 登録とアクセス

```csharp
var pm = ServiceLocator.Get<PlayerManager>();

// 등록
pm.AddContainer(new InventoryContainer());

// 접근
var inv = pm.GetContainer<InventoryContainer>();
inv.Add(101, new ItemData { Id = 101, Name = "Sword" });
var sword = inv.GetInfo(101);

// 제거
pm.RemoveContainer<InventoryContainer>();
```

## PlayerDataBase

個別のデータ項目の基底クラスです。`int Id` フィールドを含みます。

```csharp
public class ItemData : PlayerDataBase
{
    public string Name;
    public int Quantity;
}
```

## PlayerData

基本的なプレイヤー情報を表す構造体です。

```csharp
var data = new PlayerData { Id = 1, Name = "Hero", Level = 10 };
```

## 保存 / 読み込み

保存機能は `PlayerManager` から分離され、`SaveManager` が専属で担当します。
詳細は [SaveManager](./save) ドキュメントを参照してください。
