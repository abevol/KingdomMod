using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 幽魂标记映射器。
    /// </summary>
    public class HermesShadeMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.HermesShade;

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

        [HarmonyPatch(typeof(HermesShade), nameof(HermesShade.Start))]
        private class StartPatch
        {
            public static void Postfix(HermesShade __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(HermesShade), nameof(HermesShade.OnDestroy))]
        private class OnDestroyPatch
        {
            public static void Prefix(HermesShade __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }

        [HarmonyPatch(typeof(HermesShade), nameof(HermesShade.OnShadeCollectedHandler))]
        private class OnShadeCollectedHandlerPatch
        {
            public static void Prefix(HermesShade __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
