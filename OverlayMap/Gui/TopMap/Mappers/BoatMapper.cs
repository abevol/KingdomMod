using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BoatMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Boat.Color, MarkerStyle.Boat.Sign, Strings.Boat, null, null,
                comp => comp.gameObject.activeSelf);
        }
    }
}