using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 幽魂标记映射器。
    /// </summary>
    public class HermesShadeMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.HermesShade;

        public void Map(Component component)
        {
            var shade = component.Cast<HermesShade>();
            if (shade == null) return;

            // 如果已被收集，不显示标记
            if (shade._collected) return;
            if (!shade._shade.enabled) return;
            if (!shade.gameObject.activeSelf) return;

            view.TryAddMapMarker(component, MarkerStyle.HermesShade.Color, MarkerStyle.HermesShade.Sign, Strings.HermesShade);
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(HermesShade))]
    public static class HermesShadeNotifier
    {
        [HarmonyPatch(nameof(HermesShade.Start))]
        [HarmonyPostfix]
        public static void Start(HermesShade __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(HermesShade.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(HermesShade __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }

        [HarmonyPatch(nameof(HermesShade.OnDestroy))]
        [HarmonyPrefix]
        public static void OnDestroy(HermesShade __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }

        [HarmonyPatch(nameof(HermesShade.OnShadeCollectedHandler))]
        [HarmonyPrefix]
        public static void OnShadeCollectedHandler(HermesShade __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}

