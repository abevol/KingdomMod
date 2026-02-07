using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class KnightMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Knight;

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Knight.Color, MarkerStyle.Knight.Sign, Strings.Knight, null,
                null,
                comp =>
                {
                    var knight = comp.TryCast<Knight>();
                    if (knight == null) return false;
                    return knight._fsm.Current == Knight.State.Charge || knight._fsm.Current == Knight.State.GoToWall;
                }, MarkerRow.Movable
            );
        }

        [HarmonyPatch(typeof(Knight), nameof(Knight.Awake))]
        private class AwakePatch
        {
            public static void Postfix(Knight __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Knight), nameof(Knight.OnDestroy))]
        private class OnDestroyPatch
        {
            public static void Prefix(Knight __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
