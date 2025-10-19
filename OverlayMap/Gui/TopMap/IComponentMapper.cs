using System;
using System.Collections.Generic;
using UnityEngine;
using static KingdomMod.OverlayMap.Patchers.ObjectPatcher;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    public interface IComponentMapper
    {
        Component[] GetComponents() => [];
        void Map(Component component);
    }
}