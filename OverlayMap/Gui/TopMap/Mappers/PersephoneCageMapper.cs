using KingdomMod.OverlayMap.Config;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PersephoneCageMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(component, null, MarkerStyle.PersephoneCage.Sign, Strings.HermitPersephone, null,
                comp => PersephoneCage.State.IsPersephoneLocked(((PersephoneCage)comp)._fsm.Current)
                    ? MarkerStyle.PersephoneCage.Locked.Color
                    : MarkerStyle.PersephoneCage.Unlocked.Color);
        }
    }
}