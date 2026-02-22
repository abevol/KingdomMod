# AGENTS.md - KingdomMod 仓库指南

---

## 项目概述

这是一个基于 BepInEx 插件框架为游戏 **Kingdom Two Crowns** 开发的 C# .NET 模组项目。
同时支持 IL2CPP 和 Mono 版本的游戏。

**项目**：OverlayMap, StaminaBar, DevTools, BetterPayableUpgrade, SharedLib
**游戏托管库**：`_libs/BIE6_Mono/Managed/`
**游戏参考源代码**：`_libs/BIE6_Mono/Decompiled/v2.3.1/Assembly-CSharp/`  
此目录包含反编译的游戏源代码，用于实现模组功能时参考。

---

## 构建命令

```bash
# 构建 Debug 版本（使用 IL2CPP 库）
dotnet build -c Debug

# 为 IL2CPP 版本构建所有项目（发布版）
dotnet build -c BIE6_IL2CPP

# 为 Mono 版本构建所有项目（发布版）
dotnet build -c BIE6_Mono

# 构建特定项目
dotnet build OverlayMap/OverlayMap.csproj -c Debug
```

**构建配置**：

- `Debug`：使用 IL2CPP 的开发构建
- `BIE6_IL2CPP`：IL2CPP 游戏版本的发布版
- `BIE6_Mono`：Mono 游戏版本的发布版（netstandard2.1）

**构建产物**：通过 MSBuild 目标自动复制到游戏插件文件夹。

---

## 代码风格指南

### 格式化（来自 .editorconfig）

- **缩进**：4 个空格（不使用制表符）
- **行尾**：CRLF（Windows 风格）
- **编码**：.cs 文件使用带 BOM 的 UTF-8
- **删除行尾空白**：是
- **大括号**：必需（csharp_prefer_braces = true）
- **命名空间**：块作用域（非文件作用域）

### 命名约定

- **类型**（类、结构、接口、枚举）：PascalCase
- **接口**：使用 `I` 前缀（例如 `IComponentMapper`）
- **方法/属性/事件**：PascalCase
- **字段/参数/变量**：camelCase
- **私有字段**：`_camelCase`（下划线前缀）

### 语言特性

- 使用最新的 C# 版本（`<LangVersion>latest</LangVersion>`）
- 对简单属性/索引器使用表达式主体成员
- 优先使用简单的 using 语句
- 在适当的地方使用模式匹配
- 启用可空引用类型（使用 polyfill 属性）

---

## 架构指南

### 命名空间结构

```txt
KingdomMod.{ModName}           - 根命名空间
KingdomMod.{ModName}.Config    - 配置
KingdomMod.{ModName}.Gui       - UI 组件
KingdomMod.{ModName}.Patchers  - Harmony 补丁
KingdomMod.SharedLib           - 共享工具
```

### 关键设计模式

1. **插件模式**：每个模组都有一个继承自 BasePlugin/BaseUnityPlugin 的主 Plugin 类
2. **持有者模式**：静态持有者类管理模组状态（例如 `OverlayMapHolder`）
3. **补丁模式**：Harmony 补丁放在 `Patchers/` 文件夹下的独立文件中
4. **映射器模式**：组件映射器用于游戏对象可视化

### IL2CPP 与 Mono 兼容性

- 使用条件编译符号：`IL2CPP`、`MONO`、`BIE`、`BIE6`
- IL2CPP 需要 `RegisterTypeInIl2Cpp.RegisterAssembly()`
- IL2CPP：继承自 `BasePlugin`，重写 `Load()`
- Mono：继承自 `BaseUnityPlugin`，使用 `Awake()`

---

## AI 代码生成规则（来自 .cursor/rules）

生成 C# 代码时：

1. **遵循 SOLID 原则**和面向对象设计
2. **单一职责**：每个类都有一个明确的目的
3. **使用 BepInEx 日志**（ManualLogSource）- 禁止使用 Console.WriteLine
4. **添加 XML 文档**（`///`）为公共 API
5. **正确的错误处理**，使用 try-catch 块
6. **遵循现有模式**以保持 IL2CPP/Mono 兼容性
7. **使用 PascalCase/camelCase**，遵循 .NET 约定
8. **包含命名空间声明**和正确的 using 顺序

---

## 项目引用

- 使用来自 `../_libs/` 的本地 DLL 引用（BepInEx、Unity、游戏程序集）
- NuGet：BepInEx.PluginInfoProps
- SharedLib 被所有模组项目引用

---

## 测试

目前没有配置单元测试项目。测试方法：

1. 构建项目
2. 将 DLL 复制到游戏的 BepInEx/plugins 文件夹
3. 运行游戏并检查 BepInEx 日志

---
