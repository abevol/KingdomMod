using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
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
            }
        }
    }
}