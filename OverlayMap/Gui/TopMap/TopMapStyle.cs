using System;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using TMPro;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    public class TopMapStyle
    {
        private TopMapView _view;

        public TMP_FontAsset SignFont;
        public float SignFontSize;
        public TMP_FontAsset TitleFont;
        public float TitleFontSize;
        public TMP_FontAsset CountFont;
        public float CountFontSize;

        public void Init(TopMapView view)
        {
            _view = view;
            SignFont = FontManager.GetFont(GuiStyle.TopMap.Sign.Font, SignFont);
            SignFontSize = GuiStyle.TopMap.Sign.FontSize;
            TitleFont = FontManager.GetFont(GuiStyle.TopMap.Title.Font, TitleFont);
            TitleFontSize = GuiStyle.TopMap.Title.FontSize;
            CountFont = FontManager.GetFont(GuiStyle.TopMap.Count.Font, CountFont);
            CountFontSize = GuiStyle.TopMap.Count.FontSize;

            GuiStyle.TopMap.BackgroundImageFile.Entry.SettingChanged += OnBackgroundConfigChanged;
            GuiStyle.TopMap.BackgroundColor.Entry.SettingChanged += OnBackgroundConfigChanged;
            GuiStyle.TopMap.BackgroundImageArea.Entry.SettingChanged += OnBackgroundConfigChanged;
            GuiStyle.TopMap.BackgroundImageBorder.Entry.SettingChanged += OnBackgroundConfigChanged;

            GuiStyle.TopMap.Sign.Font.Entry.SettingChanged += OnSignFontConfigChanged;
            GuiStyle.TopMap.Sign.FontSize.Entry.SettingChanged += OnSignFontSizeConfigChanged;
            GuiStyle.TopMap.Sign.FallbackFonts.Entry.SettingChanged += OnSignFallbackFontsConfigChanged;

            GuiStyle.TopMap.Title.Font.Entry.SettingChanged += OnTitleFontConfigChanged;
            GuiStyle.TopMap.Title.FontSize.Entry.SettingChanged += OnTitleFontSizeConfigChanged;
            GuiStyle.TopMap.Title.FallbackFonts.Entry.SettingChanged += OnTitleFallbackFontsConfigChanged;

            GuiStyle.TopMap.Count.Font.Entry.SettingChanged += OnCountFontConfigChanged;
            GuiStyle.TopMap.Count.FontSize.Entry.SettingChanged += OnCountFontSizeConfigChanged;
            GuiStyle.TopMap.Count.FallbackFonts.Entry.SettingChanged += OnCountFallbackFontsConfigChanged;
        }

        public void Destroy()
        {
            GuiStyle.TopMap.BackgroundImageFile.Entry.SettingChanged -= OnBackgroundConfigChanged;
            GuiStyle.TopMap.BackgroundColor.Entry.SettingChanged -= OnBackgroundConfigChanged;
            GuiStyle.TopMap.BackgroundImageArea.Entry.SettingChanged -= OnBackgroundConfigChanged;
            GuiStyle.TopMap.BackgroundImageBorder.Entry.SettingChanged -= OnBackgroundConfigChanged;

            GuiStyle.TopMap.Sign.Font.Entry.SettingChanged -= OnSignFontConfigChanged;
            GuiStyle.TopMap.Sign.FontSize.Entry.SettingChanged -= OnSignFontSizeConfigChanged;
            GuiStyle.TopMap.Sign.FallbackFonts.Entry.SettingChanged -= OnSignFallbackFontsConfigChanged;

            GuiStyle.TopMap.Title.Font.Entry.SettingChanged -= OnTitleFontConfigChanged;
            GuiStyle.TopMap.Title.FontSize.Entry.SettingChanged -= OnTitleFontSizeConfigChanged;
            GuiStyle.TopMap.Title.FallbackFonts.Entry.SettingChanged -= OnTitleFallbackFontsConfigChanged;

            GuiStyle.TopMap.Count.Font.Entry.SettingChanged -= OnCountFontConfigChanged;
            GuiStyle.TopMap.Count.FontSize.Entry.SettingChanged -= OnCountFontSizeConfigChanged;
            GuiStyle.TopMap.Count.FallbackFonts.Entry.SettingChanged -= OnCountFallbackFontsConfigChanged;

        }

        private void OnBackgroundConfigChanged(object sender, EventArgs e)
        {
            _view.UpdateBackgroundImage();
        }

        private void OnSignFontConfigChanged(object sender, EventArgs e)
        {
            SignFont = FontManager.GetFont(GuiStyle.TopMap.Sign.Font, SignFont);

            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.UpdateSignFont(SignFont);
            }
        }

        private void OnSignFontSizeConfigChanged(object sender, EventArgs e)
        {
            SignFontSize = GuiStyle.TopMap.Sign.FontSize;
            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.UpdateSignFontSize(SignFontSize);
            }
        }

        private void OnSignFallbackFontsConfigChanged(object sender, EventArgs e)
        {
            var fallbackFonts = GuiStyle.TopMap.Sign.FallbackFonts.AsStringArray;
            FontManager.AddFallbackFonts(SignFont, fallbackFonts, true);
        }

        private void OnTitleFontConfigChanged(object sender, EventArgs e)
        {
            TitleFont = FontManager.GetFont(GuiStyle.TopMap.Title.Font, TitleFont);

            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.UpdateTitleFont(TitleFont);
            }
        }

        private void OnTitleFontSizeConfigChanged(object sender, EventArgs e)
        {
            TitleFontSize = GuiStyle.TopMap.Title.FontSize;
            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.UpdateTitleFontSize(TitleFontSize);
            }
        }

        private void OnTitleFallbackFontsConfigChanged(object sender, EventArgs e)
        {
            var fallbackFonts = GuiStyle.TopMap.Title.FallbackFonts.AsStringArray;
            FontManager.AddFallbackFonts(TitleFont, fallbackFonts, true);
        }

        private void OnCountFontConfigChanged(object sender, EventArgs e)
        {
            CountFont = FontManager.GetFont(GuiStyle.TopMap.Count.Font, CountFont);

            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.UpdateCountFont(CountFont);
            }
        }

        private void OnCountFontSizeConfigChanged(object sender, EventArgs e)
        {
            CountFontSize = GuiStyle.TopMap.Count.FontSize;
            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.UpdateCountFontSize(CountFontSize);
            }
        }

        private void OnCountFallbackFontsConfigChanged(object sender, EventArgs e)
        {
            var fallbackFonts = GuiStyle.TopMap.Count.FallbackFonts.AsStringArray;
            FontManager.AddFallbackFonts(CountFont, fallbackFonts, true);
        }
    }
}