using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class StatueMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var obj = (Statue)component;
            var title = obj.deity switch
            {
                Statue.Deity.Archer => Strings.StatueArcher,
                Statue.Deity.Worker => Strings.StatueWorker,
                Statue.Deity.Knight => Strings.StatueKnight,
                Statue.Deity.Farmer => Strings.StatueFarmer,
                Statue.Deity.Time => Strings.StatueTime,
                Statue.Deity.Pike => Strings.StatuePike,
                _ => null
            };
            view.TryAddMapMarker(component, null, MarkerStyle.Statues.Sign, title,
                comp =>
                {
                    bool isLocked = ((Statue)comp).deityStatus != Statue.DeityStatus.Activated;
                    return isLocked ? ((Statue)comp).Price : 0;
                },
                comp =>
                {
                    bool isLocked = ((Statue)comp).deityStatus != Statue.DeityStatus.Activated;
                    return isLocked ? MarkerStyle.Statues.Locked.Color : MarkerStyle.Statues.Unlocked.Color;
                });
        }
    }
}