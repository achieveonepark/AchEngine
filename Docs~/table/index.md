# Table Loader

Table Loader는 **Google Sheets → CSV → C# 데이터 클래스 → MemoryPack 이진 파일** 파이프라인을 자동화합니다.
런타임에서 `TableManager`로 타입-세이프하게 데이터에 접근할 수 있습니다.

:::info 선택적 패키지
MemoryPack(`com.cysharp.memorypack`)이 없으면 JSON 직렬화로 동작합니다.
:::

## 파이프라인 흐름

```
Google Sheets
      ↓  [에디터: Download CSV]
   CSV 파일
      ↓  [에디터: Generate Code]
  C# 데이터 클래스
      ↓  [에디터: Bake]
  .bytes 이진 파일 (또는 .json)
      ↓  [런타임]
  TableManager.Get<T>()
```

## 설정 위치

**Project Settings › AchEngine › Table Loader** 또는
**Tools › AchEngine › Table Loader 창** 에서 설정합니다.

설정은 `Assets/TableLoaderSettings.asset`에 저장됩니다.

## 주요 설정 항목

| 항목 | 설명 |
|---|---|
| **Spreadsheet ID** | Google Sheets URL의 `/d/` 뒤 ID |
| **CSV 출력 경로** | 다운로드된 CSV가 저장될 경로 |
| **생성 코드 경로** | C# 클래스가 생성될 경로 |
| **바이너리 출력 경로** | .bytes 파일이 저장될 경로 |
| **자동화 옵션** | 다운로드 후 자동 코드 생성, 코드 생성 후 자동 베이크 |

## 시트 등록

시트 목록에 각 시트를 추가합니다.

| 항목 | 설명 |
|---|---|
| 활성 | 해당 시트의 처리 여부 |
| 시트명 | Google Sheets의 시트 이름 탭 |
| GID | 시트 고유 ID (URL의 `gid=` 파라미터) |
| 클래스명 | 생성될 C# 클래스 이름 |

## 런타임 접근

```csharp
using AchEngine;

// 테이블 조회
var itemTable = TableManager.Get<ItemTable>();

// ID로 행 조회
var item = itemTable.Get(101);
Debug.Log($"{item.Name}: {item.Price}골드");

// 전체 순회
foreach (var row in itemTable.All)
{
    Debug.Log(row.Name);
}
```
