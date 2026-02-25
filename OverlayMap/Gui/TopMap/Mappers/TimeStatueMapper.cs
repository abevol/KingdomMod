using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class TimeStatueMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.TimeStatue;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.StatueTime.Color, MarkerStyle.StatueTime.Sign, Strings.StatueTime,
                comp => comp.Cast<TimeStatue>()._daysRemaining);
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}