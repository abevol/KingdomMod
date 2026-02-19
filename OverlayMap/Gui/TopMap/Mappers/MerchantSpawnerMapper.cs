using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class MerchantSpawnerMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.MerchantSpawner;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.MerchantHouse.Color, MarkerStyle.MerchantHouse.Sign, Strings.MerchantHouse);
        }

        [HarmonyPatch(typeof(MerchantSpawner), nameof(MerchantSpawner.Start))]
        private class StartPatch
        {
            public static void Postfix(MerchantSpawner __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(MerchantSpawner), nameof(MerchantSpawner.OnDestroy))]
        private class OnDestroyPatch
        {
            public static void Prefix(MerchantSpawner __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}