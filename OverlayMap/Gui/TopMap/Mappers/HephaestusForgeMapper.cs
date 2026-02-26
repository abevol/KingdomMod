using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class HephaestusForgeMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.HephaestusForge;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.HephaestusForge.Color, MarkerStyle.HephaestusForge.Sign, Strings.HephaestusForge);
        }
    }
}