using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CampfireMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Campfire.Color, MarkerStyle.Campfire.Sign, Strings.Campfire);
        }

        [HarmonyPatch(typeof(Campfire), nameof(Campfire.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Campfire __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Campfire), nameof(Campfire.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Campfire __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}