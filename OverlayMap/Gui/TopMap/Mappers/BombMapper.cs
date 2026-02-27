using HarmonyLib;
using UnityEngine;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BombMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Bomb;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Bomb.Color, MarkerStyle.Bomb.Sign, Strings.Bomb, null, null, null, MarkerRow.Movable);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Bomb))]
    public static class BombNotifier
    {
        [HarmonyPatch(nameof(Bomb.Start))]
        [HarmonyPostfix]
        public static void Start(Bomb __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Bomb.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Bomb __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
