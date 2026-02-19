using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BoatSummoningBellMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.BoatSummoningBell;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.SummonBell.Color, MarkerStyle.SummonBell.Sign, Strings.SummonBell);
        }

        [HarmonyPatch(typeof(BoatSummoningBell), nameof(BoatSummoningBell.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(BoatSummoningBell __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        // [HarmonyPatch(typeof(BoatSummoningBell), nameof(BoatSummoningBell.OnDestroy))]
        // private class OnDisablePatch
        // {
        //     public static void Prefix(BoatSummoningBell __instance)
        //     {
        //         OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        //     }
        // }
    }
}