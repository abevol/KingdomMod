using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class HelPuzzleControllerMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, null, MarkerStyle.HelPuzzleStatue.Sign, Strings.HelPuzzleStatue, null,
                comp => comp.Cast<HelPuzzleController>().State == 0 ? MarkerStyle.HelPuzzleStatue.Locked.Color : MarkerStyle.HelPuzzleStatue.Unlocked.Color);
        }
    }
}