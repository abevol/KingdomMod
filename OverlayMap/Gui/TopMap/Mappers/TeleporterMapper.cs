using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class TeleporterMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(
                component,
                MarkerStyle.Teleporter.Color,
                MarkerStyle.Teleporter.Sign,
                Strings.Teleporter
            );
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}
