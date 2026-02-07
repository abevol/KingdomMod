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
                case PayableShop.ShopType.Scythe:
                    view.TryAddMapMarker(component, MarkerStyle.ShopScythe.Color, MarkerStyle.ShopScythe.Sign, Strings.ShopScythe, comp => comp.Cast<PayableShop>().GetItemCount());
                    break;
            }
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}