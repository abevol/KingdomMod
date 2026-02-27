using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class DogSpawnMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.DogSpawn;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.DogSpawn.Color, MarkerStyle.DogSpawn.Sign, Strings.DogSpawn, null, null,
                comp => !comp.Cast<DogSpawn>()._dogFreed);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(DogSpawn))]
    public static class DogSpawnNotifier
    {
        [HarmonyPatch(nameof(DogSpawn.Start))]
        [HarmonyPostfix]
        public static void Start(DogSpawn __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(DogSpawn.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(DogSpawn __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
