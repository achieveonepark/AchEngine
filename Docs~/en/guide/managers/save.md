# SaveManager & Save System

`SaveManager` abstracts save, load, and delete operations behind the `ISaveService` interface.
Because it is decoupled from `PlayerManager`, you can swap the local-file backend for Firestore, AWS, Cloudflare D1, or any other storage provider without touching game code.

## Architecture

```
SaveManager          ← injected via DI
  └─ ISaveService    ← interface (backend swap point)
       └─ LocalSaveService  ← QuickSave-based local implementation
```

## Quick Start

### 1. Register with DI

```csharp
builder.Register<PlayerManager>().AsSelf().AsSingleton();
builder.Register<LocalSaveService>().As<ISaveService>().AsSingleton();
builder.Register<SaveManager>().AsSelf().AsSingleton();
```

### 2. Configure

```csharp
var save = ServiceLocator.Get<SaveManager>();

// Call once before Save/Load (from game bootstrap)
save.Configure(encryptionKey: "myKey12345678!", version: 1);
```

- `encryptionKey` — A string of 16 or more characters used to encrypt the save file.
- `version` — Used for migration when the data structure changes.

## Synchronous API

```csharp
save.Save();                        // Write current state to disk
PlayerManager loaded = save.Load(); // Read saved data from disk
save.Delete();                      // Delete save file
```

## Async API

```csharp
await save.SaveAsync();
PlayerManager loaded = await save.LoadAsync();
await save.DeleteAsync();
```

## Check If Save Exists

```csharp
bool hasData = save.IsExist;
```

## Custom Backend

Implement `ISaveService` to plug in any storage backend.

```csharp
public class FirestoreSaveService : ISaveService
{
    public bool IsExist { get; set; }

    public void Configure(string encryptionKey = "", int version = 0) { /* ... */ }

    public void Save(PlayerManager manager)          { /* write to Firestore */ }
    public PlayerManager Load()                      { /* read from Firestore */ return null; }
    public void Delete()                             { /* delete from Firestore */ }

    public async Task SaveAsync(PlayerManager manager)   { /* ... */ }
    public async Task<PlayerManager> LoadAsync()         { /* ... */ return null; }
    public async Task DeleteAsync()                      { /* ... */ }
}
```

Swap it in at the DI registration site — no other changes required.

```csharp
builder.Register<FirestoreSaveService>().As<ISaveService>().AsSingleton();
```

## Editor Menu <Badge type="tip" text="USE_QUICK_SAVE" />

When the QuickSave package is installed, use **Achieve → Delete Save** in the Unity Editor menu to delete all `.acqs` save files at once.
