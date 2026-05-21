# RedDot 系统

`RedDot` 是用于管理通知徽章 (红点) 的静态门面。
支持键的层级结构 (以 `/` 分隔),子节点的计数会自动汇总到父节点。

## API

```csharp
namespace AchEngine.UI
{
    public static class RedDot
    {
        // 设置指定节点的计数
        public static void Set(string key, int count);

        // 在计数上增加 delta 值
        public static void Add(string key, int delta);

        // 获取汇总后的计数 (自身 + 所有子节点之和)
        public static int Get(string key);

        // 计数大于 0 时为 true
        public static bool HasDot(string key);

        // 将计数重置为 0
        public static void Clear(string key);

        // 订阅计数变更事件
        public static void Subscribe(string key, Action<int> handler);

        // 取消订阅
        public static void Unsubscribe(string key, Action<int> handler);
    }
}
```

## 全部清空

`RedDot.ClearAll()` —— 一次性将所有键的计数重置为 0。
适用于场景切换或游戏重启等场景。

```csharp
// 场景切换前清空所有红点
RedDot.ClearAll();
SceneManager.LoadScene("MainMenu");
```

## 层级结构

在键中使用 `/` 时会自动构建出树形结构。
子节点的合计计数会自动反映到父节点。

```
"Shop"          → "Shop/New" + "Shop/Sale" 之和
"Shop/New"      → 直接设置的计数
"Shop/Sale"     → 直接设置的计数
```

```csharp
RedDot.Set("Shop/New", 3);    // Shop/New = 3
RedDot.Set("Shop/Sale", 1);   // Shop/Sale = 1

RedDot.Get("Shop");           // → 4 (3 + 1 自动汇总)
RedDot.HasDot("Shop");        // → true
```

## 使用示例

```csharp
// 获得新道具
RedDot.Set("Shop/New", newItemCount);

// 完成任务
RedDot.Add("Quest/Daily", 1);

// 已读处理
RedDot.Clear("Quest/Daily");

// 主菜单按钮 —— 只要 Shop 或 Quest 任一存在则显示
bool showOnMainMenu = RedDot.HasDot("Shop") || RedDot.HasDot("Quest");
```

## RedDotBadge 组件

`RedDotBadge` 是附加到 UI GameObject 上的 MonoBehaviour。
自动检测指定键的计数,启用或禁用 dot 对象。

| 字段 | 说明 |
|---|---|
| `Key` | 要订阅的 RedDot 键 (`"Shop"`、`"Quest/Daily"` 等) |
| `Dot` | 计数 > 0 时启用的 GameObject |
| `Count Label` | (可选) 显示计数的 `Text` 组件。仅在 2 以上时显示 |
| `Clear On Click` | (bool,默认 true) 按钮点击时自动 Clear 该键 |
| `Button` | 用于检测点击的 `Button` 组件。为 null 则不执行自动清除 |

```
[Button GameObject]
 └── [RedDotBadge]  Key = "Shop"
      └── [DotImage]  (计数 > 0 时启用)
           └── [Text]  (可选: 显示如 "3" 的数字)
```

在 `OnEnable` 时自动订阅,`OnDisable` 时自动取消。
对场景切换和对象禁用都是安全的。

### 点击清除红点

当 `Clear On Click` 为 true 且 `Button` 已连接时,按下按钮会自动将该键的计数重置为 0。
无需额外代码即可实现 "已读处理" 行为。

```csharp
// 在代码中直接实现相同行为
_shopButton.onClick.AddListener(() =>
{
    RedDot.Clear("Shop");
    OpenShopPanel();
});
```

## 在代码中直接订阅

也可以不使用组件,直接进行订阅。

```csharp
private void OnEnable()
{
    RedDot.Subscribe("Shop", OnShopChanged);
}

private void OnDisable()
{
    RedDot.Unsubscribe("Shop", OnShopChanged);
}

private void OnShopChanged(int count)
{
    _shopButton.SetDotVisible(count > 0);
}
```

## EnterPlayMode 支持

`RedDot` 通过 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`
在编辑器播放时无需 Domain Reload 即可自动初始化。

## 相关文档

- [UI 系统概述](/zh/guide/ui/)
- [UIView 与生命周期](/zh/guide/ui/views)
