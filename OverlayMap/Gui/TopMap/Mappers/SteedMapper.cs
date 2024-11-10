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
            var obj = (Steed)component;
            Strings.SteedNames.TryGetValue(obj.steedType, out var steedName);
            view.TryAddMapMarker(component, MarkerStyle.Steeds.Color, MarkerStyle.Steeds.Sign, steedName,
                comp => ((Steed)comp).Price, null,
                comp => ((Steed)comp).CurrentMode != SteedMode.Player);
        }
    }
}