using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class RiverMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.River;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.River.Color, MarkerStyle.River.Sign, null);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(River))]
    public static class RiverNotifier
    {
        [HarmonyPatch(nameof(River.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(River __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(River.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(River __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
