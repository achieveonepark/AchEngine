# 扩展方法 (Extensions)

`AchEngine.Extensions` 程序集包含了 Unity 开发中常用的 60 多个扩展方法。

## 分类

### 集合
| 类 | 主要功能 |
|---|---|
| `ArrayExt` | 数组工具 |
| `ListExt` | 列表工具 |
| `DictionaryExt` | 字典辅助方法 |
| `IEnumerableExt` / `IListExt` | LINQ 辅助 |
| `LinqE` | 高级 LINQ 扩展 |

### Unity 类型
| 类 | 主要功能 |
|---|---|
| `GameObjectExt` | `GetOrAddComponent`、`SetActiveIfNotNull` |
| `Vector2Ext` / `Vector3Ext` | 向量运算 |
| `ComponentExt` | 组件工具 |
| `RectExt` | Rect 运算 |

### UI
| 类 | 主要功能 |
|---|---|
| `RectTransformExt` | 布局操作 |
| `ImageExt` / `RawImageExt` | 图像辅助方法 |
| `GraphicExt` | Graphic 工具 |
| `ButtonClickedEventExt` | 按钮事件辅助方法 |

### 基本类型
| 类 | 主要功能 |
|---|---|
| `StringExt` / `StringParseExt` | 字符串处理 |
| `IntExt` / `FloatExt` / `BoolExt` | 数值·布尔扩展 |
| `EnumExt` | 枚举工具 |
| `ColorExt` | 颜色转换 |

### 工具类
| 类 | 说明 |
|---|---|
| `Selectable<T>` / `SelectableList<T>` | 可检测变更的值/列表 |
| `MultiDictionary<K,V>` | 一个键存储多个值的字典 |
| `StringAppender` | StringBuilder 封装 |
| `RandomUtils` | 便捷的随机工具 |
| `CameraUtils` | 摄像机坐标转换 |

## 使用示例

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
