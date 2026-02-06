using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    public interface IComponentMapper
    {
        Component[] GetComponents() => [];
        void Map(Component component);
    }
}