using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BombMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Bomb.Color, MarkerStyle.Bomb.Sign, Strings.Bomb, null, null, null, MarkerRow.Movable);
        }
    }
}