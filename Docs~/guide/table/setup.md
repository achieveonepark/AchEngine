# 설정 & 다운로드

## 초기 설정

**Project Settings › AchEngine › Table Loader** 를 열고:

1. **Spreadsheet ID** 입력
2. **출력 경로** 설정 (CSV / 코드 / 바이너리)
3. **시트 목록** 에 시트 추가 (시트명, GID, 클래스명)
4. **설정 저장** 버튼 클릭

설정은 `Assets/TableLoaderSettings.asset`에 저장됩니다.

## Google Sheets 준비

1. Google Sheets에서 스프레드시트를 열고 **파일 › 공유 › 링크가 있는 모든 사용자 → 뷰어** 로 설정합니다.
2. URL에서 **Spreadsheet ID**를 복사합니다.
   ```
   https://docs.google.com/spreadsheets/d/[SPREADSHEET_ID]/edit
   ```
3. 각 시트 URL 끝의 `gid=` 값을 복사합니다 (시트별 GID).

## 시트 데이터 형식

첫 번째 행은 **컬럼 이름 (= C# 프로퍼티명)**, 두 번째 행부터 데이터입니다.

```
| Id  | Name       | Price | IsActive |
|-----|------------|-------|----------|
| 101 | Iron Sword | 500   | TRUE     |
| 102 | Magic Wand | 1200  | TRUE     |
| 103 | Old Shield | 100   | FALSE    |
```

:::tip 지원 타입
`int`, `float`, `bool`, `string`, `long`. 컬럼명이 `Id`인 행이 기본 키로 사용됩니다.
:::

## Project Settings 구성

**Project Settings › AchEngine › Table Loader** 에서 각 항목을 설정합니다.

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

## CSV 다운로드

**Table Loader 창 열기** 버튼을 클릭하거나 **Tools › AchEngine › Table Loader** 메뉴에서

1. **Download CSV** 클릭 → 각 시트를 CSV로 다운로드
2. 완료 후 `csvOutputPath`에 파일이 생성됩니다.

## 자동화 옵션

| 옵션 | 동작 |
|---|---|
| 다운로드 후 자동 코드 생성 | CSV 다운로드 완료 시 즉시 코드 생성 |
| 코드 생성 후 자동 베이크 | 코드 생성 완료 시 즉시 베이크 |

두 옵션을 모두 활성화하면 **Download → Generate → Bake** 가 한 번의 클릭으로 실행됩니다.
