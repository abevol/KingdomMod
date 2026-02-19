using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableGemChestMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.GemChest;

        public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
        {
            if (notifierType != NotifierType.Payable)
                return;

            view.TryAddMapMarker(component, MarkerStyle.GemMerchant.Color, MarkerStyle.GemMerchant.Sign, Strings.GemMerchant,
                comp =>
                {
                    var payableGemChest = comp.Cast<PayableGemChest>();
                    return payableGemChest.infiniteGems ? payableGemChest.guardRef.Price : payableGemChest.gemsStored;
                });
        }
    }
}