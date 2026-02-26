using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BoatMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Boat;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Boat.Color, MarkerStyle.Boat.Sign, Strings.Boat, null, null,
                comp => comp.gameObject.activeSelf);
        }

        [HarmonyPatch(typeof(Boat), nameof(Boat.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Boat __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Boat), nameof(Boat.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Boat __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}