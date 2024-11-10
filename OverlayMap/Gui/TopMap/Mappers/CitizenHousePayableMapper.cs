using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CitizenHousePayableMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.CitizenHouse.Building.Color, MarkerStyle.CitizenHouse.Sign, Strings.CitizenHouse,
                comp => ((CitizenHousePayable)comp)._numberOfAvailableCitizens,
                comp => comp.gameObject.activeSelf ? MarkerStyle.CitizenHouse.Color : MarkerStyle.CitizenHouse.Building.Color);
        }
    }
}