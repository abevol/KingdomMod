using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeggarCampMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.BeggarCamp;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.BeggarCamp.Color, MarkerStyle.BeggarCamp.Sign, Strings.BeggarCamp,
                comp =>
                {
                    var camp = comp.Cast<BeggarCamp>();
                    if (camp == null || camp._beggars == null) return 0;

                    int count = 0;
                    foreach (var beggar in camp._beggars)
                    {
                        if (beggar != null && beggar.isActiveAndEnabled)
                            count++;
                    }

                    return count;
                });
        }

        [HarmonyPatch(typeof(BeggarCamp), nameof(BeggarCamp.Start))]
        private class StartPatch
        {
            public static void Postfix(BeggarCamp __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(BeggarCamp), nameof(BeggarCamp.OnDestroy))]
        private class OnDisablePatch
        {
            public static void Prefix(BeggarCamp __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}