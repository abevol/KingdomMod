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
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(WorldEatingSerpentWeakPoint))]
    public static class WorldEatingSerpentWeakPointNotifier
    {
        [HarmonyPatch(nameof(WorldEatingSerpentWeakPoint.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(WorldEatingSerpentWeakPoint __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(WorldEatingSerpentWeakPoint.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(WorldEatingSerpentWeakPoint __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}

