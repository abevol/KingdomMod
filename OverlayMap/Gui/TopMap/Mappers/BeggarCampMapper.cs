using HarmonyLib;
using UnityEngine;
using KingdomMod.SharedLib;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeggarCampMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.BeggarCamp;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.BeggarCamp.Color, MarkerStyle.BeggarCamp.Sign, Strings.BeggarCamp,
                comp =>
                {
                    var camp = comp.Cast<BeggarCamp>();
                    if (camp == null || camp._beggars == null) return 0;

                    int count = 0;
                    foreach (var beggar in camp._beggars)
                    {
                        if (beggar != null && beggar.isActiveAndEnabled)
                            count++;
                    }

                    return count;
                });
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(BeggarCamp))]
    public static class BeggarCampNotifier
    {
        [HarmonyPatch(nameof(BeggarCamp.Start))]
        [HarmonyPostfix]
        public static void Start(BeggarCamp __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }
        [HarmonyPatch(nameof(BeggarCamp.OnDestroy))]
        [HarmonyPrefix]
        public static void OnDisable(BeggarCamp __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
