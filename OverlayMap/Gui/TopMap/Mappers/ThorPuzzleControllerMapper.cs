using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class ThorPuzzleControllerMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.ThorPuzzleController;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, null, MarkerStyle.ThorPuzzleStatue.Sign, Strings.ThorPuzzleStatue, null,
                comp => comp.Cast<ThorPuzzleController>().State == 0 ? MarkerStyle.ThorPuzzleStatue.Locked.Color : MarkerStyle.ThorPuzzleStatue.Unlocked.Color);
        }

        // 已由 PayableBlocker 中的同级组件方法补丁通知组件的启用和禁用事件
    }
}