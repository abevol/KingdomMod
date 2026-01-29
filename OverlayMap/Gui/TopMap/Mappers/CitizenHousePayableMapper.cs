using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CitizenHousePayableMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.CitizenHouse.Building.Color, MarkerStyle.CitizenHouse.Sign, Strings.CitizenHouse,
                comp => comp.Cast<CitizenHousePayable>()._numberOfAvailableCitizens,
                comp => comp.gameObject.activeSelf ? MarkerStyle.CitizenHouse.Color : MarkerStyle.CitizenHouse.Building.Color);
        }

        [HarmonyPatch(typeof(CitizenHousePayable), nameof(CitizenHousePayable.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(CitizenHousePayable __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(CitizenHousePayable), nameof(CitizenHousePayable.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(CitizenHousePayable __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}