# AchEngine 简介

AchEngine 是将 Unity 开发中常用功能整合到一个 UPM 软件包中的 **集成开发工具包**。

各模块可独立使用,即使未安装 VContainer、MemoryPack、Addressables 等可选软件包,核心功能也能正常运行。

## 模块组成

| 模块 | 说明 | 可选软件包 |
|---|---|---|
| **DI** | VContainer 封装、ServiceLocator | `jp.hadashikick.vcontainer` |
| **UI System** | 基于层级的 View 管理、对象池、过渡 | - |
| **Table Loader** | 从 Google Sheets 生成 C# 数据管线 | `com.cysharp.memorypack` |
| **Addressables** | 资源缓存、自动分组管理、远程发布 | `com.unity.addressables` |
| **Localization** | JSON 本地化、键代码生成 | `com.unity.textmeshpro`(可选) |

## 软件包信息

- **Package ID:** `com.achieve.engine`
- **版本:** 1.0.1
- **最低 Unity 版本:** 6000.3
- **必需依赖:** `com.unity.ugui`

## 可选软件包

安装以下软件包后,对应的符号会被 **自动定义**,从而启用 AchEngine 的高级功能。
即使不安装也不会产生编译错误,仅相关功能不可用。

```
jp.hadashikick.vcontainer   -> 启用 DI 容器              → ACHENGINE_VCONTAINER
com.cysharp.memorypack      -> 启用二进制序列化          → ACHENGINE_MEMORYPACK
com.unity.addressables      -> 启用 Addressables 模块    → ACHENGINE_ADDRESSABLES
com.unity.textmeshpro       -> 启用 TMP Localization     → ACHENGINE_LOCALIZATION_TMP
com.unity.entities          -> 启用 ECS 封装             → ACHENGINE_ENTITIES
```

:::tip
可在 **Project Settings > AchEngine** 的 Overview 中一目了然地查看可选软件包的安装状况,并可通过按钮直接安装。
:::
