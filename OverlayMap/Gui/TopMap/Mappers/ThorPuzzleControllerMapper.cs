using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class ThorPuzzleControllerMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, null, MarkerStyle.ThorPuzzleStatue.Sign, Strings.ThorPuzzleStatue, null,
                comp => comp.Cast<ThorPuzzleController>().State == 0 ? MarkerStyle.ThorPuzzleStatue.Locked.Color : MarkerStyle.ThorPuzzleStatue.Unlocked.Color);
        }
    }
}