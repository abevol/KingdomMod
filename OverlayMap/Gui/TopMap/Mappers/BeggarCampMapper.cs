using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class BeggarCampMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component, HashSet<ObjectPatcher.SourceFlag> sources)
        {
            view.TryAddMapMarker(component, sources, MarkerStyle.BeggarCamp.Sign, Strings.BeggarCamp,
                _ => MarkerStyle.BeggarCamp.Color,
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