# Table Loader — 概述

Table Loader 自动化了 **Google Sheets → CSV → C# 数据类 → MemoryPack 二进制文件** 的流水线。
在运行时可通过 `TableManager` 以类型安全的方式访问数据。

:::info 可选包
若没有 MemoryPack (`com.cysharp.memorypack`)，则以 JSON 序列化方式运行。
:::

## 流水线流程

```mermaid
flowchart LR
GS([("☁ Google<br/>Sheets")])
CSV[["📄 CSV 文件"]]
CS[["⚙ C# 类<br/>(自动生成)"]]
BIN[["📦 .bytes / .json<br/>(烘焙结果)"]]
TM(["🎮 TableManager<br/>.Get<T>()"])

GS -- "Download CSV<br/>(编辑器)" --> CSV
CSV -- "Generate Code<br/>(编辑器)" --> CS
CS -- "Bake<br/>(编辑器)" --> BIN
BIN -- "Resources.Load<br/>(运行时)" --> TM

style GS   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
style CSV  fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
style CS   fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
style BIN  fill:#1e3a5f,stroke:#f59e0b,color:#fcd34d
style TM   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
```

## 主要配置项

| 项目 | 说明 |
|---|---|
| **Spreadsheet ID** | Google Sheets URL 中 `/d/` 后面的 ID |
| **CSV 输出路径** | 下载的 CSV 保存的路径 |
| **生成代码路径** | C# 类生成的路径 |
| **二进制输出路径** | .bytes 文件保存的路径 |
| **自动化选项** | 下载后自动生成代码、生成代码后自动烘焙 |

## 下一步

- [配置 & 下载](/zh/guide/table/setup)
- [JSON → CSV 导出](/zh/guide/table/json-to-csv)
- [代码生成 & 运行时](/zh/guide/table/codegen)
