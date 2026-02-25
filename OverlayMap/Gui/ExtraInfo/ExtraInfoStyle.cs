using KingdomMod.OverlayMap.Assets;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;
#if IL2CPP
using Il2CppInterop.Runtime.Attributes;
using KingdomMod.SharedLib.Attributes;
#endif

namespace KingdomMod.OverlayMap.Gui.ExtraInfo
{
    /// <summary>
    /// ExtraInfoView 风格配置组件
    /// 管理额外信息显示的字体等样式
    /// </summary>
#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class ExtraInfoStyle : MonoBehaviour
    {
        public FontData TextFont;
        public float TextFontSize;

#if IL2CPP
        public ExtraInfoStyle(IntPtr ptr) : base(ptr) { }
#endif

        private void Awake()
        {
            LogDebug("ExtraInfoStyle.Awake");

            // 初始化文本字体
            TextFont = FontManager.CreateMainFont(Config.GuiStyle.ExtraInfo.Text.Font, null);
            TextFontSize = Config.GuiStyle.ExtraInfo.Text.FontSize;
            TextFont.AssignFallbackFonts(Config.GuiStyle.ExtraInfo.Text.FallbackFonts.AsStringArray);

            // 监听配置变化
            Config.GuiStyle.ExtraInfo.Text.Font.Entry.SettingChanged += OnTextFontConfigChanged;
            Config.GuiStyle.ExtraInfo.Text.FontSize.Entry.SettingChanged += OnTextFontSizeConfigChanged;
            Config.GuiStyle.ExtraInfo.Text.FallbackFonts.Entry.SettingChanged += OnTextFallbackFontsConfigChanged;
        }

        private void OnDestroy()
        {
            Config.GuiStyle.ExtraInfo.Text.Font.Entry.SettingChanged -= OnTextFontConfigChanged;
            Config.GuiStyle.ExtraInfo.Text.FontSize.Entry.SettingChanged -= OnTextFontSizeConfigChanged;
            Config.GuiStyle.ExtraInfo.Text.FallbackFonts.Entry.SettingChanged -= OnTextFallbackFontsConfigChanged;
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTextFontConfigChanged(object sender, EventArgs e)
        {
            TextFont = FontManager.CreateMainFont(Config.GuiStyle.ExtraInfo.Text.Font, TextFont);
            TextFont.AssignFallbackFonts(Config.GuiStyle.ExtraInfo.Text.FallbackFonts.AsStringArray);

            ForEachPlayerOverlay(overlay =>
            {
                overlay.ExtraInfoView?.UpdateTextFont(TextFont.Font);
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTextFontSizeConfigChanged(object sender, EventArgs e)
        {
            TextFontSize = Config.GuiStyle.ExtraInfo.Text.FontSize;

            ForEachPlayerOverlay(overlay =>
            {
                overlay.ExtraInfoView?.UpdateTextFontSize(TextFontSize);
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTextFallbackFontsConfigChanged(object sender, EventArgs e)
        {
            var fallbackFonts = Config.GuiStyle.ExtraInfo.Text.FallbackFonts.AsStringArray;
            TextFont.AssignFallbackFonts(fallbackFonts);

            ForEachPlayerOverlay(overlay =>
            {
                overlay.ExtraInfoView?.ForceTextMeshUpdate();
            });
        }
    }
}
