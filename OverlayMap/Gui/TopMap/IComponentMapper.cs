using System.Collections.Generic;
using UnityEngine;
using static KingdomMod.OverlayMap.Patchers.ObjectPatcher;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    public interface IComponentMapper
    {
        void Map(Component component, HashSet<SourceFlag> sources);
    }
}