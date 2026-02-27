using HarmonyLib;
using UnityEngine;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
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
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Castle))]
    public static class CastleNotifier
    {
        [HarmonyPatch(nameof(Castle.Start))]
        [HarmonyPostfix]
        public static void Start(Castle __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Castle.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Castle __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
