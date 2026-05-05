# Setup & Download

## Initial Setup

Open **Project Settings › AchEngine › Table Loader** and:

1. Enter the **Spreadsheet ID**
2. Set output paths for CSV, code, and binaries
3. Add sheets to the **Sheet List** (sheet name, GID, class name)
4. Click **Save Settings**

Settings are stored in `Assets/TableLoaderSettings.asset`.

## Prepare Google Sheets

1. Open the spreadsheet in Google Sheets and set **File › Share › Anyone with the link → Viewer**.
2. Copy the **Spreadsheet ID** from the URL:
   ```
   https://docs.google.com/spreadsheets/d/[SPREADSHEET_ID]/edit
   ```
3. Copy the `gid=` value from each sheet URL.

## Sheet Data Format

The first row is the **column name (= C# property name)**, and data starts on the second row.

```
| Id  | Name       | Price | IsActive |
|-----|------------|-------|----------|
| 101 | Iron Sword | 500   | TRUE     |
| 102 | Magic Wand | 1200  | TRUE     |
| 103 | Old Shield | 100   | FALSE    |
```

:::tip Supported Types
`int`, `float`, `bool`, `string`, and `long` are supported.
Rows with a column named `Id` use it as the primary key.
:::

## Project Settings Layout

Configure the following fields in **Project Settings › AchEngine › Table Loader**.

| Item | Description |
|---|---|
| **Spreadsheet ID** | The ID after `/d/` in the Google Sheets URL |
| **CSV Output Path** | Where downloaded CSV files are stored |
| **Generated Code Path** | Where generated C# classes are stored |
| **Binary Output Path** | Where baked `.bytes` files are stored |
| **Automation Options** | Auto-generate code after download, auto-bake after code generation |

## Register Sheets

Add each sheet to the sheet list.

| Item | Description |
|---|---|
| Enabled | Whether the sheet is processed |
| Sheet Name | Sheet tab name in Google Sheets |
| GID | Unique sheet ID from the `gid=` URL parameter |
| Class Name | Name of the generated C# class |

## Download CSV Files

Click **Open Table Loader Window** or open **Tools › AchEngine › Table Loader**, then:

1. Click **Download CSV** to download every configured sheet
2. Confirm files are created in `csvOutputPath`

## Automation Options

| Option | Behavior |
|---|---|
| Auto Generate Code after Download | Generate code immediately after CSV download completes |
| Auto Bake after Code Generation | Bake immediately after code generation completes |

If you enable both options, **Download → Generate → Bake** runs with a single click.

