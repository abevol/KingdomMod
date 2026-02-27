using HarmonyLib;
using KingdomMod.SharedLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PlayerMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.Player;

        public Component[] GetComponents()
        {
            return new Component[]
            {
                Managers.Inst.kingdom.GetPlayer(0),
                Managers.Inst.kingdom.GetPlayer(1)
            }.WithoutNulls();
        }

        public void Map(Component component)
        {
            var marker = view.TryAddMapMarker(component, MarkerStyle.Player.Color, MarkerStyle.Player.Sign, Strings.You, null, null,
                comp =>
                {
                    var p = comp.Cast<Player>();
                    if (p == null) return false;
                    if (p.isActiveAndEnabled == false) return false;
                    var mover = p.mover;
                    if (mover == null) return false;
                    return true;
                }, MarkerRow.Movable);

            if (marker != null)
            {
                if (view.PlayerMarkers.Contains(marker))
                {
                    LogError("Player marker already exists");
                }
                else
                {
                    view.PlayerMarkers.Add(marker);
                }

                LogDebug($"player.playerId: {component.Cast<Player>().playerId}, _playerMarkers.Count: {view.PlayerMarkers.Count}");
            }
        }
    }
}

namespace KingdomMod.OverlayMap.Gui.TopMap.Notifiers
{
    // 该 Notifier 只服务于该组件的映射，根据“相关性聚合”原则放在同一个文件。

    [HarmonyPatch(typeof(Player))]
    public static class PlayerNotifier
    {
        [HarmonyPatch(nameof(Player.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(Player __instance)
        {
            ForEachTopMapView(view => view.OnComponentCreated(__instance));
        }

        [HarmonyPatch(nameof(Player.OnDisable))]
        [HarmonyPrefix]
        public static void OnDisable(Player __instance)
        {
            ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
        }
    }
}
