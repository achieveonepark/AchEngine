# JSON → CSV 导出

Table Loader 可以将烘焙后的 JSON 数据转换为可直接在 Google Sheets 中导入的 CSV。
支持单个 JSON 文件和文件夹单位的转换。

## 打开

在 Unity 顶部菜单中选择 **AchEngine › Table JSON to CSV**。
也可以打开现有的 **AchEngine › Table Loader** 窗口后选择 **JSON→CSV** 标签页。

## 支持的 JSON 格式

可以将以下格式转换为 CSV。

```json
[
  { "Id": 101, "Name": "Iron Sword", "Price": 500, "IsActive": true },
  { "Id": 102, "Name": "Magic Wand", "Price": 1200, "IsActive": true }
]
```

也支持 `Items` 包装器。

```json
{
  "Items": [
    { "Id": 101, "Name": "Iron Sword" }
  ]
}
```

## CSV 输出格式

转换后的 CSV 遵循 Table Loader 使用的架构格式。

```csv
Id,Name,Price,IsActive
int,string,int,bool
101,Iron Sword,500,true
102,Magic Wand,1200,true
```

数组值以 `|` 连接。CSV 文件以 UTF-8 BOM 保存，可减少在 Google Sheets 导入时韩文数据出现乱码的可能性。

## 单个文件转换

1. 通过 **选择 JSON** 选择 `.json` 文件。
2. 通过 **选择 CSV 位置** 指定要保存的 `.csv` 路径。
3. 点击 **单个转换**。

## 文件夹单位转换

1. 通过 **选择 JSON 文件夹** 指定要转换的文件夹。
2. 通过 **选择 CSV 文件夹** 指定输出文件夹。
3. 若要连同子文件夹一起转换，请打开 **包含子文件夹**。
4. 点击 **文件夹转换**。

文件夹转换会保留 `.json` 文件名，仅将扩展名改为 `.csv`。
