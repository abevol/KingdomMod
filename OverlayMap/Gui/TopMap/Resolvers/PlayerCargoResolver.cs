using KingdomMod.SharedLib;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Resolvers
{
    /// <summary>
    /// PlayerCargo 组件解析器。
    /// </summary>
    public class PlayerCargoResolver : IMarkerResolver
    {
        public ResolverType ResolverType => ResolverType.PlayerCargo;

        public Type TargetComponentType => typeof(PlayerCargo);

        public MapMarkerType? Resolve(Component component)
        {
            var obj = component.Cast<PlayerCargo>();
            var cargoType = obj._cargoData._cargoType;
            // 根据 CargoType 返回对应的标记类型
            return cargoType switch
            {
                CargoType.GodIdol => MapMarkerType.GodIdol,
                _ => MapMarkerType.PlayerCargo
            };
        }
    }
}
