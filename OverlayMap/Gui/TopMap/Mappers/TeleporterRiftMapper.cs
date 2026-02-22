using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 传送阵裂隙标记映射器。
    /// 使用 OnEnable/OnDisable Patch 作为组件创建和销毁的事件通知。
    /// </summary>
    public class TeleporterRiftMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.TeleporterRift;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.TeleporterRift.Color, MarkerStyle.TeleporterRift.Sign, Strings.TeleporterRift);
        }

        /// <summary>
        /// TeleporterRift.OnEnable 补丁。
        /// 当组件启用时通知 TopMapView 创建标记。
        /// </summary>
        [HarmonyPatch(typeof(TeleporterRift), nameof(TeleporterRift.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(TeleporterRift __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        /// <summary>
        /// TeleporterRift.OnDisable 补丁。
        /// 当组件禁用时通知 TopMapView 销毁标记。
        /// </summary>
        [HarmonyPatch(typeof(TeleporterRift), nameof(TeleporterRift.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(TeleporterRift __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
