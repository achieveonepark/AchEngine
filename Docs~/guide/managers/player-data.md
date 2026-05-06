# PlayerManager & 플레이어 데이터

`PlayerManager`는 `PlayerDataContainerBase<TKey, TValue>`를 상속한 컨테이너들을 타입별로 보관합니다.

## 컨테이너 만들기

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

## 등록 & 접근

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

개별 데이터 항목의 기본 클래스입니다. `int Id` 필드를 포함합니다.

```csharp
public class ItemData : PlayerDataBase
{
    public string Name;
    public int Quantity;
}
```
