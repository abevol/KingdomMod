using HarmonyLib;
using UnityEngine;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class FarmhouseMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Farmhouse;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Farmhouse.Color, MarkerStyle.Farmhouse.Sign, Strings.Farmhouse);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Farmhouse))]
    public static class FarmhouseNotifier
    {
        [HarmonyPatch(nameof(Farmhouse.Start))]
        [HarmonyPostfix]
        public static void Start(Farmhouse __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Farmhouse.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Farmhouse __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
