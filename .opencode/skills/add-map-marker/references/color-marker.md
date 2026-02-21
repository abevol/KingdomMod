# 纯颜色标记 (MarkerConfigColor) 指南

仅包含颜色配置，适用于不需要符号、只需要颜色区分的场景，如文字颜色、线条颜色等。

## 数据结构

```csharp
public struct MarkerConfigColor
{
    public ConfigEntryWrapper<string> Color;  // RGBA: "1,1,1,1"
}
```

## 添加步骤

### 1. MarkerStyle.cs - 添加配置

```csharp
// 字段声明
public static MarkerConfigColor MyInfo;

// ConfigBind 方法中
MyInfo.Color = config.Bind("MyInfo", "Color", "1,1,1,1", "");
```

### 2. MarkerStyle.cfg

```ini
[MyInfo]

# Setting type: String
# Default value: 1,1,1,1
Color = 1,1,1,1
```

## 典型使用场景

### 信息文字颜色

```csharp
// 在绘制 UI 时使用
GUI.color = MarkerStyle.StatsInfo.Color.GetParsedColor();
GUILayout.Label("Player Count: 10");
GUI.color = Color.white;
```

### 连接线颜色

```csharp
// 在绘制线条时使用
var color = MarkerStyle.WallLine.Color.GetParsedColor();
Drawing.DrawLine(start, end, color, thickness);
```

## 与简单标记的区别

| 特性 | MarkerConfig | MarkerConfigColor |
|------|--------------|-------------------|
| Color | ✅ | ✅ |
| Sign | ✅ | ❌ |
| 用途 | 地图标记 | 文字/线条颜色 |
| Strings | 需要 | 不需要 |
