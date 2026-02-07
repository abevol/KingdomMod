# 示例：添加全新类型的标记

本示例演示如何添加一个全新的标记类型，从 MapMarkerType 枚举开始，到完整的 Mapper 实现。

## 场景

假设游戏中有一种新的建筑 "Windmill"（风车），需要在地图上显示标记，并且需要支持锁定和解锁状态。

## 实施步骤

### 步骤 1: 添加 MapMarkerType 枚举值

**文件**: `OverlayMap/Gui/TopMap/MapMarkerType.cs`

在适当的位置添加新的枚举值：

```csharp
public enum MapMarkerType
{
    // ... 现有值 ...
    
    /// <summary>风车</summary>
    Windmill,
    
    // ... 其他值 ...
}
```

建议按类别组织枚举值，例如放在建筑类或交互建筑类。

### 步骤 2: 创建 Resolver

**文件**: `OverlayMap/Gui/TopMap/Resolvers/WindmillResolver.cs`

```csharp
using System;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    /// <summary>
    /// 风车组件解析器
    /// </summary>
    public class WindmillResolver : IMarkerResolver
    {
        // 目标组件类型
        public Type TargetComponentType => typeof(Windmill);  // 假设游戏中有 Windmill 类

        public MapMarkerType? Resolve(Component component)
        {
            var windmill = component.Cast<Windmill>();
            if (windmill == null) return null;

            // 可以添加额外的判断逻辑
            // 例如：if (!windmill.IsActive) return null;

            return MapMarkerType.Windmill;
        }
    }
}
```

如果风车是 `PayableUpgrade` 的子类型，可以使用 `PayableUpgradeResolver`：

```csharp
public MapMarkerType? Resolve(Component component)
{
    var upgrade = component.Cast<PayableUpgrade>();
    if (upgrade == null) return null;

    // 通过 PrefabID 或其他标识判断
    if (upgrade.PrefabID == "Windmill")
        return MapMarkerType.Windmill;
    
    return null;
}
```

### 步骤 3: 创建 Mapper

**文件**: `OverlayMap/Gui/TopMap/Mappers/WindmillMapper.cs`

```csharp
using System;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class WindmillMapper(TopMapView view) : IComponentMapper
    {
        // 声明此 Mapper 处理的标记类型
        public MapMarkerType? MarkerType => MapMarkerType.Windmill;

        public void Map(Component component)
        {
            var windmill = component.Cast<Windmill>();
            if (windmill == null) return;

            // 根据状态获取配置
            var config = GetConfigForState(windmill);
            
            // 添加地图标记
            view.TryAddMapMarker(
                component, 
                config.Color, 
                MarkerStyle.Windmill.Sign, 
                Strings.Windmill
            );
        }

        private MarkerConfigColor GetConfigForState(Windmill windmill)
        {
            // 根据风车的状态返回不同的颜色配置
            if (windmill.IsLocked())
                return MarkerStyle.Windmill.Locked;
            
            if (windmill.IsBuilding())
                return MarkerStyle.Windmill.Building;
            
            // 默认状态（已解锁）
            return new MarkerConfigColor { Color = MarkerStyle.Windmill.Color };
        }
    }
}
```

### 步骤 4: 注册 Resolver 和 Mapper

**文件**: `OverlayMap/Gui/TopMap/MapperInitializer.cs`

在初始化方法中注册新的 Resolver 和 Mapper：

```csharp
public class MapperInitializer
{
    public static void Initialize(TopMapView view)
    {
        // ... 现有注册 ...
        
        // 注册 Resolver
        RegisterResolver(new WindmillResolver());
        
        // ... 其他 Resolver ...
        
        // 注册 Mapper
        RegisterMapper(MapMarkerType.Windmill, new WindmillMapper(view));
        
        // ... 其他 Mapper ...
    }
    
    private static void RegisterResolver(IMarkerResolver resolver)
    {
        // 实现注册逻辑
    }
    
    private static void RegisterMapper(MapMarkerType type, IComponentMapper mapper)
    {
        // 实现注册逻辑
    }
}
```

注意：实际的注册方法可能有所不同，请参考现有代码中的注册模式。

### 步骤 5: 添加样式配置

**MarkerStyle.cs**：

```csharp
// 字段声明
public static MarkerConfigStated Windmill;  // 使用带状态配置

// ConfigBind 方法
Windmill.Sign = config.Bind("Windmill", "Sign", "⚙", "");  // 使用齿轮符号
Windmill.Color = config.Bind("Windmill", "Color", "0,1,0,1", "");
Windmill.Locked.Color = config.Bind("Windmill.Locked", "Color", "0.5,0.5,0.5,1", "");
Windmill.Building.Color = config.Bind("Windmill.Building", "Color", "0,0,1,1", "");
```

### 步骤 6: 添加字符串

**Strings.cs**：

```csharp
// 字段声明
public static ConfigEntryWrapper<string> Windmill;

// ConfigBind 方法
Windmill = config.Bind("Strings", "Windmill", "Windmill", "");
```

### 步骤 7: 更新配置文件

**MarkerStyle.cfg**：

```ini
[Windmill]

# Setting type: String
# Default value: 0,1,0,1
Color = 0,1,0,1

# Setting type: String
# Default value: ⚙
Sign = ⚙

[Windmill.Locked]

# Setting type: String
# Default value: 0.5,0.5,0.5,1
Color = 0.5,0.5,0.5,1

[Windmill.Building]

# Setting type: String
# Default value: 0,0,1,1
Color = 0,0,1,1
```

### 步骤 8: 更新语言文件

**Language_en-US.cfg**：

```ini
Windmill = Windmill
```

**Language_zh-CN.cfg**：

```ini
Windmill = 风车
```

**Language_ru-RU.cfg**：

```ini
Windmill = Мельница
```

## 完整文件列表

新增或修改的文件：

1. `OverlayMap/Gui/TopMap/MapMarkerType.cs` - 添加枚举值
2. `OverlayMap/Gui/TopMap/Resolvers/WindmillResolver.cs` - 新建
3. `OverlayMap/Gui/TopMap/Mappers/WindmillMapper.cs` - 新建
4. `OverlayMap/Gui/TopMap/MapperInitializer.cs` - 注册 Resolver 和 Mapper
5. `OverlayMap/Config/MarkerStyle.cs` - 添加配置
6. `OverlayMap/Config/Strings.cs` - 添加字符串
7. `OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.MarkerStyle.cfg` - 添加配置节
8. `OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.Language_*.cfg` - 添加多语言

## 测试检查清单

- [ ] 风车在游戏中正确显示标记
- [ ] 锁定状态显示灰色
- [ ] 建造中状态显示蓝色
- [ ] 解锁状态显示绿色
- [ ] 符号正确显示（⚙）
- [ ] 标签显示 "风车"（中文）或 "Windmill"（英文）
- [ ] 配置文件可以被正确加载和修改

## 故障排除

### 标记不显示

1. 检查 Resolver 是否正确识别组件
2. 检查 Mapper 是否已注册
3. 检查 Resolver 是否已注册
4. 查看日志确认组件是否被创建

### 状态颜色不正确

1. 检查 `GetConfigForState` 方法的逻辑
2. 检查游戏对象的状态方法（IsLocked, IsBuilding 等）
3. 检查配置文件中的颜色值

### 符号不显示

1. 检查符号字符是否正确（某些符号可能不支持）
2. 检查字体是否支持该符号
3. 尝试使用简单的 ASCII 字符测试
