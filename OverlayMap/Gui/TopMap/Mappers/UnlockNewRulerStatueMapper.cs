using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class UnlockNewRulerStatueMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.UnlockNewRulerStatue;

        public void Map(Component component)
        {
            var obj = component.Cast<UnlockNewRulerStatue>();
            Strings.MonarchNames.TryGetValue(obj.rulerToUnlock, out var monarchName);
            view.TryAddMapMarker(component, null, MarkerStyle.RulerSpawns.Sign, monarchName,
                comp => comp.Cast<UnlockNewRulerStatue>().Price,
                comp => comp.Cast<UnlockNewRulerStatue>().status switch
                {
                    UnlockNewRulerStatue.Status.Locked => MarkerStyle.RulerSpawns.Locked.Color,
                    UnlockNewRulerStatue.Status.WaitingForArcher => MarkerStyle.RulerSpawns.Building.Color,
                    _ => MarkerStyle.RulerSpawns.Unlocked.Color
                });
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}