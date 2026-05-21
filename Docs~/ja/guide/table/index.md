# Table Loader — 概要

Table Loader は **Google Sheets → CSV → C# データクラス → MemoryPack バイナリファイル** のパイプラインを自動化します。
ランタイムでは `TableManager` を使ってタイプセーフにデータへアクセスできます。

:::info オプションパッケージ
MemoryPack (`com.cysharp.memorypack`) がない場合は JSON シリアライズで動作します。
:::

## パイプラインの流れ

```mermaid
flowchart LR
GS([("☁ Google<br/>Sheets")])
CSV[["📄 CSV ファイル"]]
CS[["⚙ C# クラス<br/>(自動生成)"]]
BIN[["📦 .bytes / .json<br/>(ベイク結果)"]]
TM(["🎮 TableManager<br/>.Get<T>()"])

GS -- "Download CSV<br/>(エディター)" --> CSV
CSV -- "Generate Code<br/>(エディター)" --> CS
CS -- "Bake<br/>(エディター)" --> BIN
BIN -- "Resources.Load<br/>(ランタイム)" --> TM

style GS   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
style CSV  fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
style CS   fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
style BIN  fill:#1e3a5f,stroke:#f59e0b,color:#fcd34d
style TM   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
```

## 主な設定項目

| 項目 | 説明 |
|---|---|
| **Spreadsheet ID** | Google Sheets URL の `/d/` の後ろの ID |
| **CSV 出力パス** | ダウンロードされた CSV が保存されるパス |
| **生成コードパス** | C# クラスが生成されるパス |
| **バイナリ出力パス** | .bytes ファイルが保存されるパス |
| **自動化オプション** | ダウンロード後の自動コード生成、コード生成後の自動ベイク |

## 次のステップ

- [設定 & ダウンロード](/ja/guide/table/setup)
- [JSON → CSV エクスポート](/ja/guide/table/json-to-csv)
- [コード生成 & ランタイム](/ja/guide/table/codegen)
