using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using System.Collections.Generic;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class SteedMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var obj = component.Cast<Steed>();
            LogInfo($"Mapping Steed: {obj}, x: {obj.transform.position.x}, {obj.steedType}, enabled: {obj.enabled}, {obj.isActiveAndEnabled}, CurrentMode: {obj.CurrentMode}");
            Strings.SteedNames.TryGetValue(obj.steedType, out var steedName);
            view.TryAddMapMarker(component, MarkerStyle.Steeds.Color, MarkerStyle.Steeds.Sign, steedName,
                comp => comp.Cast<Steed>().Price, null,
                comp => comp.Cast<Steed>().CurrentMode != SteedMode.Player);
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}