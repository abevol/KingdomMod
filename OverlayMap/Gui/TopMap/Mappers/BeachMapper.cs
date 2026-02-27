using HarmonyLib;
using UnityEngine;
using KingdomMod.SharedLib;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeachMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Beach;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Beach.Color, MarkerStyle.Beach.Sign, Strings.Beach);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Beach))]
    public static class BeachNotifier
    {
        [HarmonyPatch(nameof(Beach.Start))]
        [HarmonyPostfix]
        public static void Start(Beach __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Beach.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Beach __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
