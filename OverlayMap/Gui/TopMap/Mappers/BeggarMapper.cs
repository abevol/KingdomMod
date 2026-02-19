using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeggarMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Beggar;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Beggar.Color, MarkerStyle.Beggar.Sign, Strings.Beggar, null, null,
                comp => comp.Cast<Beggar>().hasFoundBaker, MarkerRow.Movable);
        }

        [HarmonyPatch(typeof(Beggar), nameof(Beggar.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Beggar __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Beggar), nameof(Beggar.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Beggar __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}