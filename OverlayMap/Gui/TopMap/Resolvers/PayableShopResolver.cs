using System;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    /// <summary>
    /// PayableShop 组件解析器。
    /// 通过 ShopTag.type 区分不同的商店类型。
    /// 注意：目前游戏中 Shop 类型较少，大部分由 PayableUpgrade 处理。
    /// </summary>
    public class PayableShopResolver : IMarkerResolver
    {
        public Type TargetComponentType => typeof(PayableShop);

        public MapMarkerType? Resolve(Component component)
        {
            var shop = component.Cast<PayableShop>();
            if (shop == null) return null;

            var shopTag = shop.GetComponent<ShopTag>();
            if (shopTag == null) return null;

            // 根据商店类型返回对应的标记类型
            return shopTag.type switch
            {
                PayableShop.ShopType.Forge => MapMarkerType.Shop,
                PayableShop.ShopType.Bow => MapMarkerType.Shop,
                PayableShop.ShopType.Hammer => MapMarkerType.Shop,
                PayableShop.ShopType.Scythe => MapMarkerType.Shop,
                _ => MapMarkerType.Shop  // 其他商店类型统一为 Shop
            };
        }
    }
}