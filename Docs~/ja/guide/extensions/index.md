# 拡張メソッド (Extensions)

`AchEngine.Extensions` アセンブリには、Unity 開発で頻繁に使われる 60 個以上の拡張メソッドが含まれています。

## カテゴリ

### コレクション
| クラス | 主な機能 |
|---|---|
| `ArrayExt` | 配列ユーティリティ |
| `ListExt` | リストユーティリティ |
| `DictionaryExt` | ディクショナリヘルパー |
| `IEnumerableExt` / `IListExt` | LINQ 補助 |
| `LinqE` | 高度な LINQ 拡張 |

### Unity 型
| クラス | 主な機能 |
|---|---|
| `GameObjectExt` | `GetOrAddComponent`、`SetActiveIfNotNull` |
| `Vector2Ext` / `Vector3Ext` | ベクトル演算 |
| `ComponentExt` | コンポーネントユーティリティ |
| `RectExt` | Rect 演算 |

### UI
| クラス | 主な機能 |
|---|---|
| `RectTransformExt` | レイアウト操作 |
| `ImageExt` / `RawImageExt` | イメージヘルパー |
| `GraphicExt` | Graphic ユーティリティ |
| `ButtonClickedEventExt` | ボタンイベントヘルパー |

### 基本型
| クラス | 主な機能 |
|---|---|
| `StringExt` / `StringParseExt` | 文字列処理 |
| `IntExt` / `FloatExt` / `BoolExt` | 数値・ブール拡張 |
| `EnumExt` | 列挙型ユーティリティ |
| `ColorExt` | 色変換 |

### ユーティリティクラス
| クラス | 説明 |
|---|---|
| `Selectable<T>` / `SelectableList<T>` | 変更検知が可能な値/リスト |
| `MultiDictionary<K,V>` | 1 つのキーに複数の値を保存するディクショナリ |
| `StringAppender` | StringBuilder ラッパー |
| `RandomUtils` | 便利なランダムユーティリティ |
| `CameraUtils` | カメラ座標変換 |

## 使用例

```csharp
// GameObjectExt
var rb = gameObject.GetOrAddComponent<Rigidbody>();
gameObject.SetActiveIfNotNull(false);

// RectTransformExt
rectTransform.SetLeft(10f);

// StringExt
string result = "Hello".Repeat(3); // "HelloHelloHello"

// Selectable<T>
var hp = new Selectable<int>(100);
hp.mChanged += () => UpdateHpBar(hp.Value);
hp.Value = 80;
```
