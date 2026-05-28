# JSON to CSV Export

Table Loader can convert baked JSON table data into CSV files that can be imported directly into Google Sheets.
Both single-file and folder-based conversion are supported.

## Open the Tool

In Unity, select **AchEngine › Table JSON to CSV** from the top menu.
You can also open **AchEngine › Table Loader** and switch to the **JSON→CSV** tab.

## Supported JSON Shapes

The exporter supports JSON arrays:

```json
[
  { "Id": 101, "Name": "Iron Sword", "Price": 500, "IsActive": true },
  { "Id": 102, "Name": "Magic Wand", "Price": 1200, "IsActive": true }
]
```

It also supports an `Items` wrapper:

```json
{
  "Items": [
    { "Id": 101, "Name": "Iron Sword" }
  ]
}
```

## CSV Output

Generated CSV files use the Table Loader schema format.

```csv
Id,Name,Price,IsActive
int,string,int,bool
101,Iron Sword,500,true
102,Magic Wand,1200,true
```

Array values are joined with `|`. CSV files are saved with a UTF-8 BOM to reduce encoding issues when importing Korean or other non-ASCII text into Google Sheets.

## Single File Export

1. Click **JSON 선택** and choose a `.json` file.
2. Click **CSV 위치 선택** and choose the output `.csv` path.
3. Click **개별 변환**.

## Folder Export

1. Click **JSON 폴더 선택** and choose the input folder.
2. Click **CSV 폴더 선택** and choose the output folder.
3. Enable **하위 폴더 포함** to include subfolders.
4. Click **폴더 변환**.

Folder export keeps each `.json` file name and changes only the extension to `.csv`.
