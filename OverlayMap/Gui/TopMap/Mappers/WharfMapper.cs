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
        public MapMarkerType MarkerType => MapMarkerType.Wharf;

        public void Map(Component component)
        {
            var wharf = component.Cast<Wharf>();
            if (wharf == null) return;

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
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Wharf))]
    public static class WharfNotifier
    {
        [HarmonyPatch(nameof(Wharf.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(Wharf __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Wharf.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Wharf __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
