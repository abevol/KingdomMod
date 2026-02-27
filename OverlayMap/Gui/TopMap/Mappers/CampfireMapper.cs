using HarmonyLib;
using UnityEngine;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CampfireMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Campfire;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Campfire.Color, MarkerStyle.Campfire.Sign, Strings.Campfire);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Campfire))]
    public static class CampfireNotifier
    {
        [HarmonyPatch(nameof(Campfire.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(Campfire __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Campfire.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Campfire __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
