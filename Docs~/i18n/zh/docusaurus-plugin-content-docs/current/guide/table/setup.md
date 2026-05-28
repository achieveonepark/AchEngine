# 配置 & 下载

## 初始配置

打开 **Project Settings › AchEngine › Table Loader** 并:

1. 输入 **Spreadsheet ID**
2. 设置 **输出路径** (CSV / 代码 / 二进制)
3. 在 **工作表列表** 中添加工作表 (工作表名、GID、类名)
4. 点击 **保存配置** 按钮

配置保存在 `Assets/TableLoaderSettings.asset` 中。

## Google Sheets 准备

1. 在 Google Sheets 中打开电子表格，并设置为 **文件 › 共享 › 知道链接的所有人 → 查看者**。
2. 从 URL 中复制 **Spreadsheet ID**。
   ```
   https://docs.google.com/spreadsheets/d/[SPREADSHEET_ID]/edit
   ```
3. 复制各工作表 URL 末尾的 `gid=` 值 (每个工作表的 GID)。

## 工作表数据格式

第一行为 **列名 (= C# 属性名)**，从第二行开始为数据。

```
| Id  | Name       | Price | IsActive |
|-----|------------|-------|----------|
| 101 | Iron Sword | 500   | TRUE     |
| 102 | Magic Wand | 1200  | TRUE     |
| 103 | Old Shield | 100   | FALSE    |
```

:::tip 支持的类型
`int`、`float`、`bool`、`string`、`long`。列名为 `Id` 的行用作主键。
:::

## Project Settings 配置

在 **Project Settings › AchEngine › Table Loader** 中设置各项目。

| 项目 | 说明 |
|---|---|
| **Spreadsheet ID** | Google Sheets URL 中 `/d/` 后面的 ID |
| **CSV 输出路径** | 下载的 CSV 保存的路径 |
| **生成代码路径** | C# 类生成的路径 |
| **二进制输出路径** | .bytes 文件保存的路径 |
| **自动化选项** | 下载后自动生成代码、生成代码后自动烘焙 |

## 工作表注册

在工作表列表中添加各工作表。

| 项目 | 说明 |
|---|---|
| 启用 | 是否处理该工作表 |
| 工作表名 | Google Sheets 的工作表名标签 |
| GID | 工作表唯一 ID (URL 中的 `gid=` 参数) |
| 类名 | 生成的 C# 类名 |

## CSV 下载

点击 **打开 Table Loader 窗口** 按钮，或从 **Tools › AchEngine › Table Loader** 菜单进入

1. 点击 **Download CSV** → 将各工作表下载为 CSV
2. 完成后会在 `csvOutputPath` 中生成文件。

## 自动化选项

| 选项 | 行为 |
|---|---|
| 下载后自动生成代码 | CSV 下载完成时立即生成代码 |
| 生成代码后自动烘焙 | 代码生成完成时立即烘焙 |

同时启用两个选项后，**Download → Generate → Bake** 可通过一次点击执行。

## JSON → CSV 导出

当烘焙结果以 JSON 形式生成时，可从 **AchEngine › Table JSON to CSV** 菜单将其重新转换为用于 Google Sheets 导入的 CSV。
支持单个文件和文件夹单位的转换。

详细内容请参考 [JSON → CSV 导出](/guide/table/json-to-csv)。
