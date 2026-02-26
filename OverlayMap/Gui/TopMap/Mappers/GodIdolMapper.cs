using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 神像（GodIdol）标记映射器。
    /// 显示玩家可拾取的神像货物，并展示其价格。
    /// </summary>
    public class GodIdolMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType MarkerType => MapMarkerType.GodIdol;

        public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
        {
            if (notifierType != NotifierType.PlayerCargo)
                return;

            view.TryAddMapMarker(
                component,
                MarkerStyle.GodIdol.Color,
                MarkerStyle.GodIdol.Sign,
                Strings.GodIdol,
                comp => GetPrice(comp));
        }

        /// <summary>
        /// 获取神像的价格。
        /// 从同级 PayableComponent 组件获取价格信息。
        /// </summary>
        private static int GetPrice(Component comp)
        {
            var payable = comp.GetComponent<PayableComponent>();
            return payable != null ? payable.Price : 0;
        }
    }
}
