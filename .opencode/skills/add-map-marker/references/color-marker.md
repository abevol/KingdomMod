# 纯颜色标记 (MarkerConfigColor) 完整指南

纯颜色标记仅包含颜色配置，适用于不需要符号、只需要颜色区分的场景。

## 适用场景

- 只需要颜色配置
- 不需要显示符号
- 通常用于信息显示（如统计信息、额外信息）
- 示例：StatsInfo, ExtraInfo, WallLine

## 数据结构

```csharp
public struct MarkerConfigColor
{
    public ConfigEntryWrapper<string> Color;  // RGBA 格式: "1,1,1,1"
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
    public static MarkerConfigColor StatsInfo;
    public static MarkerConfigColor ExtraInfo;
    public static MarkerConfigColor MyNewInfo;  // <-- 新增
    // ...
}
```

**B. 在 ConfigBind 方法中添加配置绑定**：

```csharp
public static void ConfigBind(ConfigFile config)
{
    // ... 现有绑定 ...
    
    StatsInfo.Color = config.Bind("StatsInfo", "Color", "1,1,1,1", "");
    ExtraInfo.Color = config.Bind("ExtraInfo", "Color", "1,1,1,1", "");
    MyNewInfo.Color = config.Bind("MyNewInfo", "Color", "1,1,1,1", "");  // <-- 新增
    
    // ...
}
```

### 步骤 2: 在 Mapper 中使用

在需要使用该颜色的 Mapper 中：

```csharp
public class SomeMapper(TopMapView view) : IComponentMapper
{
    public void Map(Component component)
    {
        // 使用纯颜色配置
        view.TryAddMapMarker(
            component, 
            MarkerStyle.MyNewInfo.Color,  // 只使用颜色
            new ConfigEntryWrapper<string>("Sign", ""),  // 空符号
            null  // 无标签
        );
    }
}
```

或者创建辅助方法：

```csharp
// 创建纯颜色标记的辅助方法
private void AddColorMarker(Component component, MarkerConfigColor colorConfig)
{
    var emptySign = new ConfigEntryWrapper<string>("Sign", "");
    view.TryAddMapMarker(component, colorConfig.Color, emptySign, null);
}

// 使用
public void Map(Component component)
{
    AddColorMarker(component, MarkerStyle.MyNewInfo);
}
```

### 步骤 3: 更新 MarkerStyle.cfg 配置文件

位置：`OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.MarkerStyle.cfg`

```ini
[StatsInfo]

# Setting type: String
# Default value: 1,1,1,1
Color = 1,1,1,1

[ExtraInfo]

# Setting type: String
# Default value: 1,1,1,1
Color = 1,1,1,1

[MyNewInfo]

# Setting type: String
# Default value: 1,1,1,1
Color = 1,1,1,1
```

## 典型使用场景

### 场景 1: 信息文字颜色

用于统计面板或信息面板的文字颜色：

```csharp
// 在绘制统计信息时使用
GUI.color = MarkerStyle.StatsInfo.Color.GetParsedColor();
GUILayout.Label("Player Count: 10");
GUI.color = Color.white;  // 恢复默认
```

### 场景 2: 连接线颜色

用于绘制线条（如墙体连接线）：

```csharp
// 在绘制线条时使用
var color = MarkerStyle.WallLine.Color.GetParsedColor();
Drawing.DrawLine(start, end, color, thickness);
```

### 场景 3: 背景或边框

用于标记的背景色或边框色：

```csharp
var bgColor = MarkerStyle.ExtraInfo.Color.GetParsedColor();
GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, bgColor, 0, 0);
```

## 配置示例

### 亮色主题

```ini
[StatsInfo]
Color = 1,1,1,1        # 白色

[ExtraInfo]
Color = 0.9,0.9,0.9,1  # 浅灰
```

### 暗色主题

```ini
[StatsInfo]
Color = 0.2,0.2,0.2,1  # 深灰

[ExtraInfo]
Color = 0.1,0.1,0.1,1  # 更深灰
```

### 高对比度

```ini
[StatsInfo]
Color = 1,1,0,1        # 黄色

[ExtraInfo]
Color = 0,1,1,1        # 青色
```

## 检查清单

- [ ] MarkerStyle.cs 中添加了 `MarkerConfigColor` 字段
- [ ] MarkerStyle.cs 中添加了颜色配置绑定
- [ ] MarkerStyle.cfg 中添加了配置节
- [ ] Mapper 或其他代码中正确使用了颜色配置
- [ ] 测试颜色是否正确应用

## 与简单标记的区别

| 特性 | MarkerConfig (简单) | MarkerConfigColor (纯颜色) |
|------|---------------------|---------------------------|
| Color | ✅ | ✅ |
| Sign | ✅ | ❌ |
| 用途 | 完整标记 | 仅颜色 |
| 典型场景 | 地图标记 | 文字/线条颜色 |
| 字符串 | 需要 | 不需要 |
