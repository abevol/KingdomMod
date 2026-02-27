using HarmonyLib;
using UnityEngine;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BoatSummoningBellMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.BoatSummoningBell;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.SummonBell.Color, MarkerStyle.SummonBell.Sign, Strings.SummonBell);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(BoatSummoningBell))]
    public static class BoatSummoningBellNotifier
    {
        [HarmonyPatch(nameof(BoatSummoningBell.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(BoatSummoningBell __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }
    }
}
