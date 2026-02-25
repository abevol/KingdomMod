using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class DogSpawnMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.DogSpawn;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.DogSpawn.Color, MarkerStyle.DogSpawn.Sign, Strings.DogSpawn, null, null,
                comp => !comp.Cast<DogSpawn>()._dogFreed);
        }

        [HarmonyPatch(typeof(DogSpawn), nameof(DogSpawn.Start))]
        private class OnEnablePatch
        {
            public static void Postfix(DogSpawn __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(DogSpawn), nameof(DogSpawn.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(DogSpawn __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}