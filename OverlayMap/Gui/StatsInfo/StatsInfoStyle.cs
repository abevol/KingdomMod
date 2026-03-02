using KingdomMod.OverlayMap.Assets;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;
#if IL2CPP
using Il2CppInterop.Runtime.Attributes;
using KingdomMod.SharedLib.Attributes;
#endif

namespace KingdomMod.OverlayMap.Gui.StatsInfo
{
    /// <summary>
    /// StatsInfoView 风格配置组件
    /// 管理统计信息面板的字体、背景等样式
    /// </summary>
#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class StatsInfoStyle : MonoBehaviour
    {
        public FontData TextFont;
        public float TextFontSize;

#if IL2CPP
        public StatsInfoStyle(IntPtr ptr) : base(ptr) { }
#endif

        private void Awake()
        {
            LogDebug("StatsInfoStyle.Awake");
            InitializeFonts();
            SubscribeConfigEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeConfigEvents();
        }

        /// <summary>
        /// 从当前配置初始化字体数据
        /// </summary>
        private void InitializeFonts()
        {
            TextFont = FontManager.CreateMainFont(Config.GuiStyle.StatsInfo.Text.Font, TextFont);
            TextFontSize = Config.GuiStyle.StatsInfo.Text.FontSize;
            TextFont.AssignFallbackFonts(Config.GuiStyle.StatsInfo.Text.FallbackFonts.AsStringArray);
        }

        /// <summary>
        /// 订阅当前配置项的变更事件
        /// </summary>
        public void SubscribeConfigEvents()
        {
            Config.GuiStyle.StatsInfo.BackgroundImageFile.Entry.SettingChanged += OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundColor.Entry.SettingChanged += OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundImageArea.Entry.SettingChanged += OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundImageBorder.Entry.SettingChanged += OnBackgroundConfigChanged;

            Config.GuiStyle.StatsInfo.Text.Font.Entry.SettingChanged += OnTextFontConfigChanged;
            Config.GuiStyle.StatsInfo.Text.FontSize.Entry.SettingChanged += OnTextFontSizeConfigChanged;
            Config.GuiStyle.StatsInfo.Text.FallbackFonts.Entry.SettingChanged += OnTextFallbackFontsConfigChanged;
        }

        /// <summary>
        /// 取消订阅当前配置项的变更事件。
        /// 必须在配置项被替换之前调用。
        /// </summary>
        public void UnsubscribeConfigEvents()
        {
            Config.GuiStyle.StatsInfo.BackgroundImageFile.Entry.SettingChanged -= OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundColor.Entry.SettingChanged -= OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundImageArea.Entry.SettingChanged -= OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundImageBorder.Entry.SettingChanged -= OnBackgroundConfigChanged;

            Config.GuiStyle.StatsInfo.Text.Font.Entry.SettingChanged -= OnTextFontConfigChanged;
            Config.GuiStyle.StatsInfo.Text.FontSize.Entry.SettingChanged -= OnTextFontSizeConfigChanged;
            Config.GuiStyle.StatsInfo.Text.FallbackFonts.Entry.SettingChanged -= OnTextFallbackFontsConfigChanged;
        }

        /// <summary>
        /// 重新加载配置：重建字体、订阅新配置事件、推送到 UI。
        /// 在语言切换后调用。
        /// </summary>
        public void ReloadConfig()
        {
            LogDebug("StatsInfoStyle.ReloadConfig");
            InitializeFonts();
            SubscribeConfigEvents();
            PushToUI();
        }

        /// <summary>
        /// 将当前字体和背景配置推送到所有 UI 组件
        /// </summary>
        private void PushToUI()
        {
            ForEachPlayerOverlay(overlay =>
            {
                overlay.StatsInfoView?.UpdateBackgroundImage();
                overlay.StatsInfoView?.UpdateTextFont(TextFont.Font);
                overlay.StatsInfoView?.UpdateTextFontSize(TextFontSize);
                overlay.StatsInfoView?.ForceTextMeshUpdate();
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnBackgroundConfigChanged(object sender, EventArgs e)
        {
            ForEachPlayerOverlay(overlay =>
            {
                overlay.StatsInfoView?.UpdateBackgroundImage();
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTextFontConfigChanged(object sender, EventArgs e)
        {
            TextFont = FontManager.CreateMainFont(Config.GuiStyle.StatsInfo.Text.Font, TextFont);
            TextFont.AssignFallbackFonts(Config.GuiStyle.StatsInfo.Text.FallbackFonts.AsStringArray);

            ForEachPlayerOverlay(overlay =>
            {
                overlay.StatsInfoView?.UpdateTextFont(TextFont.Font);
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTextFontSizeConfigChanged(object sender, EventArgs e)
        {
            TextFontSize = Config.GuiStyle.StatsInfo.Text.FontSize;

            ForEachPlayerOverlay(overlay =>
            {
                overlay.StatsInfoView?.UpdateTextFontSize(TextFontSize);
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTextFallbackFontsConfigChanged(object sender, EventArgs e)
        {
            var fallbackFonts = Config.GuiStyle.StatsInfo.Text.FallbackFonts.AsStringArray;
            TextFont.AssignFallbackFonts(fallbackFonts);

            ForEachPlayerOverlay(overlay =>
            {
                overlay.StatsInfoView?.ForceTextMeshUpdate();
            });
        }
    }
}
