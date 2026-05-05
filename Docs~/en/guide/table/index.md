# Table Loader - Overview

Table Loader automates the **Google Sheets → CSV → C# data class → MemoryPack binary file** pipeline.
At runtime, you can access data in a type-safe way through `TableManager`.

:::info Optional Package
If MemoryPack (`com.cysharp.memorypack`) is not installed, the system falls back to JSON serialization.
:::

## Pipeline Flow

```mermaid
flowchart LR
    GS([("☁ Google\nSheets")])
    CSV[["📄 CSV Files"]]
    CS[["⚙ C# Classes\n(Auto Generated)"]]
    BIN[["📦 .bytes / .json\n(Baked Output)"]]
    TM(["🎮 TableManager\n.Get&lt;T&gt;()"])

    GS -- "Download CSV\n(Editor)" --> CSV
    CSV -- "Generate Code\n(Editor)" --> CS
    CS -- "Bake\n(Editor)" --> BIN
    BIN -- "Resources.Load\n(Runtime)" --> TM

    style GS   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
    style CSV  fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
    style CS   fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
    style BIN  fill:#1e3a5f,stroke:#f59e0b,color:#fcd34d
    style TM   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
```

## Main Settings

| Item | Description |
|---|---|
| **Spreadsheet ID** | The ID after `/d/` in the Google Sheets URL |
| **CSV Output Path** | Where downloaded CSV files are stored |
| **Generated Code Path** | Where generated C# classes are stored |
| **Binary Output Path** | Where baked `.bytes` files are stored |
| **Automation Options** | Auto-generate code after download, auto-bake after code generation |

## Next Steps

- [Setup & Download](/en/guide/table/setup)
- [Code Generation & Runtime](/en/guide/table/codegen)

