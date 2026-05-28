# 键常量代码生成

硬编码字符串键容易产生拼写错误。
AchEngine 提供了将 JSON 键转换为**类型安全的嵌套类**的代码生成器。

## 转换示例

JSON 键:
```json
{
  "menu.start": "게임 시작",
  "menu.settings": "설정",
  "dialog.confirm": "확인",
  "item.sword.name": "철 검"
}
```

生成的 C# 类:
```csharp
// 자동 생성 — 직접 수정하지 마세요
public static class L
{
    public static class Menu
    {
        public const string Start    = "menu.start";
        public const string Settings = "menu.settings";
    }

    public static class Dialog
    {
        public const string Confirm = "dialog.confirm";
    }

    public static class Item
    {
        public static class Sword
        {
            public const string Name = "item.sword.name";
        }
    }
}
```

## 代码生成设置

在 **Project Settings › AchEngine › Localization › 键常量代码生成** 中:

| 项目 | 默认值 |
|---|---|
| **类名** | `L` |
| **命名空间** | (留空则为全局命名空间) |
| **输出路径** | `Assets/Generated/` |

点击 **生成键常量** 按钮后，会生成 `{输出路径}/{类名}.cs` 文件。

## LocalizedString 组件

在 Inspector 中指定键时，使用 `LocalizedString` 类型。

```csharp
public class ItemNameDisplay : MonoBehaviour
{
    [SerializeField] private LocalizedString _nameKey;

    private void Start()
    {
        GetComponent<Text>().text = _nameKey.Value;
    }
}
```

在 Inspector 的 `_nameKey` 字段中输入键后，自定义 PropertyDrawer 会
显示当前区域设置的翻译预览。
