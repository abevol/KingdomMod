using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeachMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Beach;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Beach.Color, MarkerStyle.Beach.Sign, Strings.Beach);
        }

        // 该 Patch 只服务于该组件的映射，根据“相关性聚合”原则放在这里没问题。

        [HarmonyPatch(typeof(Beach), nameof(Beach.Start))]
        private class StartPatch
        {
            public static void Postfix(Beach __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Beach), nameof(Beach.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Beach __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}