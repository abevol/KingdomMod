# 简单标记 (MarkerConfig) 指南

适用于单一状态的标记，包含颜色和符号两个配置项。

## 数据结构

```csharp
public struct MarkerConfig
{
    public ConfigEntryWrapper<string> Color;  // RGBA: "1,1,1,1"
    public ConfigEntryWrapper<string> Sign;   // 符号: "♜"
}
```

## 添加步骤

### 1. MarkerStyle.cs - 添加配置

```csharp
// 字段声明
public static MarkerConfig MyMarker;

// ConfigBind 方法中
MyMarker.Color = config.Bind("MyMarker", "Color", "1,1,1,1", "");
MyMarker.Sign = config.Bind("MyMarker", "Sign", "", "");
```

### 2. Strings.cs - 添加名称（可选）

```csharp
// 字段声明
public static ConfigEntryWrapper<string> MyMarker;

// ConfigBind 方法中
MyMarker = config.Bind("Strings", "MyMarker", "MyMarker", "");
```

### 3. Mapper 中使用

```csharp
public class MyMapper(TopMapView view) : IComponentMapper
{
    public MapMarkerType? MarkerType => MapMarkerType.MyMarker;

    public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
    {
        // 基础用法
        view.TryAddMapMarker(component, MarkerStyle.MyMarker.Color, MarkerStyle.MyMarker.Sign, Strings.MyMarker);
        
        // 带动态数量
        view.TryAddMapMarker(
            component, 
            MarkerStyle.MyMarker.Color, 
            MarkerStyle.MyMarker.Sign, 
            Strings.MyMarker,
            comp => comp.Cast<MyComponent>().GetCount()  // 动态数量
        );
    }
}
```

### 4. MarkerStyle.cfg

```ini
[MyMarker]

# Setting type: String
# Default value: 1,1,1,1
Color = 1,1,1,1

# Setting type: String
# Default value: 
Sign = 
```

### 5. Language 文件

```ini
# en-US
MyMarker = MyMarker

# zh-CN
MyMarker = 我的标记
```

## 常用符号

| 符号 | 用途 |
|------|------|
| ♜ | 城堡 |
| ۩ | 墙体 |
| ∧ | 土堆 |
| ≈ | 河流 |
| ♣ | 植物/浆果 |
| ۞ | 传送门 |
