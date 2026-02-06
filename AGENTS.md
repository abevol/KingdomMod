# AGENTS.md - KingdomMod Repository Guidelines

---

## ⚡ Current Branch Work Context

**Branch:** `dev`  
**Task:**

**Current Status:**

**Key Points:**

**完成条件:**

---

## Project Overview

A C# .NET modding project for the game **Kingdom Two Crowns** using BepInEx plugin framework.
Supports both IL2CPP and Mono game versions.

**Projects**: OverlayMap, StaminaBar, DevTools, BetterPayableUpgrade, SharedLib

**Game Reference Source Code**: `~\Documents\ILSpy\KingdomTwoCrowns\2.3.2\Assembly-CSharp`  
This directory contains decompiled game source code for reference when implementing mod features.

---

## Build Commands

```bash
# Build all projects for IL2CPP version (release)
dotnet build -c BIE6_IL2CPP

# Build all projects for Mono version (release)
dotnet build -c BIE6_Mono

# Build Debug version (uses IL2CPP libs)
dotnet build -c Debug

# Build specific project
dotnet build OverlayMap/OverlayMap.csproj -c BIE6_IL2CPP

# Restore packages
dotnet restore

# Clean
dotnet clean
```

**Build Configurations**:

- `Debug`: Development build with IL2CPP
- `BIE6_IL2CPP`: Release for IL2CPP game version
- `BIE6_Mono`: Release for Mono game version (netstandard2.1)

**Build Artifacts**: Automatically copied to game plugins folder via MSBuild target.

---

## Code Style Guidelines

### Formatting (from .editorconfig)

- **Indent**: 4 spaces (no tabs)
- **Line endings**: CRLF (Windows-style)
- **Encoding**: UTF-8 with BOM for .cs files
- **Trim trailing whitespace**: Yes
- **Braces**: Required (csharp_prefer_braces = true)
- **Namespace**: Block-scoped (not file-scoped)

### Naming Conventions

- **Types** (classes, structs, interfaces, enums): PascalCase
- **Interfaces**: Prefix with `I` (e.g., `IComponentMapper`)
- **Methods/Properties/Events**: PascalCase
- **Fields/Parameters/Variables**: camelCase
- **Private fields**: `_camelCase` (underscore prefix)

### Using Directives

```csharp
// Order: System -> Third-party -> Project
using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using KingdomMod.SharedLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

// Place outside namespace
```

### Project-Specific Patterns

**Conditional Compilation**:

```csharp
#if IL2CPP
using BepInEx.Unity.IL2CPP;
#endif

#if MONO
using BepInEx.Unity.Mono;
#endif
```

**Plugin Structure**:

```csharp
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("KingdomTwoCrowns.exe")]
public class MyPlugin : BasePlugin  // or BaseUnityPlugin for Mono
```

**Logging**:

```csharp
// Use BepInEx ManualLogSource
public ManualLogSource LogSource => Log;  // IL2CPP
public ManualLogSource LogSource => Logger;  // Mono

// Log levels
LogSource.LogInfo($"Message");
LogSource.LogWarning($"Warning");
LogSource.LogError($"Error");
```

**Harmony Patching**:

```csharp
[HarmonyPatch(typeof(TargetClass), nameof(TargetClass.Method))]
public class MyPatcher
{
    [HarmonyPostfix]
    static void Postfix(ref ReturnType __result, ParamType __0)
    {
        // Patch logic
    }
}
```

### Language Features

- Use latest C# version (`<LangVersion>latest</LangVersion>`)
- Use expression-bodied members for simple properties/indexers
- Prefer simple using statements
- Use pattern matching where appropriate
- Nullable reference types enabled (with polyfill attributes)

### Error Handling

```csharp
try
{
    // Risky operation
}
catch (Exception ex)
{
    LogSource.LogError($"HResult: {ex.HResult:X}, {ex.Message}");
    // Re-throw if critical
    throw;
}
```

---

## Architecture Guidelines

### Namespace Structure

```txt
KingdomMod.{ModName}           - Root namespace
KingdomMod.{ModName}.Config    - Configuration
KingdomMod.{ModName}.Gui       - UI components
KingdomMod.{ModName}.Patchers  - Harmony patches
KingdomMod.SharedLib           - Shared utilities
```

### Key Design Patterns

1. **Plugin Pattern**: Each mod has a main Plugin class inheriting from BasePlugin/BaseUnityPlugin
2. **Holder Pattern**: Static holder classes manage mod state (e.g., `OverlayMapHolder`)
3. **Patcher Pattern**: Harmony patches in separate files under `Patchers/` folder
4. **Mapper Pattern**: Component mappers for game object visualization

### IL2CPP vs Mono Compatibility

- Use conditional compilation symbols: `IL2CPP`, `MONO`, `BIE`, `BIE6`
- IL2CPP requires `RegisterTypeInIl2Cpp.RegisterAssembly()`
- IL2CPP: Inherit from `BasePlugin`, override `Load()`
- Mono: Inherit from `BaseUnityPlugin`, use `Awake()`

---

## AI Code Generation Rules (from .cursor/rules)

When generating C# code:

1. **Follow SOLID principles** and object-oriented design
2. **Single responsibility**: Each class has one clear purpose
3. **Use BepInEx logging** (ManualLogSource) - never Console.WriteLine
4. **Add XML documentation** (`///`) for public APIs
5. **Proper error handling** with try-catch blocks
6. **Follow existing patterns** for IL2CPP/Mono compatibility
7. **Use PascalCase/camelCase** per .NET conventions
8. **Include namespace declarations** and proper using order

---

## Project References

- Uses local DLL references from `../_libs/` (BepInEx, Unity, Game assemblies)
- NuGet: BepInEx.PluginInfoProps
- SharedLib referenced by all mod projects

---

## Testing

No unit test projects currently configured. Test by:

1. Build the project
2. Copy DLLs to game's BepInEx/plugins folder
3. Run the game and check BepInEx logs

---

## OverlayMap 新架构设计 (MapMarkerType 枚举重构)

### 📋 架构概述

**重构日期**: 2026-02-06  
**目标**: 通过引入 `MapMarkerType` 枚举，解耦模组代码与游戏代码的类型依赖

### 🎯 核心价值

1. **解决多态歧义**: 游戏中 `PayableShop` 同时表示灯塔/矿井/采石场，旧架构无法区分
2. **打破 IL2CPP 硬链接**: 不再依赖脆弱的 IL2CPP 指针转换
3. **独立 Mapper**: Wall、Lighthouse、Mine、Quarry 等现在有独立的 Mapper 文件
4. **防腐层 (Anti-Corruption Layer)**: `MapMarkerType` 充当游戏代码与模组代码之间的隔离层

### 🏗️ 新架构组件

#### 1. MapMarkerType 枚举

位置: `OverlayMap/Gui/TopMap/MapMarkerType.cs`

定义了 50+ 种地图标记类型，包括：
- 地形类: Beach, River
- 建筑类: Castle, Wall, Cabin, Farmhouse
- 交互建筑: Lighthouse, Mine, Quarry, Shop
- 单位类: Player, Beggar, Deer, Enemy
- 等等...

#### 2. IMarkerResolver 接口

位置: `OverlayMap/Gui/TopMap/IMarkerResolver.cs`

```csharp
public interface IMarkerResolver
{
    Type TargetComponentType { get; }
    MapMarkerType? Resolve(Component component);
}
```

**职责**: 将游戏组件（Component）识别为具体的地图标记类型（MapMarkerType）

#### 3. Resolver 实现

**简单 Resolver** (1:1 映射):
- 位置: `OverlayMap/Gui/TopMap/Resolvers/SimpleResolvers.cs`
- 示例: `CastleResolver`, `BeachResolver`, `PortalResolver` 等 30+ 个

**复杂 Resolver** (1:N 映射):
- 位置: `OverlayMap/Gui/TopMap/Resolvers/ComplexResolvers.cs`
- `PayableUpgradeResolver`: 通过 `PrefabID` 区分 Wall/Lighthouse/Mine/Quarry
- `PayableShopResolver`: 通过 `ShopTag.type` 区分不同商店

#### 4. 新 Mapper 实现

位置: `OverlayMap/Gui/TopMap/Mappers/NewArchitectureMappers.cs`

- `LighthouseMapper`: 独立的灯塔标记映射器
- `MineMapper`: 独立的矿井标记映射器
- `QuarryMapper`: 独立的采石场标记映射器
- `WallMapper`: **首次实现**独立的墙体标记映射器

每个 Mapper 实现 `IComponentMapper` 接口，并声明 `MapMarkerType? MarkerType` 属性。

### 🔄 新架构（旧系统已完全移除）

**TopMapView 核心流程**:

```csharp
public void OnComponentCreated(Component comp)
{
    // 仅使用新架构（Resolver 系统）
    TryResolveAndMap(comp);
}
```

**架构迁移完成** (2026-02-06):
- ✅ 旧 FastLookup 系统已完全移除
- ✅ 所有 Mapper 已迁移到新架构
- ✅ 所有组件类型通过 Resolver 识别
- ✅ 初始化逻辑分离至 `MapperInitializer` 类

### 🏗️ 架构组件

#### MapperInitializer 类

位置: `OverlayMap/Gui/TopMap/MapperInitializer.cs`

**职责**: 
- 注册所有 Resolver 和 Mapper
- 构建 IL2CPP 指针查找缓存
- 将初始化结果设置到 TopMapView

**关键方法**:
- `Initialize(TopMapView view)`: 执行完整初始化流程
- `BuildResolverCache()`: 构建 IL2CPP 指针查找缓存
- `RegisterResolver()`: 注册单个 Resolver

### 📊 数据结构

```csharp
// 新架构（当前唯一系统）
private Dictionary<Type, List<IMarkerResolver>> _resolvers;
private Dictionary<IntPtr, List<IMarkerResolver>> _resolverLookup;  // IL2CPP 优化
private Dictionary<MapMarkerType, IComponentMapper> _mappers;
```

### 🚀 扩展性设计

#### 未来可配置化

```json
// 示例: config/markers.json
{
  "Lighthouse": {
    "icon": "assets/lighthouse.png",
    "color": "#FFD700",
    "sign": "🗼"
  }
}
```

#### 标记过滤系统

```csharp
public class MarkerFilter
{
    public HashSet<MapMarkerType> EnabledTypes { get; set; }
}
```

#### 标记分层

```csharp
public enum MarkerLayer
{
    Terrain,      // Beach, River
    Buildings,    // Castle, Wall
    Interactive,  // Shop, Portal
    Units         // Player, Enemy
}
```

### ✅ 测试检查清单

运行游戏后验证以下功能：

- [ ] 城堡 (Castle) 标记正常显示
- [ ] 墙体 (Wall) 标记正常显示，且有连接线
- [ ] 灯塔 (Lighthouse) 标记正常显示，颜色状态正确
- [ ] 矿井 (Mine) 标记正常显示，颜色状态正确
- [ ] 采石场 (Quarry) 标记正常显示，颜色状态正确
- [ ] 海滩 (Beach)、河流 (River) 等地形标记正常
- [ ] 玩家 (Player)、敌人 (Enemy) 等单位标记正常
- [ ] 所有 50+ 种标记类型均正常工作
- [ ] 检查 BepInEx 日志，确认新架构日志输出正常：`[NewArch] Resolved XXX -> YYY`

### 🔧 维护指南

#### 添加新的标记类型

1. 在 `MapMarkerType` 枚举中添加新类型
2. 创建对应的 Resolver（简单类型用 `SimpleResolver`，复杂类型自定义）
3. 创建对应的 Mapper（如果需要特殊渲染逻辑）
4. 在 `MapperInitializer.Initialize()` 中注册 Resolver 和 Mapper

### 📌 注意事项

- **IL2CPP 指针查找**: 使用 `_resolverLookup` 而非直接的 `Type` 查找，避免类型转换问题
- **性能**: 新架构的查找开销略高于旧架构，但由于触发频率低（仅 OnEnable），可接受
- **日志**: 新架构使用 `[NewArch]` 前缀标记日志，便于调试


