using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PortalMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var obj = (Portal)component;
            switch (obj.type)
            {
                case Portal.Type.Regular:
                    view.TryAddMapMarker(component, MarkerStyle.Portal.Color, MarkerStyle.Portal.Sign, Strings.Portal);
                    break;
                case Portal.Type.Cliff:
                    view.TryAddMapMarker(component, MarkerStyle.PortalCliff.Color, MarkerStyle.PortalCliff.Sign, Strings.PortalCliff, null,
                        (comp) => ((Portal)comp).state switch
                    {
                        Portal.State.Destroyed => MarkerStyle.PortalCliff.Destroyed.Color,
                        Portal.State.Rebuilding => MarkerStyle.PortalCliff.Rebuilding.Color,
                        _ => MarkerStyle.PortalCliff.Color
                    });
                    break;
                case Portal.Type.Dock:
                    view.TryAddMapMarker(component, MarkerStyle.PortalDock.Color, MarkerStyle.PortalDock.Sign, Strings.PortalDock);
                    break;
            }
        }
    }
}