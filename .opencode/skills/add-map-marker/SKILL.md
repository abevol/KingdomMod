---
name: add-map-marker
description: 用于在 OverlayMap 模组中添加新的地图标记。涵盖所有类型的标记添加，包括简单标记（MarkerConfig）、带状态标记（MarkerConfigStated）、颜色标记（MarkerConfigColor）等。触发场景包括："添加商店标记"、"添加地图标记"、"增加新标记类型"、"为新游戏对象添加地图标记"等。
---

# 添加地图标记技能

本技能指导如何在 KingdomMod.OverlayMap 项目中添加新的地图标记。

## 快速开始

### 确定标记类型

在添加标记前，首先确定需要的标记类型：

1. **简单标记 (MarkerConfig)** - 基础的颜色+符号配置
   - 示例：ShopForge, Beach, Portal
   - 参考：[references/simple-marker.md](references/simple-marker.md)

2. **带状态标记 (MarkerConfigStated)** - 支持多状态（锁定/解锁/建造中/损坏等）
   - 示例：Castle, Wall, Lighthouse, Mine, Quarry
   - 参考：[references/stated-marker.md](references/stated-marker.md)

3. **纯颜色标记 (MarkerConfigColor)** - 仅颜色配置
   - 示例：StatsInfo, ExtraInfo
   - 参考：[references/color-marker.md](references/color-marker.md)

## 通用添加流程

所有标记类型都遵循以下核心流程：

```
1. 在 MapMarkerType.cs 中添加枚举值（如需要新类型）
2. 在 MarkerStyle.cs 中添加配置定义
3. 在 Strings.cs 中添加显示字符串
4. 在 Mapper 中实现标记逻辑
5. 更新配置文件（MarkerStyle.cfg）
6. 更新语言配置文件（多语言支持）
```

## 按标记类型的详细流程

### 简单标记 (MarkerConfig)

适用于大多数普通标记，配置包含 Color 和 Sign。

**修改文件清单：**
- `OverlayMap/Config/MarkerStyle.cs` - 添加 `MarkerConfig` 字段
- `OverlayMap/Config/Strings.cs` - 添加字符串字段
- `OverlayMap/Gui/TopMap/Mappers/*.cs` - 实现 Mapper 逻辑
- `OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.MarkerStyle.cfg` - 添加配置节
- `OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.Language_*.cfg` - 添加多语言字符串

**完整步骤**：[references/simple-marker.md](references/simple-marker.md)

### 带状态标记 (MarkerConfigStated)

适用于有多个状态的标记（如锁定、解锁、建造中、损坏等）。

**修改文件清单：**
- `OverlayMap/Config/MarkerStyle.cs` - 添加 `MarkerConfigStated` 字段
- `OverlayMap/Config/Strings.cs` - 添加字符串字段
- `OverlayMap/Gui/TopMap/Mappers/*.cs` - 实现 Mapper 逻辑（需处理状态逻辑）
- `OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.MarkerStyle.cfg` - 添加多状态配置节
- `OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.Language_*.cfg` - 添加多语言字符串

**完整步骤**：[references/stated-marker.md](references/stated-marker.md)

### 纯颜色标记 (MarkerConfigColor)

适用于只需要颜色配置的标记。

**修改文件清单：**
- `OverlayMap/Config/MarkerStyle.cs` - 添加 `MarkerConfigColor` 字段
- `OverlayMap/Gui/TopMap/Mappers/*.cs` - 实现 Mapper 逻辑
- `OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.MarkerStyle.cfg` - 添加配置节

**完整步骤**：[references/color-marker.md](references/color-marker.md)

## 架构说明

### 核心组件关系

```
MapMarkerType (枚举)
    ↓
IMarkerResolver (解析器) → 游戏组件 → MapMarkerType
    ↓
IComponentMapper (映射器) → MapMarkerType → 地图标记
    ↓
MarkerStyle (样式配置) + Strings (显示字符串)
```

### 关键文件位置

| 文件 | 用途 |
|------|------|
| `OverlayMap/Gui/TopMap/MapMarkerType.cs` | 标记类型枚举定义 |
| `OverlayMap/Config/MarkerStyle.cs` | 标记样式配置类 |
| `OverlayMap/Config/Strings.cs` | 本地化字符串配置 |
| `OverlayMap/Gui/TopMap/Mappers/*.cs` | 标记映射器实现 |
| `OverlayMap/Gui/TopMap/Resolvers/*.cs` | 组件解析器实现 |

## 常见问题

### Q: 如何确定使用哪种标记类型？

A: 根据游戏对象的特性：
- 只有单一外观 → **简单标记**
- 有锁定/解锁/建造中等状态 → **带状态标记**
- 只需要颜色区分 → **纯颜色标记**

### Q: 已有标记类型可以复用吗？

A: 可以。如果新对象与现有对象行为相同，可以在 Resolver 中返回相同的 `MapMarkerType`，只需在 Mapper 中根据子类型使用不同的样式配置。

例如：`PayableShopResolver` 中 Forge、Bow、Hammer、Scythe 都返回 `MapMarkerType.Shop`，但在 `PayableShopMapper` 中根据 `shopType` 使用不同的样式。

### Q: 需要添加新的 MapMarkerType 吗？

A: 如果现有类型无法区分（如 Forge 和 Scythe 都需要独立配置），则需要：
1. 在 `MapMarkerType.cs` 中添加新枚举值
2. 在 `PayableShopResolver` 中返回不同的类型
3. 创建独立的 Mapper 或修改现有 Mapper

如果现有类型足够（如多个商店共用一个类型但不同样式），则不需要。

## 示例场景

### 场景 1: 添加新商店（已有 Shop 类型）

参考 ShopScythe 的实现：
- 复用 `MapMarkerType.Shop`
- 在 `PayableShopMapper` 中添加 case
- 添加独立的 `ShopScythe` 配置

详见 [references/example-shop.md](references/example-shop.md)

### 场景 2: 添加全新类型的标记

如添加一个 "Windmill"（风车）：
- 在 `MapMarkerType.cs` 添加 `Windmill`
- 创建 `WindmillResolver`
- 创建 `WindmillMapper`
- 添加配置和字符串

详见 [references/example-new-type.md](references/example-new-type.md)

## 配置文件模板

### MarkerStyle.cfg 模板

```ini
[SectionName]
# Setting type: String
# Default value: 1,1,1,1
Color = 1,1,1,1

# Setting type: String
# Default value: 
Sign = 
```

### Language.cfg 模板

```ini
# Setting type: String
# Default value: DisplayName
KeyName = DisplayName
```

## 故障排除

### 标记不显示
1. 检查 Resolver 是否正确识别组件
2. 检查 Mapper 是否正确处理该类型
3. 检查配置文件是否包含该标记的配置

### 颜色/符号不正确
1. 检查 `MarkerStyle.cs` 中的配置绑定是否正确
2. 检查配置文件的配置节名称是否匹配
3. 检查 Mapper 中使用的样式字段是否正确

### 多语言不生效
1. 检查 `Strings.cs` 中的配置绑定
2. 检查语言文件中的键名是否匹配
3. 检查游戏设置的语言是否与配置文件匹配

## 参考资源

- [简单标记完整指南](references/simple-marker.md)
- [带状态标记完整指南](references/stated-marker.md)
- [纯颜色标记完整指南](references/color-marker.md)
- [商店标记示例](references/example-shop.md)
- [新类型标记示例](references/example-new-type.md)
- [现有标记类型参考](references/marker-types-reference.md)
