using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BoarSpawnGroupMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.BoarSpawn.Color, MarkerStyle.BoarSpawn.Sign, Strings.BoarSpawn,
                comp => comp.Cast<BoarSpawnGroup>()._spawnedBoar ? 0 : 1);
        }

        [HarmonyPatch(typeof(BoarSpawnGroup), nameof(BoarSpawnGroup.Awake))]
        private class OnEnablePatch
        {
            public static void Postfix(BoarSpawnGroup __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(BoarSpawnGroup), nameof(BoarSpawnGroup.OnDestroy))]
        private class OnDisablePatch
        {
            public static void Prefix(BoarSpawnGroup __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}