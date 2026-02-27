using HarmonyLib;
using UnityEngine;
using KingdomMod.SharedLib;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BoarSpawnGroupMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.BoarSpawnGroup;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.BoarSpawn.Color, MarkerStyle.BoarSpawn.Sign, Strings.BoarSpawn,
                comp => comp.Cast<BoarSpawnGroup>()._spawnedBoar ? 0 : 1);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(BoarSpawnGroup))]
    public static class BoarSpawnGroupNotifier
    {
        [HarmonyPatch(nameof(BoarSpawnGroup.Awake))]
        [HarmonyPostfix]
        public static void Awake(BoarSpawnGroup __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(BoarSpawnGroup.OnDestroy))]
        [HarmonyPrefix]
        public static void OnDestroy(BoarSpawnGroup __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
