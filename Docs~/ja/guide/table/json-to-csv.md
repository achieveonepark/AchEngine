# JSON → CSV エクスポート

Table Loader は、ベイクされた JSON データを Google Sheets でそのままインポートできる CSV に変換できます。
個別の JSON ファイルとフォルダ単位の変換の両方をサポートします。

## 開く

Unity 上部のメニューから **AchEngine › Table JSON to CSV** を選択します。
既存の **AchEngine › Table Loader** ウィンドウを開いた後、**JSON→CSV** タブを選択しても構いません。

## 対応する JSON 形式

次の形式を CSV に変換できます。

```json
[
  { "Id": 101, "Name": "Iron Sword", "Price": 500, "IsActive": true },
  { "Id": 102, "Name": "Magic Wand", "Price": 1200, "IsActive": true }
]
```

`Items` ラッパーもサポートします。

```json
{
  "Items": [
    { "Id": 101, "Name": "Iron Sword" }
  ]
}
```

## CSV 出力形式

変換された CSV は Table Loader が使用するスキーマ形式に従います。

```csv
Id,Name,Price,IsActive
int,string,int,bool
101,Iron Sword,500,true
102,Magic Wand,1200,true
```

配列値は `|` で連結されます。CSV ファイルは UTF-8 BOM で保存され、Google Sheets へのインポート時に韓国語データが文字化けする可能性を減らします。

## 個別ファイルの変換

1. **JSON 選択** で `.json` ファイルを選択します。
2. **CSV 位置選択** で保存する `.csv` パスを指定します。
3. **個別変換** をクリックします。

## フォルダ単位の変換

1. **JSON フォルダ選択** で変換するフォルダを指定します。
2. **CSV フォルダ選択** で出力フォルダを指定します。
3. サブフォルダまで変換するには **サブフォルダを含む** をオンにします。
4. **フォルダ変換** をクリックします。

フォルダ変換は `.json` ファイル名を維持し、拡張子のみを `.csv` に変更します。
