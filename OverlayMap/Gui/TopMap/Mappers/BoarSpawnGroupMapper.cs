using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BoarSpawnGroupMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.BoarSpawn.Color, MarkerStyle.BoarSpawn.Sign, Strings.BoarSpawn,
                comp => comp.Cast<BoarSpawnGroup>()._spawnedBoar ? 0 : 1);
        }
    }
}