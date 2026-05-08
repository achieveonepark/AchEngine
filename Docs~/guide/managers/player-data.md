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

## PlayerData

기본 플레이어 정보 구조체입니다.

```csharp
var data = new PlayerData { Id = 1, Name = "Hero", Level = 10 };
```

## QuickSave <Badge type="tip" text="USE_QUICK_SAVE" />

QuickSave 패키지를 설치하면 `USE_QUICK_SAVE` 심볼이 **자동으로 정의**되어 `PlayerManager`에 저장/불러오기 기능이 활성화됩니다.
내부적으로 [Achieve.QuickSave](https://github.com/achieveonepark/QuickSave) 라이브러리를 사용하며, 암호화와 버전 관리를 지원합니다.

### 설정

```csharp
var pm = ServiceLocator.Get<PlayerManager>();

// 반드시 Save/Load 전에 한 번 호출 (게임 부트스트랩에서)
pm.Configure(encryptionKey: "myKey12345678!", version: 1);
```

- `encryptionKey` — 16자 이상의 문자열로 저장 파일을 암호화합니다.
- `version` — 데이터 구조 변경 시 마이그레이션에 사용합니다.

### 저장 & 불러오기

```csharp
pm.Save();                       // 현재 상태를 디스크에 저장
var loaded = pm.Load();          // 저장된 데이터 불러오기
```

> `Configure()` 없이 `Save()`/`Load()`를 호출하면 `InvalidOperationException`이 발생합니다.
