# 带状态标记 (MarkerConfigStated) 指南

适用于需要根据状态改变外观的标记，支持锁定、解锁、建造、损坏等多种状态。

## 数据结构

```csharp
public struct MarkerConfigStated
{
    public ConfigEntryWrapper<string> Color;  // 基础颜色
    public ConfigEntryWrapper<string> Sign;   // 符号
    public MarkerConfigColor Rebuilding;      // 重建中
    public MarkerConfigColor Destroyed;       // 已损坏
    public MarkerConfigColor Unpaid;          // 未付款
    public MarkerConfigColor Locked;          // 已锁定
    public MarkerConfigColor Unlocked;        // 已解锁
    public MarkerConfigColor Wrecked;         // 已毁坏
    public MarkerConfigColor Building;        // 建造中
}
```

## 添加步骤

### 1. MarkerStyle.cs - 添加配置

```csharp
// 字段声明
public static MarkerConfigStated MyBuilding;

// ConfigBind 方法中
MyBuilding.Sign = config.Bind("MyBuilding", "Sign", "", "");
MyBuilding.Color = config.Bind("MyBuilding", "Color", "0,1,0,1", "");
MyBuilding.Locked.Color = config.Bind("MyBuilding.Locked", "Color", "0.5,0.5,0.5,1", "");
MyBuilding.Building.Color = config.Bind("MyBuilding.Building", "Color", "0,0,1,1", "");
MyBuilding.Wrecked.Color = config.Bind("MyBuilding.Wrecked", "Color", "1,0,0,1", "");
```

### 2. Mapper 中使用

```csharp
public class MyBuildingMapper(TopMapView view) : IComponentMapper
{
    public MapMarkerType? MarkerType => MapMarkerType.MyBuilding;

    public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
    {
        var obj = component.Cast<PayableUpgrade>();
        
        // 使用动态颜色更新器
        view.TryAddMapMarker(
            component, 
            MarkerStyle.MyBuilding.Color, 
            MarkerStyle.MyBuilding.Sign, 
            Strings.MyBuilding,
            comp => GetCount(comp),
            comp => GetColor(comp, resolverType)  // 动态颜色
        );
    }

    private int GetCount(Component comp)
    {
        var p = comp.GetComponent<PayableUpgrade>();
        if (p == null) return 0;
        return p.IsLocked(GetLocalPlayer(), out _) ? 0 : p.Price;
    }

    private ConfigEntryWrapper<string> GetColor(Component comp, ResolverType resolverType)
    {
        // 建造中状态
        if (resolverType == ResolverType.Scaffolding)
            return MarkerStyle.MyBuilding.Building.Color;
        
        var p = comp.GetComponent<PayableUpgrade>();
        if (p == null) return MarkerStyle.MyBuilding.Color;
        
        // 锁定状态
        if (p.IsLocked(GetLocalPlayer(), out var reason))
        {
            bool isLocked = reason != LockIndicator.LockReason.NotLocked 
                         && reason != LockIndicator.LockReason.NoUpgrade;
            if (isLocked) return MarkerStyle.MyBuilding.Locked.Color;
        }
        
        return MarkerStyle.MyBuilding.Color;
    }
}
```

### 3. MarkerStyle.cfg

```ini
[MyBuilding]

# Setting type: String
# Default value: 0,1,0,1
Color = 0,1,0,1

# Setting type: String
# Default value: 
Sign = 

[MyBuilding.Locked]

# Setting type: String
# Default value: 0.5,0.5,0.5,1
Color = 0.5,0.5,0.5,1

[MyBuilding.Building]

# Setting type: String
# Default value: 0,0,1,1
Color = 0,0,1,1

[MyBuilding.Wrecked]

# Setting type: String
# Default value: 1,0,0,1
Color = 1,0,0,1
```

## 状态优先级

1. **Wrecked/Destroyed** (红色) - 最高
2. **Building/Rebuilding** (蓝色)
3. **Unpaid** (红色)
4. **Locked** (灰色)
5. **正常/Unlocked** (绿色) - 默认

## 配置节命名

```
[MarkerName]           # 基础配置
[MarkerName.Locked]    # 锁定状态
[MarkerName.Unlocked]  # 解锁状态
[MarkerName.Building]  # 建造中
[MarkerName.Wrecked]   # 已毁坏
[MarkerName.Unpaid]    # 未付款
```
