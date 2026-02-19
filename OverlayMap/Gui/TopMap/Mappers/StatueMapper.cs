using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class StatueMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Statue;

        public void Map(Component component)
        {
            var obj = component.Cast<Statue>();
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
                    bool isLocked = comp.Cast<Statue>().deityStatus != Statue.DeityStatus.Activated;
                    return isLocked ? comp.Cast<Statue>().Price : 0;
                },
                comp =>
                {
                    bool isLocked = comp.Cast<Statue>().deityStatus != Statue.DeityStatus.Activated;
                    return isLocked ? MarkerStyle.Statues.Locked.Color : MarkerStyle.Statues.Unlocked.Color;
                });
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}