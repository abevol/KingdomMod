using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class RiverMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.River;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.River.Color, MarkerStyle.River.Sign, null);
        }

        [HarmonyPatch(typeof(River), nameof(River.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(River __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(River), nameof(River.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(River __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}