# SaveManager と保存システム

`SaveManager` は保存・読み込み・削除を `ISaveService` インターフェースの背後に抽象化します。
`PlayerManager` から切り離されているため、後からローカルファイル方式を Firestore / AWS / Cloudflare D1 などのクラウドバックエンドへ差し替えても、ゲームコードには一切手を入れる必要がありません。

## 構成

```
SaveManager          ← DI로 주입
  └─ ISaveService    ← 인터페이스 (백엔드 교체 지점)
       └─ LocalSaveService  ← QuickSave 기반 로컬 구현체
```

## クイックスタート

### 1. DI 登録

```csharp
builder.Register<PlayerManager>();
builder.Register<ISaveService, LocalSaveService>();
builder.Register<SaveManager>();
```

### 2. 設定

`Configure()` は `ISaveService` インターフェースのメソッドです。DI から `ISaveService` を注入するか、Resolve して呼び出します。

```csharp
var saveService = ServiceLocator.Resolve<ISaveService>();

// 반드시 Save/Load 전에 한 번 호출
saveService.Configure(encryptionKey: "myKey12345678!", version: 1);

var save = ServiceLocator.Resolve<SaveManager>();
```

- `encryptionKey` — 16 文字以上の文字列で保存ファイルを暗号化します。
- `version` — データ構造が変わった際のマイグレーションに利用します。

## 同期 API

```csharp
save.Save();                      // 현재 상태를 디스크에 저장
PlayerManager loaded = save.Load(); // 저장된 데이터 불러오기
save.Delete();                    // 저장 파일 삭제
```

## 非同期 API

```csharp
await save.SaveAsync();
PlayerManager loaded = await save.LoadAsync();
await save.DeleteAsync();
```

## 保存ファイルの存在確認

```csharp
bool hasData = save.IsExist;
```

## カスタムバックエンドの接続

`ISaveService` を実装すれば、ローカル以外のバックエンドを接続できます。

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

DI 登録時に `LocalSaveService` の代わりに差し替えるだけで済みます。

```csharp
builder.Register<ISaveService, FirestoreSaveService>();
```

## エディタメニュー <Badge type="tip" text="USE_QUICK_SAVE" />

QuickSave パッケージがインストールされている場合、Unity エディタのメニュー **Achieve → Delete Save** から保存ファイル (`.acqs`) を一括削除できます。
