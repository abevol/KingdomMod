using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableBushMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.BerryBush.Color,
                component.Cast<PayableBush>().paid ? MarkerStyle.BerryBushPaid.Sign : MarkerStyle.BerryBush.Sign, null, null,
                comp => comp.Cast<PayableBush>().paid ? MarkerStyle.BerryBushPaid.Color : MarkerStyle.BerryBush.Color);
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}