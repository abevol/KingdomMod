---
name: add-map-marker
description: 用于在 OverlayMap 模组中添加新的地图标记。涵盖所有类型的标记添加，包括简单标记（MarkerConfig）、带状态标记（MarkerConfigStated）、颜色标记（MarkerConfigColor）等。触发场景包括："添加商店标记"、"添加地图标记"、"增加新标记类型"、"为新游戏对象添加地图标记"等。
---

# 添加地图标记技能

本技能指导如何在 KingdomMod.OverlayMap 项目中添加新的地图标记。

## 核心架构

```
游戏组件 → Resolver → MapMarkerType → Mapper → MapMarker
                ↓                        ↓
         识别组件类型              创建标记并配置样式
```

- **Resolver**: 识别游戏组件，返回对应的 `MapMarkerType`
- **Mapper**: 根据标记类型和组件状态，调用 `TryAddMapMarker` 创建标记
- **MarkerStyle**: 配置标记的颜色、符号等样式
- **Strings**: 配置标记的显示名称（多语言）

## 标记类型选择

| 类型 | 结构 | 适用场景 | 示例 |
|------|------|----------|------|
| **MarkerConfig** | Color + Sign | 单一状态标记 | Beach, Portal, Shop |
| **MarkerConfigStated** | Color + Sign + 状态子配置 | 多状态标记（锁定/建造/损坏等） | Wall, Lighthouse, Mine |
| **MarkerConfigColor** | Color only | 仅颜色（用于线条/文字） | StatsInfo, ExtraInfo |

## 通用添加流程

所有标记类型都遵循以下核心流程：

1. 在 `MarkerStyle.cs` 中添加配置字段和绑定
2. 在 `Strings.cs` 中添加显示名称
3. 在 Mapper 中实现标记逻辑
4. 更新风格配置文件 `MarkerStyle.cfg`
5. 更新语言配置文件 `Language.en-US.cfg`（en-US、ru-RU、zh-CN 多语言支持）

## 关键文件位置

| 文件 | 用途 |
|------|------|
| `OverlayMap/Config/MarkerStyle.cs` | 标记样式配置 |
| `OverlayMap/Config/Strings.cs` | 本地化字符串 |
| `OverlayMap/Gui/TopMap/MapMarkerType.cs` | 标记类型枚举 |
| `OverlayMap/Gui/TopMap/Mappers/*.cs` | 标记映射器 |
| `OverlayMap/Gui/TopMap/Resolvers/*.cs` | 组件解析器 |
| `OverlayMap/Gui/TopMap/TopMapView.cs` | TryAddMapMarker 方法 |
| `OverlayMap/ConfigPrefabs/MarkerStyle.cfg` | 风格配置文件 |
| `OverlayMap/ConfigPrefabs/Language.en-US.cfg` | 英文本地化配置文件 |
| `OverlayMap/Gui/TopMap/MapperInitializer.cs` | 注册组件解析器和标记映射器 |

## TryAddMapMarker 方法签名

```csharp
public MapMarker TryAddMapMarker(
    Component target,
    ConfigEntryWrapper<string> color,       // 颜色 (RGBA: "1,1,1,1")
    ConfigEntryWrapper<string> sign,        // 符号 (如 "♜")
    ConfigEntryWrapper<string> title,       // 标题/名称
    CountUpdaterFn countUpdater = null,     // 动态数量更新器
    ColorUpdaterFn colorUpdater = null,     // 动态颜色更新器
    VisibleUpdaterFn visibleUpdater = null, // 可见性更新器
    MarkerRow row = MarkerRow.Settled)      // 行位置
```

**回调函数签名**：
```csharp
Func<Component, int> CountUpdaterFn;      // 返回显示数量
Func<Component, ConfigEntryWrapper<string>> ColorUpdaterFn;  // 返回颜色配置
Func<Component, bool> VisibleUpdaterFn;   // 返回是否可见
```

## IComponentMapper 接口

```csharp
public interface IComponentMapper
{
    MapMarkerType? MarkerType => null;  // 处理的标记类型
    
    // 旧方法（逐步废弃）
    void Map(Component component) { }
    
    // 新方法（推荐）
    void Map(Component component, NotifierType notifierType, ResolverType resolverType) 
        => Map(component);
}
```

## 颜色格式

RGBA 格式，值范围 0-1：

| 颜色 | 值 | 含义 |
|------|------|------|
| 绿色 | `0,1,0,1` | 正常/已解锁/安全 |
| 红色 | `1,0,0,1` | 损坏/危险/未付款/敌人 |
| 蓝色 | `0,0,1,1` | 建造中/进行中 |
| 灰色 | `0.5,0.5,0.5,1` | 锁定/不可用 |
| 紫色 | `0.62,0,1,1` | 传送门/魔法 |
| 白色 | `1,1,1,1` | 默认/中立 |

## 参考文档

- [简单标记 (MarkerConfig)](references/simple-marker.md)
- [带状态标记 (MarkerConfigStated)](references/stated-marker.md)
- [纯颜色标记 (MarkerConfigColor)](references/color-marker.md)
- [完整示例](references/examples.md)

## 常见问题

### Q: 标记不显示？

检查清单：
1. Resolver 是否正确返回 `MapMarkerType`
2. Mapper 是否已在 `MapperInitializer` 中注册
3. 配置文件是否包含该标记的配置
4. **如果组件继承其他类型并重写了 OnEnable/OnDisable**：必须在 Mapper 中单独 Patch

### Q: 什么时候需要添加 OnEnable/OnDisable Patch？

当目标组件**继承**自其他类型并**重写**了 `OnEnable`/`OnDisable` 方法时：

```csharp
public class WharfMapper(TopMapView view) : IComponentMapper
{
    public void Map(Component component) { /* ... */ }

    // Wharf 继承自 Payable，但重写了 OnEnable/OnDisable
    [HarmonyPatch(typeof(Wharf), nameof(Wharf.OnEnable))]
    private class OnEnablePatch
    {
        public static void Postfix(Wharf __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }
    }

    [HarmonyPatch(typeof(Wharf), nameof(Wharf.OnDisable))]
    private class OnDisablePatch
    {
        public static void Prefix(Wharf __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
```

### Q: 复用现有标记类型还是创建新类型？

- **复用**：如果新对象与现有对象行为相同，只是样式不同（如不同类型的商店）
- **新建**：如果需要独立的状态管理或与其他标记完全区分
