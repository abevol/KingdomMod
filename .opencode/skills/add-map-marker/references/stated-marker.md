# 带状态标记 (MarkerConfigStated) 完整指南

带状态标记用于需要根据对象状态改变外观的标记，支持多种状态如锁定、解锁、建造中、损坏等。

## 适用场景

- 对象有多个状态（锁定/解锁/建造中/已损坏）
- 不同状态需要不同的颜色或符号
- 示例：Castle, Wall, Lighthouse, Mine, Quarry, Boat

## 数据结构

```csharp
public struct MarkerConfigStated
{
    public ConfigEntryWrapper<string> Color;
    public ConfigEntryWrapper<string> Sign;
    public MarkerConfigColor Rebuilding;  // 重建中
    public MarkerConfigColor Destroyed;   // 已损坏
    public MarkerConfigColor Unpaid;      // 未付款
    public MarkerConfigColor Locked;      // 已锁定
    public MarkerConfigColor Unlocked;    // 已解锁
    public MarkerConfigColor Wrecked;     // 已毁坏
    public MarkerConfigColor Building;    // 建造中
}

public struct MarkerConfigColor
{
    public ConfigEntryWrapper<string> Color;
}
```

## 完整添加步骤

### 步骤 1: 在 MarkerStyle.cs 中添加字段

位置：`OverlayMap/Config/MarkerStyle.cs`

**A. 添加静态字段声明**：

```csharp
public class MarkerStyle
{
    // ... 现有字段 ...
    public static MarkerConfigStated Wall;
    public static MarkerConfigStated Lighthouse;  // <-- 新增
    public static MarkerConfig ShopForge;
    // ...
}
```

**B. 在 ConfigBind 方法中添加配置绑定**：

```csharp
public static void ConfigBind(ConfigFile config)
{
    // ... 现有绑定 ...
    
    Wall.Sign = config.Bind("Wall", "Sign", "۩", "");
    Wall.Color = config.Bind("Wall", "Color", "0,1,0,1", "");
    Wall.Wrecked.Color = config.Bind("Wall.Wrecked", "Color", "1,0,0,1", "");
    Wall.Building.Color = config.Bind("Wall.Building", "Color", "0,0,1,1", "");

    // Lighthouse 配置 - 新增
    Lighthouse.Sign = config.Bind("Lighthouse", "Sign", "", "");
    Lighthouse.Color = config.Bind("Lighthouse", "Color", "0,1,0,1", "");
    Lighthouse.Locked.Color = config.Bind("Lighthouse.Locked", "Color", "0.5,0.5,0.5,1", "");
    Lighthouse.Unpaid.Color = config.Bind("Lighthouse.Unpaid", "Color", "1,0,0,1", "");
    Lighthouse.Building.Color = config.Bind("Lighthouse.Building", "Color", "0,0,1,1", "");

    ShopForge.Color = config.Bind("ShopForge", "Color", "1,1,1,1", "");
    // ...
}
```

### 步骤 2: 在 Strings.cs 中添加字符串（可选）

位置：`OverlayMap/Config/Strings.cs`

```csharp
public static ConfigEntryWrapper<string> Wall;
public static ConfigEntryWrapper<string> Lighthouse;  // <-- 新增
public static ConfigEntryWrapper<string> ShopForge;
```

在 ConfigBind 中：

```csharp
Wall = config.Bind("Strings", "Wall", "Wall", "");
Lighthouse = config.Bind("Strings", "Lighthouse", "Lighthouse", "");  // <-- 新增
ShopForge = config.Bind("Strings", "ShopForge", "Smithy", "");
```

### 步骤 3: 创建 Mapper

位置：`OverlayMap/Gui/TopMap/Mappers/LighthouseMapper.cs`

```csharp
using System;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class LighthouseMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Lighthouse;

        public void Map(Component component)
        {
            var obj = component.Cast<PayableUpgrade>();
            var config = GetConfigForState(obj);
            
            view.TryAddMapMarker(
                component, 
                config.Color, 
                MarkerStyle.Lighthouse.Sign, 
                Strings.Lighthouse
            );
        }

        private MarkerConfigColor GetConfigForState(PayableUpgrade upgrade)
        {
            // 根据对象状态返回对应的配置
            if (upgrade.IsLocked())
                return MarkerStyle.Lighthouse.Locked;
            
            if (upgrade.IsUnpaid())
                return MarkerStyle.Lighthouse.Unpaid;
            
            if (upgrade.IsBuilding())
                return MarkerStyle.Lighthouse.Building;
            
            // 默认状态（已解锁）
            return new MarkerConfigColor { Color = MarkerStyle.Lighthouse.Color };
        }

        // 可选：在状态改变时更新标记
        public void OnStateChanged(Component component)
        {
            // 重新映射以更新外观
            Map(component);
        }
    }
}
```

### 步骤 4: 更新 MarkerStyle.cfg 配置文件

位置：`OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.MarkerStyle.cfg`

```ini
[Lighthouse]

# Setting type: String
# Default value: 0,1,0,1
Color = 0,1,0,1

# Setting type: String
# Default value: 
Sign = 

[Lighthouse.Locked]

# Setting type: String
# Default value: 0.5,0.5,0.5,1
Color = 0.5,0.5,0.5,1

[Lighthouse.Unpaid]

# Setting type: String
# Default value: 1,0,0,1
Color = 1,0,0,1

[Lighthouse.Building]

# Setting type: String
# Default value: 0,0,1,1
Color = 0,0,1,1
```

### 步骤 5: 更新语言配置文件

#### en-US
```ini
# Setting type: String
# Default value: Lighthouse
Lighthouse = Lighthouse
```

#### zh-CN
```ini
# Setting type: String
# Default value: Lighthouse
Lighthouse = 灯塔
```

#### ru-RU
```ini
# Setting type: String
# Default value: Lighthouse
Lighthouse = Маяк
```

## 状态判断方法参考

根据游戏对象类型，状态判断方法可能不同：

### PayableUpgrade 类型

```csharp
// 常见状态检查方法
upgrade.IsLocked()      // 是否锁定（未解锁）
upgrade.IsUnlocked()    // 是否已解锁
upgrade.IsBuilding()    // 是否正在建造
upgrade.IsUnpaid()      // 是否未付款
upgrade.IsPaid()        // 是否已付款
upgrade.IsDestroyed()   // 是否已损坏
upgrade.IsRebuilding()  // 是否正在重建
upgrade.GetState()      // 获取状态枚举
```

### 通用状态模式

```csharp
private MarkerConfigColor GetConfigForState(YourComponent component)
{
    // 优先级：损坏 > 建造中 > 锁定 > 正常
    
    if (component.IsDestroyed() || component.IsWrecked())
        return MarkerStyle.YourMarker.Wrecked;
    
    if (component.IsBuilding() || component.IsRebuilding())
        return MarkerStyle.YourMarker.Building;
    
    if (component.IsLocked())
        return MarkerStyle.YourMarker.Locked;
    
    if (component.IsUnpaid())
        return MarkerStyle.YourMarker.Unpaid;
    
    // 正常状态
    return new MarkerConfigColor { Color = MarkerStyle.YourMarker.Color };
}
```

## 配置节命名规范

```
[MarkerName]                    # 基础配置
[MarkerName.Locked]             # 锁定状态
[MarkerName.Unlocked]           # 解锁状态
[MarkerName.Building]           # 建造中
[MarkerName.Rebuilding]         # 重建中
[MarkerName.Wrecked]            # 已毁坏
[MarkerName.Destroyed]          # 已损坏
[MarkerName.Unpaid]             # 未付款
```

## 颜色约定

```
绿色 (0,1,0,1)      - 正常/已解锁/安全
红色 (1,0,0,1)      - 损坏/锁定/危险/未付款
蓝色 (0,0,1,1)      - 建造中/进行中
灰色 (0.5,0.5,0.5,1) - 锁定/不可用/未激活
黄色 (1,1,0,1)      - 警告/注意
白色 (1,1,1,1)      - 默认/中立
```

## 检查清单

- [ ] MarkerStyle.cs 中添加了 `MarkerConfigStated` 字段
- [ ] MarkerStyle.cs 中添加了所有状态的颜色绑定
- [ ] Strings.cs 中添加了字符串（如需要显示名称）
- [ ] Mapper 中实现了状态判断逻辑
- [ ] Mapper 中根据状态返回对应的颜色配置
- [ ] MarkerStyle.cfg 中添加了所有状态节的配置
- [ ] 语言配置文件中添加了对应的字符串
- [ ] 在 Resolver 中正确返回 MapMarkerType
- [ ] 在 MapperInitializer 中注册了 Mapper（如创建新 Mapper）
