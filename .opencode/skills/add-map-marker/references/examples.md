# 完整示例

## 示例 1: 复用现有类型（商店标记）

为新的商店类型（如 Bow）添加独立配置，复用 `MapMarkerType.PayableShop`。

### MarkerStyle.cs

```csharp
// 字段声明
public static MarkerConfig ShopBow;

// ConfigBind 方法中（按字母顺序插入）
ShopBow.Color = config.Bind("ShopBow", "Color", "1,1,1,1", "");
ShopBow.Sign = config.Bind("ShopBow", "Sign", "", "");
```

### Strings.cs

```csharp
// 字段声明
public static ConfigEntryWrapper<string> ShopBow;

// ConfigBind 方法中
ShopBow = config.Bind("Strings", "ShopBow", "Bow", "");
```

### PayableShopMapper.cs

```csharp
public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
{
    if (notifierType != NotifierType.Payable) return;
    
    var shopType = component.GetComponent<ShopTag>().type;
    switch (shopType)
    {
        case PayableShop.ShopType.Forge:
            view.TryAddMapMarker(component, MarkerStyle.ShopForge.Color, MarkerStyle.ShopForge.Sign, Strings.ShopForge, 
                comp => comp.Cast<PayableShop>().GetItemCount());
            break;
        case PayableShop.ShopType.Bow:  // 新增
            view.TryAddMapMarker(component, MarkerStyle.ShopBow.Color, MarkerStyle.ShopBow.Sign, Strings.ShopBow,
                comp => comp.Cast<PayableShop>().GetItemCount());
            break;
    }
}
```

### MarkerStyle.cfg

```ini
[ShopBow]

# Setting type: String
# Default value: 1,1,1,1
Color = 1,1,1,1

# Setting type: String
# Default value: 
Sign = 
```

---

## 示例 2: 创建新类型（带 OnEnable/OnDisable Patch）

为 `Wharf`（船坞）添加新标记类型。Wharf 继承自 Payable 并重写了 OnEnable/OnDisable。

### MarkerStyle.cs

```csharp
// 字段声明
public static MarkerConfigStated Wharf;

// ConfigBind 方法中
Wharf.Sign = config.Bind("Wharf", "Sign", "", "");
Wharf.Color = config.Bind("Wharf", "Color", "0,1,0,1", "");
Wharf.Building.Color = config.Bind("Wharf.Building", "Color", "0,0,1,1", "");
```

### WharfMapper.cs

```csharp
using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class WharfMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Wharf;

        public void Map(Component component)
        {
            var wharf = component.Cast<Wharf>();
            if (wharf == null) return;

            view.TryAddMapMarker(
                component, 
                MarkerStyle.Wharf.Color, 
                MarkerStyle.Wharf.Sign, 
                Strings.Wharf,
                comp => GetCount(comp.Cast<Wharf>()),
                comp => GetColor(comp.Cast<Wharf>())
            );
        }

        private int GetCount(Wharf p)
        {
            if (p._availableParts.Count <= 0 && p.WorkAvailable()) return 0;
            if (p._availableParts.Count <= 0 && !p._boatLaunched) return p.Price;
            return p._availableParts.Count * p.Price;
        }

        private ConfigEntryWrapper<string> GetColor(Wharf p)
        {
            if (p.WorkAvailable() || p._availableParts.Count > 0)
                return MarkerStyle.Wharf.Building.Color;
            return MarkerStyle.Wharf.Color;
        }

        // ⚠️ 必须单独 Patch：Wharf 重写了 OnEnable/OnDisable
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
}
```

---

## 示例 3: 使用 ResolverType 判断状态

通过 ResolverType 区分建造中状态和已建状态。

```csharp
public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
{
    var prefabId = component.gameObject.GetComponent<PrefabID>();
    if (prefabId == null) return;
    
    var gamePrefabId = (GamePrefabID)prefabId.prefabID;
    
    // 建造中状态（通过 Scaffolding Resolver 识别）
    if (resolverType == ResolverType.Scaffolding)
    {
        view.TryAddMapMarker(
            component, 
            MarkerStyle.Lighthouse.Building.Color,  // 蓝色
            MarkerStyle.Lighthouse.Sign, 
            Strings.Lighthouse
        );
        return;
    }
    
    // 已建状态（通过 PayableUpgrade Resolver 识别）
    if (resolverType == ResolverType.PayableUpgrade)
    {
        view.TryAddMapMarker(
            component, 
            MarkerStyle.Lighthouse.Color,
            MarkerStyle.Lighthouse.Sign, 
            Strings.Lighthouse,
            comp => GetCount(comp),
            comp => GetColor(comp)
        );
    }
}
```

---

## 检查清单

### 复用现有类型

- [ ] MarkerStyle.cs 中添加配置字段和绑定
- [ ] Strings.cs 中添加字符串（如需要）
- [ ] Mapper 中添加 case 分支
- [ ] MarkerStyle.cfg 中添加配置节
- [ ] Language 文件中添加多语言

### 创建新类型

- [ ] MapMarkerType.cs 中添加枚举值
- [ ] Resolver 中添加类型识别逻辑
- [ ] MapperInitializer 中注册 Resolver 和 Mapper
- [ ] 创建 Mapper 文件
- [ ] MarkerStyle.cs 中添加配置
- [ ] Strings.cs 中添加字符串
- [ ] **如果组件重写了 OnEnable/OnDisable，添加 Patch**
- [ ] MarkerStyle.cfg 中添加配置节
- [ ] Language 文件中添加多语言
