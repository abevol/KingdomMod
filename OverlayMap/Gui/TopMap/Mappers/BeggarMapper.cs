using HarmonyLib;
using UnityEngine;
using KingdomMod.SharedLib;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeggarMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Beggar;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Beggar.Color, MarkerStyle.Beggar.Sign, Strings.Beggar, null, null,
                comp => comp.Cast<Beggar>().hasFoundBaker, MarkerRow.Movable);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Beggar))]
    public static class BeggarNotifier
    {
        [HarmonyPatch(nameof(Beggar.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(Beggar __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Beggar.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Beggar __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
