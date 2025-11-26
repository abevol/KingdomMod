using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class DogSpawnMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.DogSpawn.Color, MarkerStyle.DogSpawn.Sign, Strings.DogSpawn, null, null,
                comp => !comp.Cast<DogSpawn>()._dogFreed);
        }
    }
}