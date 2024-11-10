using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class MerchantSpawnerMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.MerchantHouse.Color, MarkerStyle.MerchantHouse.Sign, Strings.MerchantHouse);
        }
    }
}