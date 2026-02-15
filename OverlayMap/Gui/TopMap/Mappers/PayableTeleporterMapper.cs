using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableTeleporterMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(
                component,
                MarkerStyle.Teleporter.Color,
                MarkerStyle.Teleporter.Sign,
                Strings.Teleporter
            );
        }

        [HarmonyPatch(typeof(PayableTeleporter), nameof(PayableTeleporter.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(PayableTeleporter __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(PayableTeleporter), nameof(PayableTeleporter.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(PayableTeleporter __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
