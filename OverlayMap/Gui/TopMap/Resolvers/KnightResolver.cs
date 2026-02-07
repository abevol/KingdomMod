using System;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    public class KnightResolver : IMarkerResolver
    {
        public Type TargetComponentType => typeof(Knight);

        public MapMarkerType? Resolve(Component component)
        {
            return MapMarkerType.Knight;
        }
    }
}
