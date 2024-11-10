using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class DeerMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Deer.Color, MarkerStyle.Deer.Sign, Strings.Deer, null,
                comp =>
            {
                var state = ((Deer)comp)._fsm.Current;
                return state == 5 ? MarkerStyle.DeerFollowing.Color : MarkerStyle.Deer.Color;
            }, comp => comp.gameObject.activeSelf && !((Deer)comp)._damageable.isDead, MarkerRow.Movable);
        }
    }
}