using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
        }

        [HarmonyPatch(typeof(Payable), nameof(Payable.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Payable __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Payable), nameof(Payable.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Payable __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}