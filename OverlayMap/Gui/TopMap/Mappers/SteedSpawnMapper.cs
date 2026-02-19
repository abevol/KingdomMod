using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class SteedSpawnMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.SteedSpawn;

        public void Map(Component component)
        {
            var obj = component.Cast<SteedSpawn>();
            var steed = obj.steeds.FirstOrDefault();
            if (steed == null) return;
            if (!Strings.SteedNames.TryGetValue(steed.steedType, out var steedName))
                return;

            view.TryAddMapMarker(component, MarkerStyle.SteedSpawns.Color, MarkerStyle.SteedSpawns.Sign, steedName,
                comp => comp.Cast<SteedSpawn>().Price, null,
                comp => !comp.Cast<SteedSpawn>().HasSpawned);
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}