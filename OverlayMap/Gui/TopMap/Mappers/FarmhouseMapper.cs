using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class FarmhouseMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Farmhouse.Color, MarkerStyle.Farmhouse.Sign, Strings.Farmhouse);
        }

        [HarmonyPatch(typeof(Farmhouse), nameof(Farmhouse.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Farmhouse __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Farmhouse), nameof(Farmhouse.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Farmhouse __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}