using System.Collections.Generic;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PortalMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component, HashSet<ObjectPatcher.SourceFlag> sources)
        {
            var obj = (Portal)component;
            switch (obj.type)
            {
                case Portal.Type.Regular:
                    view.TryAddMapMarker(component, sources, MarkerStyle.Portal.Sign, Strings.Portal, _ => MarkerStyle.Portal.Color, _ => 0);
                    break;
                case Portal.Type.Cliff:
                    view.TryAddMapMarker(component, sources, MarkerStyle.PortalCliff.Sign, Strings.PortalCliff, (comp) => ((Portal)comp).state switch
                    {
                        Portal.State.Destroyed => MarkerStyle.PortalCliff.Destroyed.Color,
                        Portal.State.Rebuilding => MarkerStyle.PortalCliff.Rebuilding.Color,
                        _ => MarkerStyle.PortalCliff.Color
                    }, _ => 0);
                    break;
                case Portal.Type.Dock:
                    view.TryAddMapMarker(component, sources, MarkerStyle.PortalDock.Sign, Strings.PortalDock, _ => MarkerStyle.PortalDock.Color, _ => 0);
                    break;
            }
        }
    }
}