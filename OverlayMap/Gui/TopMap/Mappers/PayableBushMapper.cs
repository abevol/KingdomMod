using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableBushMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.PayableBush;

        public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
        {
            if (notifierType != NotifierType.Payable)
                return;

            view.TryAddMapMarker(component, MarkerStyle.BerryBush.Color,
                component.Cast<PayableBush>().paid ? MarkerStyle.BerryBushPaid.Sign : MarkerStyle.BerryBush.Sign, null, null,
                comp => comp.Cast<PayableBush>().paid ? MarkerStyle.BerryBushPaid.Color : MarkerStyle.BerryBush.Color);
        }
    }
}
