# 設定 & ダウンロード

## 初期設定

**Project Settings › AchEngine › Table Loader** を開いて:

1. **Spreadsheet ID** を入力
2. **出力パス** を設定 (CSV / コード / バイナリ)
3. **シート一覧** にシートを追加 (シート名、GID、クラス名)
4. **設定保存** ボタンをクリック

設定は `Assets/TableLoaderSettings.asset` に保存されます。

## Google Sheets の準備

1. Google Sheets でスプレッドシートを開き、**ファイル › 共有 › リンクを知っている全員 → 閲覧者** に設定します。
2. URL から **Spreadsheet ID** をコピーします。
   ```
   https://docs.google.com/spreadsheets/d/[SPREADSHEET_ID]/edit
   ```
3. 各シート URL の末尾の `gid=` の値をコピーします (シートごとの GID)。

## シートデータ形式

1 行目は **列名 (= C# プロパティ名)**、2 行目以降がデータです。

```
| Id  | Name       | Price | IsActive |
|-----|------------|-------|----------|
| 101 | Iron Sword | 500   | TRUE     |
| 102 | Magic Wand | 1200  | TRUE     |
| 103 | Old Shield | 100   | FALSE    |
```

:::tip 対応する型
`int`, `float`, `bool`, `string`, `long`。列名が `Id` の行が主キーとして使用されます。
:::

## Project Settings の構成

**Project Settings › AchEngine › Table Loader** で各項目を設定します。

| 項目 | 説明 |
|---|---|
| **Spreadsheet ID** | Google Sheets URL の `/d/` の後ろの ID |
| **CSV 出力パス** | ダウンロードされた CSV が保存されるパス |
| **生成コードパス** | C# クラスが生成されるパス |
| **バイナリ出力パス** | .bytes ファイルが保存されるパス |
| **自動化オプション** | ダウンロード後の自動コード生成、コード生成後の自動ベイク |

## シートの登録

シート一覧に各シートを追加します。

| 項目 | 説明 |
|---|---|
| 有効 | 該当シートを処理するかどうか |
| シート名 | Google Sheets のシート名タブ |
| GID | シート固有 ID (URL の `gid=` パラメータ) |
| クラス名 | 生成される C# クラス名 |

## CSV ダウンロード

**Table Loader ウィンドウを開く** ボタンをクリックするか、**Tools › AchEngine › Table Loader** メニューから

1. **Download CSV** をクリック → 各シートを CSV としてダウンロード
2. 完了後 `csvOutputPath` にファイルが生成されます。

## 自動化オプション

| オプション | 動作 |
|---|---|
| ダウンロード後の自動コード生成 | CSV ダウンロード完了時にすぐコード生成 |
| コード生成後の自動ベイク | コード生成完了時にすぐベイク |

両方のオプションを有効にすると、**Download → Generate → Bake** がワンクリックで実行されます。

## JSON → CSV エクスポート

ベイク結果が JSON で生成された場合、**AchEngine › Table JSON to CSV** メニューから Google Sheets インポート用の CSV に再変換できます。
個別ファイルとフォルダ単位の変換の両方をサポートします。

詳細は [JSON → CSV エクスポート](/guide/table/json-to-csv) を参照してください。
