using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class UnlockNewRulerStatueMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var obj = (UnlockNewRulerStatue)component;
            Strings.MonarchNames.TryGetValue(obj.rulerToUnlock, out var monarchName);
            view.TryAddMapMarker(component, null, MarkerStyle.RulerSpawns.Sign, monarchName,
                comp => ((UnlockNewRulerStatue)comp).Price,
                comp => ((UnlockNewRulerStatue)comp).status switch
                {
                    UnlockNewRulerStatue.Status.Locked => MarkerStyle.RulerSpawns.Locked.Color,
                    UnlockNewRulerStatue.Status.WaitingForArcher => MarkerStyle.RulerSpawns.Building.Color,
                    _ => MarkerStyle.RulerSpawns.Unlocked.Color
                });
        }
    }
}