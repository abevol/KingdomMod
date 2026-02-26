using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 弱点标记映射器。
    /// </summary>
    public class WeakPointMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.WeakPoint;

        public void Map(Component component)
        {
            var weakPoint = component.Cast<WorldEatingSerpentWeakPoint>();
            if (weakPoint == null) return;

            view.TryAddMapMarker(component, MarkerStyle.WeakPoint.Color, MarkerStyle.WeakPoint.Sign, Strings.WeakPoint, null, null,
                comp =>
                {
                    var p = comp.Cast<WorldEatingSerpentWeakPoint>();
                    return !p.IsSubmerged;
                });
        }

        /// <summary>
        /// WorldEatingSerpentWeakPoint.OnEnable 补丁。
        /// </summary>
        [HarmonyPatch(typeof(WorldEatingSerpentWeakPoint), nameof(WorldEatingSerpentWeakPoint.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(WorldEatingSerpentWeakPoint __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        /// <summary>
        /// WorldEatingSerpentWeakPoint.OnDisable 补丁。
        /// </summary>
        [HarmonyPatch(typeof(WorldEatingSerpentWeakPoint), nameof(WorldEatingSerpentWeakPoint.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(WorldEatingSerpentWeakPoint __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
