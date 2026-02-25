using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PersephoneCageMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.PersephoneCage;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, null, MarkerStyle.PersephoneCage.Sign, Strings.HermitPersephone, null,
                comp => PersephoneCage.State.IsPersephoneLocked(comp.Cast<PersephoneCage>()._fsm.Current)
                    ? MarkerStyle.PersephoneCage.Locked.Color
                    : MarkerStyle.PersephoneCage.Unlocked.Color);
        }

        [HarmonyPatch(typeof(PersephoneCage), nameof(PersephoneCage.Awake))]
        private class AwakePatch
        {
            public static void Postfix(PersephoneCage __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(PersephoneCage), nameof(PersephoneCage.OnDestroy))]
        private class OnDestroyPatch
        {
            public static void Prefix(PersephoneCage __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}