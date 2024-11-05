using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CampfireMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component, HashSet<ObjectPatcher.SourceFlag> sources)
        {
            view.TryAddMapMarker(component, sources, MarkerStyle.Campfire.Sign, Strings.Campfire, _ => MarkerStyle.Campfire.Color);
        }
    }
}