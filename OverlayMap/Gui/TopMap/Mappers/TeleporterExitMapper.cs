using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using System.Collections.Generic;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class TeleporterExitMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var teleExit = component.Cast<TeleporterExit>();
            if (teleExit._player == null) return;

            var playerId = (PlayerId)teleExit._player.playerId;
            var sign = playerId == PlayerId.P1 ? MarkerStyle.TeleExitP1.Sign : MarkerStyle.TeleExitP2.Sign;
            var title = playerId == PlayerId.P1 ? Strings.TeleExitP1 : Strings.TeleExitP2;
            view.TryAddMapMarker(component, null, sign, title, null,
                comp =>
            {
                var t = comp.Cast<TeleporterExit>();
                if (t._player == null) return null;
                var id = (PlayerId)t._player.playerId;
                return id == PlayerId.P1 ? MarkerStyle.TeleExitP1.Color : MarkerStyle.TeleExitP2.Color;
            }, null, MarkerRow.Movable);
        }

        [HarmonyPatch(typeof(TeleporterExit), nameof(TeleporterExit.Initialize))]
        private class InitializePatch
        {
            public static void Postfix(TeleporterExit __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(TeleporterExit), nameof(TeleporterExit.OnDestroy))]
        private class OnDestroyPatch
        {
            public static void Prefix(TeleporterExit __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}