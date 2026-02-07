# 简单标记 (MarkerConfig) 完整指南

简单标记是最基础的标记类型，包含颜色 (Color) 和符号 (Sign) 两个配置项。

## 适用场景

- 单一状态的标记
- 不需要根据状态改变外观的标记
- 示例：ShopForge, Beach, Portal, BeggarCamp

## 数据结构

```csharp
public struct MarkerConfig
{
    public ConfigEntryWrapper<string> Color;  // RGBA 格式: "1,1,1,1"
    public ConfigEntryWrapper<string> Sign;   // 显示符号，如 "♜"
}
```

## 完整添加步骤

### 步骤 1: 在 MarkerStyle.cs 中添加字段

位置：`OverlayMap/Config/MarkerStyle.cs`

**A. 添加静态字段声明**（在类顶部，与其他字段一起）：

```csharp
public class MarkerStyle
{
    // ... 现有字段 ...
    public static MarkerConfig ShopForge;
    public static MarkerConfig ShopScythe;  // <-- 新增
    public static MarkerConfigStated CitizenHouse;
    // ...
}
```

**B. 在 ConfigBind 方法中添加配置绑定**（在方法内，推荐按字母顺序插入）：

```csharp
public static void ConfigBind(ConfigFile config)
{
    // ... 现有绑定 ...
    
    ShopForge.Color = config.Bind("ShopForge", "Color", "1,1,1,1", "");
    ShopForge.Sign = config.Bind("ShopForge", "Sign", "", "");

    ShopScythe.Color = config.Bind("ShopScythe", "Color", "1,1,1,1", "");  // <-- 新增
    ShopScythe.Sign = config.Bind("ShopScythe", "Sign", "", "");           // <-- 新增

    CitizenHouse.Sign = config.Bind("CitizenHouse", "Sign", "", "");
    // ...
}
```

### 步骤 2: 在 Strings.cs 中添加字符串（可选但推荐）

如果标记需要在 UI 中显示名称，添加字符串配置：

位置：`OverlayMap/Config/Strings.cs`

**A. 添加静态字段声明**：

```csharp
public class Strings
{
    // ... 现有字段 ...
    public static ConfigEntryWrapper<string> ShopForge;
    public static ConfigEntryWrapper<string> ShopScythe;  // <-- 新增
    public static ConfigEntryWrapper<string> Sleipnir;
    // ...
}
```

**B. 在 ConfigBind 方法中添加配置绑定**：

```csharp
ShopForge = config.Bind("Strings", "ShopForge", "Smithy", "");
ShopScythe = config.Bind("Strings", "ShopScythe", "Scythe", "");  // <-- 新增
Sleipnir = config.Bind("Strings", "Sleipnir", "Sleipnir", "");
```

### 步骤 3: 在 Mapper 中实现标记逻辑

根据标记关联的游戏组件，在相应的 Mapper 中添加处理逻辑。

#### 情况 A: 复用现有 Mapper（如商店类型）

位置：`OverlayMap/Gui/TopMap/Mappers/PayableShopMapper.cs`

```csharp
public class PayableShopMapper(TopMapView view) : IComponentMapper
{
    public void Map(Component component)
    {
        var obj = component.Cast<PayableShop>();
        var shopType = obj.GetComponent<ShopTag>().type;
        switch (shopType)
        {
            case PayableShop.ShopType.Forge:
                view.TryAddMapMarker(component, MarkerStyle.ShopForge.Color, MarkerStyle.ShopForge.Sign, Strings.ShopForge, comp => comp.Cast<PayableShop>().GetItemCount());
                break;
            case PayableShop.ShopType.Scythe:  // <-- 新增
                view.TryAddMapMarker(component, MarkerStyle.ShopScythe.Color, MarkerStyle.ShopScythe.Sign, Strings.ShopScythe, comp => comp.Cast<PayableShop>().GetItemCount());
                break;
        }
    }
}
```

#### 情况 B: 创建新 Mapper（全新类型）

创建新文件：`OverlayMap/Gui/TopMap/Mappers/YourMarkerMapper.cs`

```csharp
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class YourMarkerMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.YourMarker;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.YourMarker.Color, MarkerStyle.YourMarker.Sign, Strings.YourMarker);
        }
    }
}
```

### 步骤 4: 更新 MarkerStyle.cfg 配置文件

位置：`OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.MarkerStyle.cfg`

在适当位置添加配置节（保持字母顺序）：

```ini
[ShopForge]

# Setting type: String
# Default value: 1,1,1,1
Color = 1,1,1,1

# Setting type: String
# Default value: 
Sign = 

[ShopScythe]

# Setting type: String
# Default value: 1,1,1,1
Color = 1,1,1,1

# Setting type: String
# Default value: 
Sign = 

[StatsInfo]
```

### 步骤 5: 更新语言配置文件

#### 英文配置（en-US）
位置：`OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.Language_en-US.cfg`

```ini
# Setting type: String
# Default value: Smithy
ShopForge = Smithy

# Setting type: String
# Default value: Scythe
ShopScythe = Scythe

# Setting type: String
# Default value: Sleipnir
Sleipnir = Sleipnir
```

#### 中文配置（zh-CN）
位置：`OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.Language_zh-CN.cfg`

```ini
# Setting type: String
# Default value: Smithy
ShopForge = 铁匠铺

# Setting type: String
# Default value: Scythe
ShopScythe = 镰刀铺

# Setting type: String
# Default value: Sleipnir
Sleipnir = 神骏天马
```

#### 俄文配置（ru-RU）
位置：`OverlayMap/ConfigPrefabs/KingdomMod.OverlayMap.Language.ru-RU.cfg`

```ini
# Setting type: String
# Default value: Smithy
ShopForge = Кузница

# Setting type: String
# Default value: Scythe
ShopScythe = Косарь

# Setting type: String
# Default value: Sleipnir
Sleipnir = Слейпнир
```

## TryAddMapMarker 方法签名

```csharp
// 基础版本 - 只有颜色和符号
public bool TryAddMapMarker(Component component, ConfigEntryWrapper<string> color, ConfigEntryWrapper<string> sign)

// 带标签版本 - 显示名称
public bool TryAddMapMarker(Component component, ConfigEntryWrapper<string> color, ConfigEntryWrapper<string> sign, ConfigEntryWrapper<string> label)

// 带动态数量版本 - 显示数量和名称
public bool TryAddMapMarker(Component component, ConfigEntryWrapper<string> color, ConfigEntryWrapper<string> sign, ConfigEntryWrapper<string> label, Func<Component, int> getCount)
```

## 颜色格式

颜色使用 RGBA 格式，四个值范围都是 0-1：

```
"1,0,0,1"     // 红色，不透明
"0,1,0,1"     // 绿色，不透明
"0,0,1,1"     // 蓝色，不透明
"1,1,1,1"     // 白色，不透明
"0,0,0,1"     // 黑色，不透明
"1,1,1,0.5"   // 白色，半透明
"0.5,0.5,0.5,1" // 灰色，不透明
```

## 常用符号参考

```
♜  城堡
۩  墙体
∧  土堆
≈  河流
♣  浆果丛
⚔  武器
⛏  工具
⚒  铁匠
⚓  码头
⛵  船只
🏠 房屋
⚡  闪电
🔥 火焰
```

## 检查清单

- [ ] MarkerStyle.cs 中添加了静态字段
- [ ] MarkerStyle.cs 中添加了配置绑定
- [ ] Strings.cs 中添加了字符串字段（如需要）
- [ ] Strings.cs 中添加了配置绑定（如需要）
- [ ] Mapper 中实现了标记逻辑
- [ ] MarkerStyle.cfg 中添加了配置节
- [ ] Language_en-US.cfg 中添加了英文字符串
- [ ] Language_zh-CN.cfg 中添加了中文字符串
- [ ] Language_ru-RU.cfg 中添加了俄文字符串
