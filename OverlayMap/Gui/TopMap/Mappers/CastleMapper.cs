﻿using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CastleMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component, HashSet<ObjectPatcher.SourceFlag> sources)
        {
            view.TryAddMapMarker(component, sources, MarkerStyle.Castle.Sign, Strings.Castle,
                (comp) =>
                {
                    var payable = ((Castle)comp)._payableUpgrade;
                    bool _ = !payable.IsLocked(GetLocalPlayer(), out var reason);
                    bool isLocked = reason != LockIndicator.LockReason.NotLocked && reason != LockIndicator.LockReason.NoUpgrade;
                    var color = isLocked ? MarkerStyle.Castle.Locked.Color : MarkerStyle.Castle.Color;
                    return color;
                }, (comp) =>
                {
                    var payable = ((Castle)comp)._payableUpgrade;
                    bool canPay = !payable.IsLocked(GetLocalPlayer(), out var reason);
                    bool isLockedForInvalidTime = reason == LockIndicator.LockReason.InvalidTime;
                    var price = isLockedForInvalidTime ? (int)(payable.timeAvailableFrom - Time.time) : canPay ? payable.Price : 0;
                    return price;
                });
        }
    }
}