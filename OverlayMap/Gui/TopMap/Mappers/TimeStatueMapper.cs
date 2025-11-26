using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class TimeStatueMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.StatueTime.Color, MarkerStyle.StatueTime.Sign, Strings.StatueTime,
                comp => comp.Cast<TimeStatue>()._daysRemaining);
        }
    }
}