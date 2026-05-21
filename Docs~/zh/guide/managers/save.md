# SaveManager 与保存系统

`SaveManager` 将保存、加载、删除操作抽象在 `ISaveService` 接口之后。
由于已与 `PlayerManager` 解耦,日后将本地文件方式替换为 Firestore / AWS / Cloudflare D1 等云端后端时,无需改动游戏代码。

## 架构

```
SaveManager          ← DI로 주입
  └─ ISaveService    ← 인터페이스 (백엔드 교체 지점)
       └─ LocalSaveService  ← QuickSave 기반 로컬 구현체
```

## 快速开始

### 1. 注册到 DI

```csharp
builder.Register<PlayerManager>();
builder.Register<ISaveService, LocalSaveService>();
builder.Register<SaveManager>();
```

### 2. 配置

`Configure()` 是 `ISaveService` 接口的方法。可通过 DI 注入或 Resolve 获取 `ISaveService` 后调用。

```csharp
var saveService = ServiceLocator.Resolve<ISaveService>();

// 반드시 Save/Load 전에 한 번 호출
saveService.Configure(encryptionKey: "myKey12345678!", version: 1);

var save = ServiceLocator.Resolve<SaveManager>();
```

- `encryptionKey` — 16 个字符以上的字符串,用于加密保存文件。
- `version` — 用于数据结构变更时的迁移。

## 同步 API

```csharp
save.Save();                      // 현재 상태를 디스크에 저장
PlayerManager loaded = save.Load(); // 저장된 데이터 불러오기
save.Delete();                    // 저장 파일 삭제
```

## 异步 API

```csharp
await save.SaveAsync();
PlayerManager loaded = await save.LoadAsync();
await save.DeleteAsync();
```

## 检查保存文件是否存在

```csharp
bool hasData = save.IsExist;
```

## 接入自定义后端

实现 `ISaveService` 即可接入本地以外的任意后端。

```csharp
public class FirestoreSaveService : ISaveService
{
    public bool IsExist { get; set; }

    public void Configure(string encryptionKey = "", int version = 0) { /* ... */ }

    public void Save(PlayerManager manager)          { /* Firestore에 동기 쓰기 */ }
    public PlayerManager Load()                      { /* Firestore에서 동기 읽기 */ return null; }
    public void Delete()                             { /* Firestore에서 삭제 */ }

    public async Task SaveAsync(PlayerManager manager)   { /* ... */ }
    public async Task<PlayerManager> LoadAsync()         { /* ... */ return null; }
    public async Task DeleteAsync()                      { /* ... */ }
}
```

只需在 DI 注册时替换掉 `LocalSaveService` 即可。

```csharp
builder.Register<ISaveService, FirestoreSaveService>();
```

## 编辑器菜单 <Badge type="tip" text="USE_QUICK_SAVE" />

安装 QuickSave 包后,可通过 Unity 编辑器菜单 **Achieve → Delete Save** 一键删除保存文件 (`.acqs`)。
