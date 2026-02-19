using System.Collections.Generic;
using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using KingdomMod.SharedLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PlayerMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Player;

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

        [HarmonyPatch(typeof(Player), nameof(Player.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Player __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Player __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}