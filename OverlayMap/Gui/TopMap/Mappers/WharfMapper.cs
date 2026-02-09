using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 船坞标记映射器。
    /// </summary>
    public class WharfMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Wharf;

        public void Map(Component component)
        {
            var wharf = component.Cast<Wharf>();
            if (wharf == null) return;

            // 船坞使用蓝色，标识为空
            view.TryAddMapMarker(component, MarkerStyle.Wharf.Color, MarkerStyle.Wharf.Sign, Strings.Wharf,
                comp =>
                {
                    var p = comp.Cast<Wharf>();
                    if (p._availableParts.Count <= 0 && p.WorkAvailable()) return 0;
                    if (p._availableParts.Count <= 0 && !p._boatLaunched) return p.Price;
                    return p._availableParts.Count * p.Price;
                },
                comp =>
                {
                    var p = comp.Cast<Wharf>();
                    if (p.WorkAvailable()) return MarkerStyle.Wharf.Building.Color;
                    if (p._availableParts.Count > 0) return MarkerStyle.Wharf.Building.Color;
                    return MarkerStyle.Wharf.Color;
                },
                comp =>
                {
                    var p = comp.Cast<Wharf>();
                    if (p._boatLaunched) return false;
                    return true;
                });
        }

        /// <summary>
        /// Wharf.OnEnable 补丁。
        /// Wharf 重写了 OnEnable，因此需要单独 Patch。
        /// </summary>
        [HarmonyPatch(typeof(Wharf), nameof(Wharf.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Wharf __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        /// <summary>
        /// Wharf.OnDisable 补丁。
        /// Wharf 重写了 OnDisable，因此需要单独 Patch。
        /// </summary>
        [HarmonyPatch(typeof(Wharf), nameof(Wharf.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Wharf __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
