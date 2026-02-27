using HarmonyLib;
using UnityEngine;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BoatMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Boat;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Boat.Color, MarkerStyle.Boat.Sign, Strings.Boat, null, null,
                comp => comp.gameObject.activeSelf);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Boat))]
    public static class BoatNotifier
    {
        [HarmonyPatch(nameof(Boat.Start))]
        [HarmonyPostfix]
        public static void Start(Boat __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Boat.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Boat __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
