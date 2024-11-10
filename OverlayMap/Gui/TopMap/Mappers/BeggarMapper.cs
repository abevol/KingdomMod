using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeggarMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Beggar.Color, MarkerStyle.Beggar.Sign, Strings.Beggar, null, null,
                comp => ((Beggar)comp).hasFoundBaker, MarkerRow.Movable);
        }
    }
}