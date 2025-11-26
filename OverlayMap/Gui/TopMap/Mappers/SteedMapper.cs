using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class SteedMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var obj = component.Cast<Steed>();
            Strings.SteedNames.TryGetValue(obj.steedType, out var steedName);
            view.TryAddMapMarker(component, MarkerStyle.Steeds.Color, MarkerStyle.Steeds.Sign, steedName,
                comp => comp.Cast<Steed>().Price, null,
                comp => comp.Cast<Steed>().CurrentMode != SteedMode.Player);
        }
    }
}