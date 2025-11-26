using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableBushMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.BerryBush.Color,
                component.Cast<PayableBush>().paid ? MarkerStyle.BerryBushPaid.Sign : MarkerStyle.BerryBush.Sign, null, null,
                comp => comp.Cast<PayableBush>().paid ? MarkerStyle.BerryBushPaid.Color : MarkerStyle.BerryBush.Color);
        }
    }
}