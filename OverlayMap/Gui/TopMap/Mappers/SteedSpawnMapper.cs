using System.Collections.Generic;
using System.Linq;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class SteedSpawnMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var obj = (SteedSpawn)component;
            var steed = obj.steeds.FirstOrDefault();
            if (steed == null) return;
            if (!Strings.SteedNames.TryGetValue(steed.steedType, out var steedName))
                return;

            view.TryAddMapMarker(component, MarkerStyle.SteedSpawns.Color, MarkerStyle.SteedSpawns.Sign, steedName,
                comp => ((SteedSpawn)comp).Price, null,
                comp => !((SteedSpawn)comp).HasSpawned);
        }
    }
}