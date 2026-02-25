using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class HelPuzzleControllerMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.HelPuzzleController;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, null, MarkerStyle.HelPuzzleStatue.Sign, Strings.HelPuzzleStatue, null,
                comp => comp.GetComponent<HelPuzzleController>().State == 0 ? MarkerStyle.HelPuzzleStatue.Locked.Color : MarkerStyle.HelPuzzleStatue.Unlocked.Color);
        }

        // 已由 PayableBlocker 中的同级组件方法补丁通知组件的启用和禁用事件
    }
}