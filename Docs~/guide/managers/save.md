# SaveManager & 저장 시스템

`SaveManager`는 저장·불러오기·삭제를 `ISaveService` 인터페이스 뒤로 추상화합니다.
`PlayerManager`에서 분리되어 있으므로, 나중에 로컬 파일 방식을 Firestore / AWS / Cloudflare D1 등 클라우드 백엔드로 교체해도 게임 코드를 손댈 필요가 없습니다.

## 구조

```
SaveManager          ← DI로 주입
  └─ ISaveService    ← 인터페이스 (백엔드 교체 지점)
       └─ LocalSaveService  ← QuickSave 기반 로컬 구현체
```

## 빠른 시작

### 1. DI 등록

```csharp
builder.Register<PlayerManager>().AsSelf().AsSingleton();
builder.Register<LocalSaveService>().As<ISaveService>().AsSingleton();
builder.Register<SaveManager>().AsSelf().AsSingleton();
```

### 2. 설정

```csharp
var save = ServiceLocator.Get<SaveManager>();

// 반드시 Save/Load 전에 한 번 호출
save.Configure(encryptionKey: "myKey12345678!", version: 1);
```

- `encryptionKey` — 16자 이상의 문자열로 저장 파일을 암호화합니다.
- `version` — 데이터 구조 변경 시 마이그레이션에 사용합니다.

## 동기 API

```csharp
save.Save();                      // 현재 상태를 디스크에 저장
PlayerManager loaded = save.Load(); // 저장된 데이터 불러오기
save.Delete();                    // 저장 파일 삭제
```

## 비동기 API

```csharp
await save.SaveAsync();
PlayerManager loaded = await save.LoadAsync();
await save.DeleteAsync();
```

## 저장 파일 존재 확인

```csharp
bool hasData = save.IsExist;
```

## 커스텀 백엔드 연결

`ISaveService`를 구현하면 로컬 외의 백엔드를 연결할 수 있습니다.

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

DI 등록 시 `LocalSaveService` 대신 교체하면 됩니다.

```csharp
builder.Register<FirestoreSaveService>().As<ISaveService>().AsSingleton();
```

## Editor 메뉴 <Badge type="tip" text="USE_QUICK_SAVE" />

QuickSave 패키지가 설치된 경우, Unity 에디터 메뉴 **Achieve → Delete Save** 로 저장 파일(`.acqs`)을 한 번에 삭제할 수 있습니다.
