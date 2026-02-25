using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableShopMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.PayableShop;

        public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
        {
            if (notifierType != NotifierType.Payable)
                return;

            var shopType = component.GetComponent<ShopTag>().type;
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
    }
}
