using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class TeleporterExitMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.TeleporterExit;

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
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(TeleporterExit))]
    public static class TeleporterExitNotifier
    {
        [HarmonyPatch(nameof(TeleporterExit.Initialize))]
        [HarmonyPostfix]
        public static void Initialize(TeleporterExit __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(TeleporterExit.OnDestroy))]
        [HarmonyPrefix]
        public static void OnDestroy(TeleporterExit __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
