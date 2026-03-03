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
            var newFont = FontManager.CreateMainFont(Config.GuiStyle.ExtraInfo.Text.Font, TextFont);
            if (newFont != null)
            {
                TextFont = newFont;
                TextFont.AssignFallbackFonts(Config.GuiStyle.ExtraInfo.Text.FallbackFonts.AsStringArray);
            }
            TextFontSize = Config.GuiStyle.ExtraInfo.Text.FontSize;
        }

        /// <summary>
        /// 订阅当前配置项的变更事件
        /// </summary>
        public void SubscribeConfigEvents()
        {
            Config.GuiStyle.ExtraInfo.Text.Font.Entry.SettingChanged += OnTextFontConfigChanged;
            Config.GuiStyle.ExtraInfo.Text.FontSize.Entry.SettingChanged += OnTextFontSizeConfigChanged;
            Config.GuiStyle.ExtraInfo.Text.FallbackFonts.Entry.SettingChanged += OnTextFallbackFontsConfigChanged;
        }

        /// <summary>
        /// 取消订阅当前配置项的变更事件。
        /// 必须在配置项被替换之前调用。
        /// </summary>
        public void UnsubscribeConfigEvents()
        {
            Config.GuiStyle.ExtraInfo.Text.Font.Entry.SettingChanged -= OnTextFontConfigChanged;
            Config.GuiStyle.ExtraInfo.Text.FontSize.Entry.SettingChanged -= OnTextFontSizeConfigChanged;
            Config.GuiStyle.ExtraInfo.Text.FallbackFonts.Entry.SettingChanged -= OnTextFallbackFontsConfigChanged;
        }

        /// <summary>
        /// 重新加载配置：重建字体、订阅新配置事件、推送到 UI。
        /// 在语言切换后调用。
        /// </summary>
        public void ReloadConfig()
        {
            LogDebug("ExtraInfoStyle.ReloadConfig");
            InitializeFonts();
            SubscribeConfigEvents();
            PushToUI();
        }

        /// <summary>
        /// 将当前字体配置推送到所有 UI 组件
        /// </summary>
        private void PushToUI()
        {
            ForEachPlayerOverlay(overlay =>
            {
                if (TextFont != null)
                {
                    overlay.ExtraInfoView?.UpdateTextFont(TextFont.Font);
                    overlay.ExtraInfoView?.UpdateTextFontSize(TextFontSize);
                    overlay.ExtraInfoView?.ForceTextMeshUpdate();
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTextFontConfigChanged(object sender, EventArgs e)
        {
            var newFont = FontManager.CreateMainFont(Config.GuiStyle.ExtraInfo.Text.Font, TextFont);
            if (newFont == null) return;

            TextFont = newFont;
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
            if (TextFont == null) return;

            var fallbackFonts = Config.GuiStyle.ExtraInfo.Text.FallbackFonts.AsStringArray;
            TextFont.AssignFallbackFonts(fallbackFonts);

            ForEachPlayerOverlay(overlay =>
            {
                overlay.ExtraInfoView?.ForceTextMeshUpdate();
            });
        }
    }
}
