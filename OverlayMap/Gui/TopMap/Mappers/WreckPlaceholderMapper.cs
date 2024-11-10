using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class WreckPlaceholderMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Boat.Wrecked.Color, MarkerStyle.Boat.Sign, Strings.BoatWreck, null, null,
                comp => comp.gameObject.activeSelf);
        }
    }
}