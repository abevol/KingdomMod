using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class ChestMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var obj = component.Cast<Chest>();
            var isGem = obj.currencyType == CurrencyType.Gems;
            view.TryAddMapMarker(component,
                isGem ? MarkerStyle.GemChest.Color : MarkerStyle.Chest.Color,
                isGem ? MarkerStyle.GemChest.Sign : MarkerStyle.Chest.Sign,
                isGem ? Strings.GemChest : Strings.Chest,
                comp => comp.Cast<Chest>().currencyAmount,
                null, comp => comp.Cast<Chest>().currencyAmount != 0);
        }
    }
}