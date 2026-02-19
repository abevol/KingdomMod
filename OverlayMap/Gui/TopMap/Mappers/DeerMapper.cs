using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using KingdomMod.SharedLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class DeerMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Deer;

        public Component[] GetComponents()
        {
            return GameExtensions.FindObjectsWithTagOfType<Deer>(Tags.Wildlife).Cast<Component>().ToArray();
        }

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Deer.Color, MarkerStyle.Deer.Sign, Strings.Deer, null,
                comp =>
            {
                var state = comp.Cast<Deer>()._fsm.Current;
                return state == 5 ? MarkerStyle.DeerFollowing.Color : MarkerStyle.Deer.Color;
            }, comp => comp.gameObject.activeSelf && !comp.Cast<Deer>()._damageable.isDead, MarkerRow.Movable);
        }

        [HarmonyPatch(typeof(Deer), nameof(Deer.Start))]
        private class StartPatch
        {
            public static void Postfix(Deer __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Deer), nameof(Deer.OnDestroy))]
        private class OnDestroyPatch
        {
            public static void Prefix(Deer __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}