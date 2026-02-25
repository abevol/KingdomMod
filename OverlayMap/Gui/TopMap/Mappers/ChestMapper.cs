using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class ChestMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Chest;

        public void Map(Component component)
        {
            var obj = component.Cast<Chest>();
            var isGem = obj.currencyType == CurrencyType.Gems;
            view.TryAddMapMarker(component,
                isGem ? MarkerStyle.GemChest.Color : MarkerStyle.Chest.Color,
                isGem ? MarkerStyle.GemChest.Sign : MarkerStyle.Chest.Sign,
                isGem ? Strings.GemChest : Strings.Chest,
                comp => comp.Cast<Chest>().currencyAmount,
                null, comp => comp.Cast<Chest>().currencyAmount != 0);
        }

        [HarmonyPatch(typeof(Chest), nameof(Chest.Awake))]
        private class OnEnablePatch
        {
            public static void Postfix(Chest __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Chest), nameof(Chest.OnDestroy))]
        private class OnDisablePatch
        {
            public static void Prefix(Chest __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}