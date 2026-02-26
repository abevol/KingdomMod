using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class TeleporterMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Teleporter;

        public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
        {
            if (notifierType == NotifierType.Payable)
                view.TryAddMapMarker(component, MarkerStyle.Teleporter.Color, MarkerStyle.Teleporter.Sign, Strings.Teleporter);
            else if (resolverType == ResolverType.Scaffolding)
                view.TryAddMapMarker(component, MarkerStyle.Teleporter.Building.Color, MarkerStyle.Teleporter.Sign, Strings.Teleporter);
        }
    }
}
