using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PlayerMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var marker = view.TryAddMapMarker(component, MarkerStyle.Player.Color, MarkerStyle.Player.Sign, Strings.You, null, null,
                comp =>
                {
                    var p = (Player)comp;
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

                LogDebug($"player.playerId: {((Player)component).playerId}, _playerMarkers.Count: {view.PlayerMarkers.Count}");
            }
        }
    }
}