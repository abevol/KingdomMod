using KingdomMod.OverlayMap.Assets;
using KingdomMod.OverlayMap.Patchers;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;
#if IL2CPP
using Il2CppInterop.Runtime.Attributes;
using KingdomMod.SharedLib.Attributes;
#endif

namespace KingdomMod.OverlayMap.Gui.TopMap
{
#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class TopMapStyle : MonoBehaviour
    {
        public FontData SignFont;
        public float SignFontSize;
        public FontData TitleFont;
        public float TitleFontSize;
        public FontData CountFont;
        public float CountFontSize;

        private void Awake()
        {
            LogDebug("TopMapStyle.Init");
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
            LogDebug($"TopMapStyle.InitializeFonts, Sign.Font: {Config.GuiStyle.TopMap.Sign.Font.Value}");
            LogDebug($"TopMapStyle.InitializeFonts, Title.Font: {Config.GuiStyle.TopMap.Title.Font.Value}");
            LogDebug($"TopMapStyle.InitializeFonts, Count.Font: {Config.GuiStyle.TopMap.Count.Font.Value}");

            var signFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Sign.Font, SignFont);
            if (signFont != null)
            {
                SignFont = signFont;
                SignFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Sign.FallbackFonts.AsStringArray);
            }
            SignFontSize = Config.GuiStyle.TopMap.Sign.FontSize;

            var titleFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Title.Font, TitleFont);
            if (titleFont != null)
            {
                TitleFont = titleFont;
                TitleFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Title.FallbackFonts.AsStringArray);
            }
            TitleFontSize = Config.GuiStyle.TopMap.Title.FontSize;

            var countFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Count.Font, CountFont);
            if (countFont != null)
            {
                CountFont = countFont;
                CountFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Count.FallbackFonts.AsStringArray);
            }
            CountFontSize = Config.GuiStyle.TopMap.Count.FontSize;
        }

        /// <summary>
        /// 订阅当前配置项的变更事件
        /// </summary>
        public void SubscribeConfigEvents()
        {
            Config.GuiStyle.TopMap.BackgroundImageFile.Entry.SettingChanged += OnBackgroundConfigChanged;
            Config.GuiStyle.TopMap.BackgroundColor.Entry.SettingChanged += OnBackgroundConfigChanged;
            Config.GuiStyle.TopMap.BackgroundImageArea.Entry.SettingChanged += OnBackgroundConfigChanged;
            Config.GuiStyle.TopMap.BackgroundImageBorder.Entry.SettingChanged += OnBackgroundConfigChanged;

            Config.GuiStyle.TopMap.Sign.Font.Entry.SettingChanged += OnSignFontConfigChanged;
            Config.GuiStyle.TopMap.Sign.FontSize.Entry.SettingChanged += OnSignFontSizeConfigChanged;
            Config.GuiStyle.TopMap.Sign.FallbackFonts.Entry.SettingChanged += OnSignFallbackFontsConfigChanged;

            Config.GuiStyle.TopMap.Title.Font.Entry.SettingChanged += OnTitleFontConfigChanged;
            Config.GuiStyle.TopMap.Title.FontSize.Entry.SettingChanged += OnTitleFontSizeConfigChanged;
            Config.GuiStyle.TopMap.Title.FallbackFonts.Entry.SettingChanged += OnTitleFallbackFontsConfigChanged;

            Config.GuiStyle.TopMap.Count.Font.Entry.SettingChanged += OnCountFontConfigChanged;
            Config.GuiStyle.TopMap.Count.FontSize.Entry.SettingChanged += OnCountFontSizeConfigChanged;
            Config.GuiStyle.TopMap.Count.FallbackFonts.Entry.SettingChanged += OnCountFallbackFontsConfigChanged;
        }

        /// <summary>
        /// 取消订阅当前配置项的变更事件。
        /// 必须在配置项被替换之前调用，否则会取消错误的事件订阅。
        /// </summary>
        public void UnsubscribeConfigEvents()
        {
            Config.GuiStyle.TopMap.BackgroundImageFile.Entry.SettingChanged -= OnBackgroundConfigChanged;
            Config.GuiStyle.TopMap.BackgroundColor.Entry.SettingChanged -= OnBackgroundConfigChanged;
            Config.GuiStyle.TopMap.BackgroundImageArea.Entry.SettingChanged -= OnBackgroundConfigChanged;
            Config.GuiStyle.TopMap.BackgroundImageBorder.Entry.SettingChanged -= OnBackgroundConfigChanged;

            Config.GuiStyle.TopMap.Sign.Font.Entry.SettingChanged -= OnSignFontConfigChanged;
            Config.GuiStyle.TopMap.Sign.FontSize.Entry.SettingChanged -= OnSignFontSizeConfigChanged;
            Config.GuiStyle.TopMap.Sign.FallbackFonts.Entry.SettingChanged -= OnSignFallbackFontsConfigChanged;

            Config.GuiStyle.TopMap.Title.Font.Entry.SettingChanged -= OnTitleFontConfigChanged;
            Config.GuiStyle.TopMap.Title.FontSize.Entry.SettingChanged -= OnTitleFontSizeConfigChanged;
            Config.GuiStyle.TopMap.Title.FallbackFonts.Entry.SettingChanged -= OnTitleFallbackFontsConfigChanged;

            Config.GuiStyle.TopMap.Count.Font.Entry.SettingChanged -= OnCountFontConfigChanged;
            Config.GuiStyle.TopMap.Count.FontSize.Entry.SettingChanged -= OnCountFontSizeConfigChanged;
            Config.GuiStyle.TopMap.Count.FallbackFonts.Entry.SettingChanged -= OnCountFallbackFontsConfigChanged;
        }

        /// <summary>
        /// 重新加载配置：重建字体、订阅新配置事件、推送到 UI。
        /// 在语言切换后调用，此时配置项已被替换为新语言的配置项。
        /// </summary>
        public void ReloadConfig()
        {
            LogDebug("TopMapStyle.ReloadConfig");
            InitializeFonts();
            SubscribeConfigEvents();
            PushToUI();
        }

        /// <summary>
        /// 将当前字体和背景配置推送到所有 UI 组件
        /// </summary>
        private void PushToUI()
        {
            ForEachTopMapView(view =>
            {
                view.UpdateBackgroundImage();
                foreach (var pair in view.MapMarkers)
                {
                    if (SignFont != null)
                    {
                        pair.Value.UpdateSignFont(SignFont.Font);
                        pair.Value.UpdateSignFontSize(SignFontSize);
                        pair.Value.ForceSignMeshUpdate();
                    }

                    if (TitleFont != null)
                    {
                        pair.Value.UpdateTitleFont(TitleFont.Font);
                        pair.Value.UpdateTitleFontSize(TitleFontSize);
                        pair.Value.ForceTitleMeshUpdate();
                    }

                    if (CountFont != null)
                    {
                        pair.Value.UpdateCountFont(CountFont.Font);
                        pair.Value.UpdateCountFontSize(CountFontSize);
                        pair.Value.ForceCountMeshUpdate();
                    }
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnBackgroundConfigChanged(object sender, EventArgs e)
        {
            ForEachTopMapView(view => view.UpdateBackgroundImage());
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnSignFontConfigChanged(object sender, EventArgs e)
        {
            var newFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Sign.Font, SignFont);
            if (newFont == null) return;

            SignFont = newFont;
            SignFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Sign.FallbackFonts.AsStringArray);

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.UpdateSignFont(SignFont.Font);
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnSignFontSizeConfigChanged(object sender, EventArgs e)
        {
            SignFontSize = Config.GuiStyle.TopMap.Sign.FontSize;

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.UpdateSignFontSize(SignFontSize);
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnSignFallbackFontsConfigChanged(object sender, EventArgs e)
        {
            if (SignFont == null) return;

            var fallbackFonts = Config.GuiStyle.TopMap.Sign.FallbackFonts.AsStringArray;
            SignFont.AssignFallbackFonts(fallbackFonts);

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.ForceSignMeshUpdate();
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTitleFontConfigChanged(object sender, EventArgs e)
        {
            var newFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Title.Font, TitleFont);
            if (newFont == null) return;

            TitleFont = newFont;
            TitleFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Title.FallbackFonts.AsStringArray);

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.UpdateTitleFont(TitleFont.Font);
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTitleFontSizeConfigChanged(object sender, EventArgs e)
        {
            TitleFontSize = Config.GuiStyle.TopMap.Title.FontSize;

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.UpdateTitleFontSize(TitleFontSize);
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnTitleFallbackFontsConfigChanged(object sender, EventArgs e)
        {
            if (TitleFont == null) return;

            var fallbackFonts = Config.GuiStyle.TopMap.Title.FallbackFonts.AsStringArray;
            TitleFont.AssignFallbackFonts(fallbackFonts);

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.ForceTitleMeshUpdate();
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnCountFontConfigChanged(object sender, EventArgs e)
        {
            var newFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Count.Font, CountFont);
            if (newFont == null) return;

            CountFont = newFont;
            CountFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Count.FallbackFonts.AsStringArray);

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.UpdateCountFont(CountFont.Font);
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnCountFontSizeConfigChanged(object sender, EventArgs e)
        {
            CountFontSize = Config.GuiStyle.TopMap.Count.FontSize;

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.UpdateCountFontSize(CountFontSize);
                }
            });
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnCountFallbackFontsConfigChanged(object sender, EventArgs e)
        {
            if (CountFont == null) return;

            var fallbackFonts = Config.GuiStyle.TopMap.Count.FallbackFonts.AsStringArray;
            CountFont.AssignFallbackFonts(fallbackFonts);

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.ForceCountMeshUpdate();
                }
            });
        }
    }
}
