using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class TeleporterExitMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var teleExit = (TeleporterExit)component;
            var playerId = (PlayerId)teleExit._player.playerId;
            var sign = playerId == PlayerId.P1 ? MarkerStyle.TeleExitP1.Sign : MarkerStyle.TeleExitP2.Sign;
            var title = playerId == PlayerId.P1 ? Strings.TeleExitP1 : Strings.TeleExitP2;
            view.TryAddMapMarker(component, null, sign, title, null,
                comp =>
            {
                var t = (TeleporterExit)comp;
                var id = (PlayerId)t._player.playerId;
                return id == PlayerId.P1 ? MarkerStyle.TeleExitP1.Color : MarkerStyle.TeleExitP2.Color;
            }, null, MarkerRow.Movable);
        }
    }
}