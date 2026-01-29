using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BombMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Bomb.Color, MarkerStyle.Bomb.Sign, Strings.Bomb, null, null, null, MarkerRow.Movable);
        }

        [HarmonyPatch(typeof(Bomb), nameof(Bomb.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Bomb __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Bomb), nameof(Bomb.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Bomb __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}