# 传送阵地图标记设计文档

**日期**: 2026-02-16  
**主题**: PayableTeleporter地图标记  
**状态**: 已批准，待实施

---

## 1. 需求概述

为游戏中的传送阵（PayableTeleporter）添加地图标记显示，使其在OverlayMap上可见。

### 关键信息
- 游戏类型: `PayableTeleporter`，继承自 `Payable`
- 建造状态: 由 `ScaffoldingMapper` 处理（PrefabID = GamePrefabID.Teleporter）
- 显示符号: `⧉`
- 颜色方案: 紫色系

---

## 2. 架构设计

采用现有Resolver+Mapper架构模式，遵循单一职责原则。

### 2.1 组件关系

```
PayableTeleporter (游戏组件)
    ↓
PayableTeleporterResolver (识别组件类型)
    ↓
MapMarkerType.Teleporter (标记类型)
    ↓
PayableTeleporterMapper (渲染逻辑)
    ↓
MapMarker (地图标记)
```

### 2.2 与现有系统的关系

- **ScaffoldingMapper**: 负责建造中的传送阵（PrefabID.Teleporter）
- **PayableTeleporterMapper**: 负责已建造但未激活/已激活的传送阵

---

## 3. 文件变更清单

### 3.1 修改文件

| 文件路径 | 变更内容 |
|---------|---------|
| `OverlayMap/Gui/TopMap/MapMarkerType.cs` | 添加 `Teleporter` 枚举值 |
| `OverlayMap/Gui/TopMap/Resolvers/SimpleResolvers.cs` | 添加 `PayableTeleporterResolver` 类 |
| `OverlayMap/Gui/TopMap/MapperInitializer.cs` | 注册新的Resolver和Mapper |
| `OverlayMap/Config/MarkerStyle.cs` | 添加 `Teleporter` 样式配置 |
| `OverlayMap/Config/Strings.cs` | 添加 `Teleporter` 文本配置 |

### 3.2 新建文件

| 文件路径 | 说明 |
|---------|------|
| `OverlayMap/Gui/TopMap/Mappers/PayableTeleporterMapper.cs` | 传送阵标记渲染逻辑 |

---

## 4. 详细设计

### 4.1 MapMarkerType 枚举

```csharp
// 在传送点/特殊建筑区域添加
/// <summary>传送阵</summary>
Teleporter,
```

### 4.2 PayableTeleporterResolver

```csharp
/// <summary>传送阵解析器</summary>
public class PayableTeleporterResolver : SimpleResolver
{
    public PayableTeleporterResolver() : base(typeof(PayableTeleporter), MapMarkerType.Teleporter) { }
}
```

### 4.3 PayableTeleporterMapper

实现要点：
- 监听 `PayableTeleporter.OnEnable/OnDisable` 事件
- 基础显示：紫色 + 符号 `⧉`
- 支持根据激活状态变化颜色（可选）

### 4.4 样式配置

```csharp
// MarkerStyle.cs
public static MarkerConfig Teleporter;

// 配置值
Teleporter.Color = "0.62,0,1,1"  // 紫色，与Portal一致
Teleporter.Sign = "⧉"
```

### 4.5 文本配置

```csharp
// Strings.cs
public static ConfigEntryWrapper<string> Teleporter;

// 配置值
Teleporter = config.Bind("Strings", "Teleporter", "Teleporter", "");
```

---

## 5. 实现步骤

1. 在 `MapMarkerType.cs` 添加 `Teleporter` 枚举
2. 在 `SimpleResolvers.cs` 添加 `PayableTeleporterResolver`
3. 创建 `PayableTeleporterMapper.cs` 文件
4. 在 `MarkerStyle.cs` 添加样式配置
5. 在 `Strings.cs` 添加文本配置
6. 在 `MapperInitializer.cs` 注册Resolver和Mapper

---

## 6. 验收标准

- [ ] 传送阵在地图上以紫色 `⧉` 符号显示
- [ ] 鼠标悬停显示"Teleporter"提示
- [ ] 建造中的传送阵仍由ScaffoldingMapper正确处理
- [ ] 配置文件可自定义颜色和符号

---

## 7. 备注

- 建造状态由ScaffoldingMapper处理，无需在PayableTeleporterMapper中重复处理
- 颜色使用紫色（0.62,0,1,1），与现有Portal标记保持一致
- 符号使用 ⧉（U+29C9），可在配置中自定义
