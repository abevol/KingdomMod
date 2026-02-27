using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class KnightMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Knight;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Knight.Color, MarkerStyle.Knight.Sign, Strings.Knight, null,
                null,
                comp =>
                {
                    var knight = comp.TryCast<Knight>();
                    if (knight == null) return false;
                    return knight._fsm.Current == Knight.State.Charge || knight._fsm.Current == Knight.State.GoToWall;
                }, MarkerRow.Movable
            );
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Knight))]
    public static class KnightNotifier
    {
        [HarmonyPatch(nameof(Knight.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(Knight __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Knight.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Knight __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}

