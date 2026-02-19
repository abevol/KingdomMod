using KingdomMod.OverlayMap.Assets;
using KingdomMod.Shared.Attributes;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;
#if IL2CPP
using Il2CppInterop.Runtime.Attributes;
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

            // 初始化文本字体
            TextFont = FontManager.CreateMainFont(Config.GuiStyle.StatsInfo.Text.Font, null);
            TextFontSize = Config.GuiStyle.StatsInfo.Text.FontSize;
            TextFont.AssignFallbackFonts(Config.GuiStyle.StatsInfo.Text.FallbackFonts.AsStringArray);

            // 监听配置变化
            Config.GuiStyle.StatsInfo.BackgroundImageFile.Entry.SettingChanged += OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundColor.Entry.SettingChanged += OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundImageArea.Entry.SettingChanged += OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundImageBorder.Entry.SettingChanged += OnBackgroundConfigChanged;

            Config.GuiStyle.StatsInfo.Text.Font.Entry.SettingChanged += OnTextFontConfigChanged;
            Config.GuiStyle.StatsInfo.Text.FontSize.Entry.SettingChanged += OnTextFontSizeConfigChanged;
            Config.GuiStyle.StatsInfo.Text.FallbackFonts.Entry.SettingChanged += OnTextFallbackFontsConfigChanged;
        }

        private void OnDestroy()
        {
            Config.GuiStyle.StatsInfo.BackgroundImageFile.Entry.SettingChanged -= OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundColor.Entry.SettingChanged -= OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundImageArea.Entry.SettingChanged -= OnBackgroundConfigChanged;
            Config.GuiStyle.StatsInfo.BackgroundImageBorder.Entry.SettingChanged -= OnBackgroundConfigChanged;

            Config.GuiStyle.StatsInfo.Text.Font.Entry.SettingChanged -= OnTextFontConfigChanged;
            Config.GuiStyle.StatsInfo.Text.FontSize.Entry.SettingChanged -= OnTextFontSizeConfigChanged;
            Config.GuiStyle.StatsInfo.Text.FallbackFonts.Entry.SettingChanged -= OnTextFallbackFontsConfigChanged;
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
