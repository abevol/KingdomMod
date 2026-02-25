using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 神殿标记映射器。
    /// </summary>
    public class TholosMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Tholos;

        public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
        {
            if (resolverType == ResolverType.PayableUpgrade)
            {
                var payableUpgrade = component.Cast<PayableUpgrade>();
                if (payableUpgrade.gameObject.tag == Tags.Tholos)
                {
                    var marker = view.TryAddMapMarker(component, MarkerStyle.Tholos.Unpaid.Color, MarkerStyle.Tholos.Sign, Strings.Tholos,
                        comp => comp.Cast<PayableUpgrade>().Price);
                }
            }
            else if (resolverType == ResolverType.WorkableBuilding)
            {
                var workableBuilding = component.Cast<WorkableBuilding>();
                if (workableBuilding.gameObject.tag == Tags.Tholos)
                {
                    var marker = view.TryAddMapMarker(component,
                        component.gameObject.activeSelf ? MarkerStyle.Tholos.Color : MarkerStyle.Tholos.Building.Color,
                        MarkerStyle.Tholos.Sign, Strings.Tholos);
                    ConstructionEventHandler.Create(marker, MarkerStyle.Tholos.Color, MarkerStyle.Tholos.Building.Color);
                }
            }
        }
    }
}
