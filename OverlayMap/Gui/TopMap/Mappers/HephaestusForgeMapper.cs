using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class HephaestusForgeMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.HephaestusForge.Color, MarkerStyle.HephaestusForge.Sign, Strings.HephaestusForge);
        }
    }
}