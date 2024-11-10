using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeggarCampMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.BeggarCamp.Color, MarkerStyle.BeggarCamp.Sign, Strings.BeggarCamp,
                comp =>
                {
                    int count = 0;
                    foreach (var beggar in ((BeggarCamp)comp)._beggars)
                    {
                        if (beggar != null && beggar.isActiveAndEnabled)
                            count++;
                    }

                    return count;
                });
        }
    }
}