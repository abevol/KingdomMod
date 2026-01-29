using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeachMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Beach.Color, MarkerStyle.Beach.Sign, Strings.Beach);
        }

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