using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CastleMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var marker = view.TryAddMapMarker(component, MarkerStyle.Castle.Color, MarkerStyle.Castle.Sign, Strings.Castle, (comp) =>
            {
                var payable = ((Castle)comp)._payableUpgrade;
                bool canPay = !payable.IsLocked(GetLocalPlayer(), out var reason);
                bool isLockedForInvalidTime = reason == LockIndicator.LockReason.InvalidTime;
                var price = isLockedForInvalidTime ? (int)(payable.timeAvailableFrom - Time.time) : canPay ? payable.Price : 0;
                return price;
            }, (comp) =>
            {
                var payable = ((Castle)comp)._payableUpgrade;
                bool _ = !payable.IsLocked(GetLocalPlayer(), out var reason);
                bool isLocked = reason != LockIndicator.LockReason.NotLocked && reason != LockIndicator.LockReason.NoUpgrade;
                var color = isLocked ? MarkerStyle.Castle.Locked.Color : MarkerStyle.Castle.Color;
                return color;
            });

            if (marker != null)
                view.CastleMarker = marker;
        }

        [HarmonyPatch(typeof(Castle), nameof(Castle.Awake))]
        private class Deer_Awake_Patch
        {
            public static void Postfix(Castle __instance)
            {
                TopMapView.ForEachTopMapView(view => view.OnComponentCreated(__instance, [ObjectPatcher.SourceFlag.Create10]));
            }
        }
        
        [HarmonyPatch(typeof(Castle), nameof(Castle.OnDestroy))]
        private class Deer_OnDestroy_Patch
        {
            public static void Prefix(Castle __instance)
            {
                TopMapView.ForEachTopMapView(view => view.OnComponentDestroyed(__instance, [ObjectPatcher.SourceFlag.Destroy10]));
            }
        }
    }
}