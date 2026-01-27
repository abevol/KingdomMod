using KingdomMod.OverlayMap.Assets;
using KingdomMod.OverlayMap.Patchers;
using KingdomMod.Shared.Attributes;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    [RegisterTypeInIl2Cpp]
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
            LogTrace("TopMapStyle.Init");
            LogTrace($"TopMapStyle.Init, Config.GuiStyle.TopMap.Sign.Font: {Config.GuiStyle.TopMap.Sign.Font.Value}");
            LogTrace($"TopMapStyle.Init, Config.GuiStyle.TopMap.Title.Font: {Config.GuiStyle.TopMap.Title.Font.Value}");
            LogTrace($"TopMapStyle.Init, Config.GuiStyle.TopMap.Count.Font: {Config.GuiStyle.TopMap.Count.Font.Value}");

            SignFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Sign.Font, null);
            SignFontSize = Config.GuiStyle.TopMap.Sign.FontSize;
            SignFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Sign.FallbackFonts.AsStringArray);

            TitleFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Title.Font, null);
            TitleFontSize = Config.GuiStyle.TopMap.Title.FontSize;
            TitleFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Title.FallbackFonts.AsStringArray);

            CountFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Count.Font, null);
            CountFontSize = Config.GuiStyle.TopMap.Count.FontSize;
            CountFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Count.FallbackFonts.AsStringArray);

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

        private void OnDestroy()
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

        private void OnBackgroundConfigChanged(object sender, EventArgs e)
        {
            ForEachTopMapView(view => view.UpdateBackgroundImage());
        }

        private void OnSignFontConfigChanged(object sender, EventArgs e)
        {
            SignFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Sign.Font, SignFont);
            SignFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Sign.FallbackFonts.AsStringArray);

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.UpdateSignFont(SignFont.Font);
                }
            });
        }

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

        private void OnSignFallbackFontsConfigChanged(object sender, EventArgs e)
        {
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

        private void OnTitleFontConfigChanged(object sender, EventArgs e)
        {
            TitleFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Title.Font, TitleFont);
            TitleFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Title.FallbackFonts.AsStringArray);

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.UpdateTitleFont(TitleFont.Font);
                }
            });
        }

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

        private void OnTitleFallbackFontsConfigChanged(object sender, EventArgs e)
        {
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

        private void OnCountFontConfigChanged(object sender, EventArgs e)
        {
            CountFont = FontManager.CreateMainFont(Config.GuiStyle.TopMap.Count.Font, CountFont);
            CountFont.AssignFallbackFonts(Config.GuiStyle.TopMap.Count.FallbackFonts.AsStringArray);

            ForEachTopMapView(view =>
            {
                foreach (var pair in view.MapMarkers)
                {
                    pair.Value.UpdateCountFont(CountFont.Font);
                }
            });
        }

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

        private void OnCountFallbackFontsConfigChanged(object sender, EventArgs e)
        {
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