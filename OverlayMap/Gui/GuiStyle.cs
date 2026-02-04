using KingdomMod.OverlayMap.Gui.TopMap;
using KingdomMod.OverlayMap.Gui.StatsInfo;
using KingdomMod.OverlayMap.Gui.ExtraInfo;
using KingdomMod.Shared.Attributes;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui
{
    [RegisterTypeInIl2Cpp]
    public class GuiStyle : MonoBehaviour
    {
        public TopMapStyle topMapStyle;
        public StatsInfoStyle statsInfoStyle;
        public ExtraInfoStyle extraInfoStyle;

#if IL2CPP
        public GuiStyle(IntPtr ptr) : base(ptr) { }
#endif

        private void Awake()
        {
            LogTrace($"GuiStyle.Awake");
            topMapStyle = CreateTopMapStyle();
            statsInfoStyle = CreateStatsInfoStyle();
            extraInfoStyle = CreateExtraInfoStyle();
        }

        private TopMapStyle CreateTopMapStyle()
        {
            LogDebug($"CreateTopMapStyle");

            var guiObj = new GameObject(nameof(TopMapStyle));
            guiObj.transform.SetParent(this.transform, false);
            var guiComp = guiObj.AddComponent<TopMapStyle>();

            return guiComp;
        }

        private StatsInfoStyle CreateStatsInfoStyle()
        {
            LogDebug($"CreateStatsInfoStyle");

            var guiObj = new GameObject(nameof(StatsInfoStyle));
            guiObj.transform.SetParent(this.transform, false);
            var guiComp = guiObj.AddComponent<StatsInfoStyle>();

            return guiComp;
        }

        private ExtraInfoStyle CreateExtraInfoStyle()
        {
            LogDebug($"CreateExtraInfoStyle");

            var guiObj = new GameObject(nameof(ExtraInfoStyle));
            guiObj.transform.SetParent(this.transform, false);
            var guiComp = guiObj.AddComponent<ExtraInfoStyle>();

            return guiComp;
        }
    }
}
