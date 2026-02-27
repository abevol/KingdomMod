using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class MerchantSpawnerMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.MerchantSpawner;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.MerchantHouse.Color, MarkerStyle.MerchantHouse.Sign, Strings.MerchantHouse);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(MerchantSpawner))]
    public static class MerchantSpawnerNotifier
    {
        [HarmonyPatch(nameof(MerchantSpawner.Start))]
        [HarmonyPostfix]
        public static void Start(MerchantSpawner __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(MerchantSpawner.OnDestroy))]
        [HarmonyPrefix]
        public static void OnDestroy(MerchantSpawner __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
