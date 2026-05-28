# 更新日志

## 1.0.5

**新功能**
- 新增 `AchTask` / `AchTask<T>`。这是将 UniTask 与 `System.Threading.Tasks.Task` 统一为单一 API 的异步封装。当检测到 `com.cysharp.unitask` 软件包时,会自动定义 `ENABLE_UNITASK` 符号并使用 UniTask 运行;若未安装则回退到 `Task`。提供 `Delay`、`DelayRealtime`、`WaitUntil`、`WhenAll`、`WhenAny`、`CompletedTask` 以及隐式转换。

**文档**
- 新增 `AchTask` 的中文与英文文档(`guide/async`)。

## 1.0.4

**重大变更**
- 移除了 `SoundManager`。所有使用处都需要替换为新的 `AudioManager`。

**新功能**
- 新增 `AudioManager`。用于替代 `SoundManager`,提供 BGM 交叉淡入淡出(`PlayBgm(clip, fadeDuration)`、`StopBgm(fadeDuration)`)、BGM 音量淡变(`SetBgmVolume(volume, fadeDuration)`)、按通道静音(`MuteBgm`、`MuteSfx`、`MuteAll`)、8 通道并发播放的 SFX 池、3D 空间音效(`PlaySfxAt(clip, worldPosition)`)。
- 新增 `AchTimer`。可使用 `AchTimer.Wait(seconds)` / `AchTimer.WaitRealtime(seconds)` 处理简单等待,而 `AchTimer.Start(duration)` 返回的 `AchTimerHandle` 提供 `Elapsed`、`Remaining`、`Progress`(0–1)、`IsDone`、`Cancel()`,并支持直接 `await`。支持 `CancellationToken` 与 `useUnscaledTime`。内部的 `AchTimerRunner` 会在应用启动时自动创建,无需在场景中布置。
- 新增 `UIAchTimer` 组件。通过 `Bind(handle)` / `Unbind()` 将 `AchTimerHandle` 关联到 `Text` 与 `Slider`,实时显示进度。
- 新增 `AchButtonCooldown` 组件。点击后立即禁用 `Button`,并在指定时间后自动重新启用。内置倒计时 `Text` 标签以及 `OnCooldownStart` / `OnCooldownEnd` UnityEvent,提供 `StartCooldown()`、`ResetCooldown()`、`IsCoolingDown`。
- 新增 `AchButtonHold` 组件。在按住按钮期间按设定的间隔反复触发 UnityEvent。`InitialDelay` 与 `RepeatInterval` 均可调整。
- 新增 `AchDebugConsole`。这是不影响 Unity 渲染线程的原生 UI 叠加式调试控制台。在 Android 上以可拖动的 `WindowManager` 叠加层运行(需要 `SYSTEM_ALERT_WINDOW` 权限),在 iOS 上使用层级为 `UIWindowLevelAlert + 100` 的 `UIWindow`,在编辑器中通过 `DrawEditorGUI()` 提供 IMGUI 回退。API:`Show()`、`Hide()`、`Toggle()`、`Clear()`、`IsVisible`。
- 新增 `RedDot.ClearAll()`。可一次性将所有节点的计数重置为 0。
- 为 `RedDotBadge` 新增点击清除功能。新增 `Clear On Click`(默认 `true`)与 `Button` 字段,按下关联按钮时会自动调用相应键的 `RedDot.Clear(key)`。

**文档**
- 修正了与源代码不一致的 15 处 API 错误:`ServiceLocator.Get<T>()` → `Resolve<T>()`、修正 `UIView` 生命周期钩子签名(`object payload`)、修正 `CloseSelf()`、移除不存在的 `Show<T>()` / `Close<T>()` / `CloseLayer()` 重载、修正 `AchEngineScope` ↔ `ServiceLocator` 生命周期示意图、修正 `IServiceBuilder` 注册语法、明示 `ISaveService.Configure()` 的所有权、从寻路文档中移除 `Rigidbody2D.MovePosition()` 引用、修正 `Selectable<T>.mChanged` 事件名、说明 `Build()` 仅支持 GET / POST。
- 为所有新功能添加完整的中文与英文文档:`AudioManager`、`AchTimer` + `UIAchTimer`、`AchButtonCooldown` + `AchButtonHold`、`AchDebugConsole`。
- 在 RedDot 文档中补充了 `ClearAll()` 与点击清除徽章的用法。

## 1.0.3

- 新增 `SaveManager`、`ISaveService`、`LocalSaveService`。这是将保存逻辑从 `PlayerManager` 中抽离出来的抽象层,同时提供同步与异步 API,可在未来无缝替换为 Firestore、AWS 等云端后端。
- 从 `PlayerManager` 中移除保存/加载逻辑。现在仅负责按类型管理数据容器(`Add`、`Get`、`Remove`)。
- 新增 `AchProjectile`。这是无需 Rigidbody2D 的直线/追踪弹一体化弹道组件。
- 将 `AchFollower` 重构为完全独立的组件。移除了对 `AchMover` 的依赖。
- 在 FontAsset Maker 中新增多语言 FontAsset 构建功能。可通过多选选择中文、英文、日文,分别生成各自的 `*_TMP.asset` 文件。
- 所有运行时异步 API 统一使用 `System.Threading.Tasks.Task`。移除了中间抽象 `AchTask`。

## 1.0.2

- 新增面向 Unity Entities 的可选 ECS 辅助工具。包含 World、CommandBuffer、Baker、System、DI 封装。
- 新增 Managers、Singleton、Log、WebRequest、PlayerData、QuickSave 等游戏框架运行时模块。
- 新增 Runtime Extensions 程序集,涵盖 Unity 对象、UI 组件、集合、字符串、委托、Task 以及通用工具。
- 新增 A* 寻路工具与 Grid Baker。
- 新增基于 AchMover 的移动辅助工具。
- 新增 RedDot 通知徽章的运行时功能。
- 新增 Drag、Object Touch、Binding、Open/Close Button 等 UI 组件辅助工具。
- 新增以三个场景展示 AchEngine 主要系统的 Full Sample。
- 在中英文两侧补强了 Addressables、DI、Localization、Table、UI 文档。
- 即使在关闭 Domain Reload 的 Enter Play Mode 下,也能正确初始化静态状态。
- 修复了文档站点、Mermaid 图、交叉链接以及 JSON 处理相关的问题。
- 移除了 Editor Decorators 模块及相关文档。
- 将根目录 README 整理为以文档链接为主的简洁着陆页。

## 1.0.1

- 新增将 Table JSON 数据导出为可供 Google Sheets 导入的 CSV 工具。支持单文件与按文件夹批量转换。
