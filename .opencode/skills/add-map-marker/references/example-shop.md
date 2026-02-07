# 示例：添加商店标记（复用现有类型）

本示例演示如何在已有 `MapMarkerType.Shop` 类型的基础上，为新的商店类型（如 Scythe）添加独立的配置。

## 场景

游戏中已存在铁匠铺（ShopForge），现在需要添加镰刀铺（ShopScythe）。两者都是商店，但需要在地图上显示不同的配置（颜色、符号等）。

## 实现策略

**策略**：复用 `MapMarkerType.Shop` 枚举，在 `PayableShopMapper` 中根据 `ShopTag.type` 使用不同的样式配置。

**优势**：
- 无需修改 Resolver
- 无需添加新的枚举值
- 配置完全独立

## 逐步实施

### 步骤 1: 确认 Resolver 支持

检查 `PayableShopResolver.cs` 是否已包含 Scythe 类型：

```csharp
public MapMarkerType? Resolve(Component component)
{
    var shop = component.Cast<PayableShop>();
    if (shop == null) return null;

    var shopTag = shop.GetComponent<ShopTag>();
    if (shopTag == null) return null;

    return shopTag.type switch
    {
        PayableShop.ShopType.Forge => MapMarkerType.Shop,
        PayableShop.ShopType.Bow => MapMarkerType.Shop,
        PayableShop.ShopType.Hammer => MapMarkerType.Shop,
        PayableShop.ShopType.Scythe => MapMarkerType.Shop,  // 确认已添加
        _ => MapMarkerType.Shop
    };
}
```

如果未添加，需要在此添加。

### 步骤 2: 添加样式配置

**MarkerStyle.cs**：

```csharp
// 字段声明
public static MarkerConfig ShopForge;
public static MarkerConfig ShopScythe;  // 新增

// ConfigBind 方法
ShopForge.Color = config.Bind("ShopForge", "Color", "1,1,1,1", "");
ShopForge.Sign = config.Bind("ShopForge", "Sign", "", "");

ShopScythe.Color = config.Bind("ShopScythe", "Color", "1,1,1,1", "");  // 新增
ShopScythe.Sign = config.Bind("ShopScythe", "Sign", "", "");           // 新增
```

### 步骤 3: 添加字符串

**Strings.cs**：

```csharp
// 字段声明
public static ConfigEntryWrapper<string> ShopForge;
public static ConfigEntryWrapper<string> ShopScythe;  // 新增

// ConfigBind 方法
ShopForge = config.Bind("Strings", "ShopForge", "Smithy", "");
ShopScythe = config.Bind("Strings", "ShopScythe", "Scythe", "");  // 新增
```

### 步骤 4: 在 Mapper 中添加处理

**PayableShopMapper.cs**：

```csharp
public void Map(Component component)
{
    var obj = component.Cast<PayableShop>();
    var shopType = obj.GetComponent<ShopTag>().type;
    switch (shopType)
    {
        case PayableShop.ShopType.Forge:
            view.TryAddMapMarker(
                component, 
                MarkerStyle.ShopForge.Color, 
                MarkerStyle.ShopForge.Sign, 
                Strings.ShopForge, 
                comp => comp.Cast<PayableShop>().GetItemCount()
            );
            break;
            
        case PayableShop.ShopType.Scythe:  // 新增
            view.TryAddMapMarker(
                component, 
                MarkerStyle.ShopScythe.Color, 
                MarkerStyle.ShopScythe.Sign, 
                Strings.ShopScythe, 
                comp => comp.Cast<PayableShop>().GetItemCount()
            );
            break;
    }
}
```

### 步骤 5: 更新配置文件

**MarkerStyle.cfg**：

```ini
[ShopForge]
Color = 1,1,1,1
Sign = ⚒

[ShopScythe]          # 新增
Color = 1,1,1,1
Sign = 🌾             # 可以使用镰刀相关的符号
```

### 步骤 6: 更新语言文件

**Language_en-US.cfg**：

```ini
ShopForge = Smithy
ShopScythe = Scythe   # 新增
```

**Language_zh-CN.cfg**：

```ini
ShopForge = 铁匠铺
ShopScythe = 镰刀铺   # 新增
```

**Language_ru-RU.cfg**：

```ini
ShopForge = Кузница
ShopScythe = Косарь   # 新增
```

## 扩展：更多商店类型

按照相同模式，可以为其他商店类型添加配置：

```csharp
// MarkerStyle.cs
public static MarkerConfig ShopBow;
public static MarkerConfig ShopHammer;

// PayableShopMapper.cs
case PayableShop.ShopType.Bow:
    view.TryAddMapMarker(component, MarkerStyle.ShopBow.Color, MarkerStyle.ShopBow.Sign, Strings.ShopBow, comp => comp.Cast<PayableShop>().GetItemCount());
    break;
case PayableShop.ShopType.Hammer:
    view.TryAddMapMarker(component, MarkerStyle.ShopHammer.Color, MarkerStyle.ShopHammer.Sign, Strings.ShopHammer, comp => comp.Cast<PayableShop>().GetItemCount());
    break;
```

## 完整代码参考

### PayableShopMapper.cs 完整示例

```csharp
using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableShopMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Shop;

        public void Map(Component component)
        {
            var obj = component.Cast<PayableShop>();
            var shopTag = obj.GetComponent<ShopTag>();
            if (shopTag == null) return;
            
            var shopType = shopTag.type;
            switch (shopType)
            {
                case PayableShop.ShopType.Forge:
                    view.TryAddMapMarker(
                        component, 
                        MarkerStyle.ShopForge.Color, 
                        MarkerStyle.ShopForge.Sign, 
                        Strings.ShopForge, 
                        comp => comp.Cast<PayableShop>().GetItemCount()
                    );
                    break;
                    
                case PayableShop.ShopType.Scythe:
                    view.TryAddMapMarker(
                        component, 
                        MarkerStyle.ShopScythe.Color, 
                        MarkerStyle.ShopScythe.Sign, 
                        Strings.ShopScythe, 
                        comp => comp.Cast<PayableShop>().GetItemCount()
                    );
                    break;
                    
                case PayableShop.ShopType.Bow:
                    view.TryAddMapMarker(
                        component, 
                        MarkerStyle.ShopBow.Color, 
                        MarkerStyle.ShopBow.Sign, 
                        Strings.ShopBow, 
                        comp => comp.Cast<PayableShop>().GetItemCount()
                    );
                    break;
                    
                case PayableShop.ShopType.Hammer:
                    view.TryAddMapMarker(
                        component, 
                        MarkerStyle.ShopHammer.Color, 
                        MarkerStyle.ShopHammer.Sign, 
                        Strings.ShopHammer, 
                        comp => comp.Cast<PayableShop>().GetItemCount()
                    );
                    break;
            }
        }
    }
}
```

## 检查清单

- [ ] PayableShopResolver 中包含新商店类型
- [ ] MarkerStyle.cs 中添加 ShopXxx 字段
- [ ] MarkerStyle.cs 中添加配置绑定
- [ ] Strings.cs 中添加字符串字段（可选但推荐）
- [ ] Strings.cs 中添加配置绑定
- [ ] PayableShopMapper.cs 中添加 case
- [ ] MarkerStyle.cfg 中添加配置节
- [ ] Language_*.cfg 中添加多语言字符串
