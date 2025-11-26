using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class CabinMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var obj = component.Cast<Cabin>();
            var title = obj.hermitType switch
            {
                Hermit.HermitType.Horse => Strings.HermitHorse,
                Hermit.HermitType.Horn => Strings.HermitHorn,
                Hermit.HermitType.Ballista => Strings.HermitBallista,
                Hermit.HermitType.Baker => Strings.HermitBaker,
                Hermit.HermitType.Knight => Strings.HermitKnight,
                Hermit.HermitType.Persephone => Strings.HermitPersephone,
                Hermit.HermitType.Fire => Strings.HermitFire,
                _ => LogUnknownHermitType(obj.hermitType)
            };

            view.TryAddMapMarker(component, null, MarkerStyle.HermitCabins.Sign, title,
                comp => comp.Cast<Cabin>().canPay ? comp.Cast<Cabin>().Price : 0,
                comp => comp.Cast<Cabin>().canPay? MarkerStyle.HermitCabins.Locked.Color : MarkerStyle.HermitCabins.Unlocked.Color);
        }

        private static ConfigEntryWrapper<string> LogUnknownHermitType(Hermit.HermitType hermitType)
        {
            LogWarning($"Unknown hermit type: {hermitType}");
            return null;
        }
    }
}