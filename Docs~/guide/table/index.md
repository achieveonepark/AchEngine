# Table Loader — 개요

Table Loader는 **Google Sheets → CSV → C# 데이터 클래스 → MemoryPack 이진 파일** 파이프라인을 자동화합니다.
런타임에서 `TableManager`로 타입-세이프하게 데이터에 접근할 수 있습니다.

:::info 선택적 패키지
MemoryPack(`com.cysharp.memorypack`)이 없으면 JSON 직렬화로 동작합니다.
:::

## 파이프라인 흐름

```mermaid
flowchart LR
    GS([("☁ Google\nSheets")])
    CSV[["📄 CSV 파일"]]
    CS[["⚙ C# 클래스\n(자동 생성)"]]
    BIN[["📦 .bytes / .json\n(베이크 결과)"]]
    TM(["🎮 TableManager\n.Get&lt;T&gt;()"])

    GS -- "Download CSV\n(에디터)" --> CSV
    CSV -- "Generate Code\n(에디터)" --> CS
    CS -- "Bake\n(에디터)" --> BIN
    BIN -- "Resources.Load\n(런타임)" --> TM

    style GS   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
    style CSV  fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
    style CS   fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
    style BIN  fill:#1e3a5f,stroke:#f59e0b,color:#fcd34d
    style TM   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
```

## 주요 설정 항목

| 항목 | 설명 |
|---|---|
| **Spreadsheet ID** | Google Sheets URL의 `/d/` 뒤 ID |
| **CSV 출력 경로** | 다운로드된 CSV가 저장될 경로 |
| **생성 코드 경로** | C# 클래스가 생성될 경로 |
| **바이너리 출력 경로** | .bytes 파일이 저장될 경로 |
| **자동화 옵션** | 다운로드 후 자동 코드 생성, 코드 생성 후 자동 베이크 |

## 다음 단계

- [설정 & 다운로드](/guide/table/setup)
- [코드 생성 & 런타임](/guide/table/codegen)
