---
layout: home

hero:
  name: AchEngine
  text: Unity 集成开发工具包
  tagline: 将 VContainer DI、UI 系统、表加载器、Addressables 与本地化整合在同一个软件包中。
  actions:
    - theme: brand
      text: 开始使用
      link: /zh/guide/
    - theme: alt
      text: GitHub
      link: https://github.com/achieveonepark/AchEngine

features:
  - icon: 🧩
    title: DI (VContainer 封装)
    details: 无需直接处理 VContainer。通过 AchEngineInstaller 与 ServiceLocator 注册并解析服务即可。
    link: /zh/guide/di/
    linkText: 查看详情

  - icon: 🖼️
    title: UI System
    details: 提供基于层级的 View 管理、对象池与过渡动画。在 UIViewCatalog 中注册后,即可通过 ID 或类型打开。
    link: /zh/guide/ui/
    linkText: 查看详情

  - icon: 📊
    title: Table Loader
    details: 从 Google Sheets 下载数据并生成 C# 模型,构建类型安全的数据管线。
    link: /zh/guide/table/
    linkText: 查看详情

  - icon: 📦
    title: Addressables
    details: 支持自动分组注册、基于引用计数的缓存以及场景级生命周期管理,提供贴近实战的资源加载流程。
    link: /zh/guide/addressables/
    linkText: 查看详情

  - icon: 🌐
    title: Localization
    details: 支持基于 JSON 与 CSV 的多语言工作流、区域切换、回退、系统语言检测以及键常量生成。
    link: /zh/guide/localization/
    linkText: 查看详情

  - icon: ⚙️
    title: Project Settings 集成
    details: 在 Project Settings > AchEngine 中集中管理所有模块设置,并快速查看安装状态。

---
