# JSON → CSV 내보내기

Table Loader는 베이크된 JSON 데이터를 Google Sheets에서 바로 임포트할 수 있는 CSV로 변환할 수 있습니다.
개별 JSON 파일과 폴더 단위 변환을 모두 지원합니다.

## 열기

Unity 상단 메뉴에서 **AchEngine › Table JSON to CSV** 를 선택합니다.
기존 **AchEngine › Table Loader** 창을 연 뒤 **JSON→CSV** 탭을 선택해도 됩니다.

## 지원 JSON 형식

다음 형식을 CSV로 변환할 수 있습니다.

```json
[
  { "Id": 101, "Name": "Iron Sword", "Price": 500, "IsActive": true },
  { "Id": 102, "Name": "Magic Wand", "Price": 1200, "IsActive": true }
]
```

`Items` 래퍼도 지원합니다.

```json
{
  "Items": [
    { "Id": 101, "Name": "Iron Sword" }
  ]
}
```

## CSV 출력 형식

변환된 CSV는 Table Loader가 사용하는 스키마 형식을 따릅니다.

```csv
Id,Name,Price,IsActive
int,string,int,bool
101,Iron Sword,500,true
102,Magic Wand,1200,true
```

배열 값은 `|` 로 연결됩니다. CSV 파일은 UTF-8 BOM으로 저장되어 Google Sheets 임포트 시 한글 데이터가 깨질 가능성을 줄입니다.

## 개별 파일 변환

1. **JSON 선택** 으로 `.json` 파일을 선택합니다.
2. **CSV 위치 선택** 으로 저장할 `.csv` 경로를 지정합니다.
3. **개별 변환** 을 클릭합니다.

## 폴더 단위 변환

1. **JSON 폴더 선택** 으로 변환할 폴더를 지정합니다.
2. **CSV 폴더 선택** 으로 출력 폴더를 지정합니다.
3. 하위 폴더까지 변환하려면 **하위 폴더 포함** 을 켭니다.
4. **폴더 변환** 을 클릭합니다.

폴더 변환은 `.json` 파일 이름을 유지하고 확장자만 `.csv`로 바꿉니다.
