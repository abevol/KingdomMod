using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CastleMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Castle;

        public Component[] GetComponents()
        {
            return [Managers.Inst.kingdom.castle];
        }

        public void Map(Component component)
        {
            var marker = view.TryAddMapMarker(component, MarkerStyle.Castle.Color, MarkerStyle.Castle.Sign, Strings.Castle, (comp) =>
            {
                var payable = comp.Cast<Castle>()._payableUpgrade;
                bool canPay = !payable.IsLocked(GetLocalPlayer(), out var reason);
                bool isLockedForInvalidTime = reason == LockIndicator.LockReason.InvalidTime;
                var price = isLockedForInvalidTime ? (int)(payable.timeAvailableFrom - Time.time) : canPay ? payable.Price : 0;
                return price;
            }, (comp) =>
            {
                var payable = comp.Cast<Castle>()._payableUpgrade;
                bool _ = !payable.IsLocked(GetLocalPlayer(), out var reason);
                bool isLocked = reason != LockIndicator.LockReason.NotLocked && reason != LockIndicator.LockReason.NoUpgrade;
                var color = isLocked ? MarkerStyle.Castle.Locked.Color : MarkerStyle.Castle.Color;
                return color;
            });

            if (marker != null)
                view.CastleMarker = marker;
        }

        [HarmonyPatch(typeof(Castle), nameof(Castle.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Castle __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }
        
        [HarmonyPatch(typeof(Castle), nameof(Castle.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Castle __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}