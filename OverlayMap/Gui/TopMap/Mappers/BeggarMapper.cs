using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeggarMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component, HashSet<ObjectPatcher.SourceFlag> sources)
        {
            view.TryAddMapMarker(component, sources, MarkerStyle.Beggar.Sign, Strings.Beggar,
                _ => MarkerStyle.Beggar.Color, null,
                comp => ((Beggar)comp).hasFoundBaker,
                MarkerRow.Movable);
        }
    }
}