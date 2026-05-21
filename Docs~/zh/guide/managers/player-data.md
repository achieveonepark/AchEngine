# PlayerManager 与玩家数据

`PlayerManager` 按类型保存继承自 `PlayerDataContainerBase<TKey, TValue>` 的容器。

## 创建容器

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

## 注册与访问

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

单个数据项的基类,包含 `int Id` 字段。

```csharp
public class ItemData : PlayerDataBase
{
    public string Name;
    public int Quantity;
}
```

## PlayerData

基础玩家信息结构体。

```csharp
var data = new PlayerData { Id = 1, Name = "Hero", Level = 10 };
```

## 保存 / 加载

保存功能已从 `PlayerManager` 中分离,改由 `SaveManager` 专门负责。
详细内容请参阅 [SaveManager](./save) 文档。
