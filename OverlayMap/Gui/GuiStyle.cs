using KingdomMod.OverlayMap.Gui.TopMap;
using KingdomMod.OverlayMap.Gui.StatsInfo;
using KingdomMod.OverlayMap.Gui.ExtraInfo;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;
#if IL2CPP
using KingdomMod.SharedLib.Attributes;
#endif

namespace KingdomMod.OverlayMap.Gui
{
#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class GuiStyle : MonoBehaviour
    {
        public TopMapStyle TopMapStyle;
        public StatsInfoStyle StatsInfoStyle;
        public ExtraInfoStyle ExtraInfoStyle;

#if IL2CPP
        public GuiStyle(IntPtr ptr) : base(ptr) { }
#endif

        private void Awake()
        {
            LogDebug($"GuiStyle.Awake");
            TopMapStyle = CreateTopMapStyle();
            StatsInfoStyle = CreateStatsInfoStyle();
            ExtraInfoStyle = CreateExtraInfoStyle();
        }

        private TopMapStyle CreateTopMapStyle()
        {
            LogDebug($"CreateTopMapStyle");

            var guiObj = new GameObject(nameof(TopMap.TopMapStyle));
            guiObj.transform.SetParent(this.transform, false);
            var guiComp = guiObj.AddComponent<TopMapStyle>();

            return guiComp;
        }

        private StatsInfoStyle CreateStatsInfoStyle()
        {
            LogDebug($"CreateStatsInfoStyle");

            var guiObj = new GameObject(nameof(StatsInfo.StatsInfoStyle));
            guiObj.transform.SetParent(this.transform, false);
            var guiComp = guiObj.AddComponent<StatsInfoStyle>();

            return guiComp;
        }

        private ExtraInfoStyle CreateExtraInfoStyle()
        {
            LogDebug($"CreateExtraInfoStyle");

            var guiObj = new GameObject(nameof(ExtraInfo.ExtraInfoStyle));
            guiObj.transform.SetParent(this.transform, false);
            var guiComp = guiObj.AddComponent<ExtraInfoStyle>();

            return guiComp;
        }

        /// <summary>
        /// 取消所有子样式组件的配置事件订阅。
        /// 在语言切换前调用，确保旧配置项的事件被正确取消。
        /// </summary>
        public void UnsubscribeConfigEvents()
        {
            TopMapStyle?.UnsubscribeConfigEvents();
            StatsInfoStyle?.UnsubscribeConfigEvents();
            ExtraInfoStyle?.UnsubscribeConfigEvents();
        }

        /// <summary>
        /// 重新加载所有子样式组件的配置。
        /// 在语言切换后调用，从新配置项重建字体并推送到 UI。
        /// </summary>
        public void ReloadConfig()
        {
            TopMapStyle?.ReloadConfig();
            StatsInfoStyle?.ReloadConfig();
            ExtraInfoStyle?.ReloadConfig();
        }
    }
}
