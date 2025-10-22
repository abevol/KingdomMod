using System;
using KingdomMod.OverlayMap.Assets;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    public class TopMapStyle
    {
        private TopMapView _view;

        public FontData SignFont;
        public float SignFontSize;
        public FontData TitleFont;
        public float TitleFontSize;
        public FontData CountFont;
        public float CountFontSize;

        public void Init(TopMapView view)
        {
            LogTrace("TopMapStyle.Init");
            LogTrace($"TopMapStyle.Init, GuiStyle.TopMap.Sign.Font: {GuiStyle.TopMap.Sign.Font.Value}");
            LogTrace($"TopMapStyle.Init, GuiStyle.TopMap.Title.Font: {GuiStyle.TopMap.Title.Font.Value}");
            LogTrace($"TopMapStyle.Init, GuiStyle.TopMap.Count.Font: {GuiStyle.TopMap.Count.Font.Value}");

            _view = view;
            SignFont = FontManager.CreateMainFont(GuiStyle.TopMap.Sign.Font, null);
            SignFontSize = GuiStyle.TopMap.Sign.FontSize;
            SignFont.AssignFallbackFonts(GuiStyle.TopMap.Sign.FallbackFonts.AsStringArray);

            TitleFont = FontManager.CreateMainFont(GuiStyle.TopMap.Title.Font, null);
            TitleFontSize = GuiStyle.TopMap.Title.FontSize;
            TitleFont.AssignFallbackFonts(GuiStyle.TopMap.Title.FallbackFonts.AsStringArray);

            CountFont = FontManager.CreateMainFont(GuiStyle.TopMap.Count.Font, null);
            CountFontSize = GuiStyle.TopMap.Count.FontSize;
            CountFont.AssignFallbackFonts(GuiStyle.TopMap.Count.FallbackFonts.AsStringArray);

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
            SignFont = FontManager.CreateMainFont(GuiStyle.TopMap.Sign.Font, SignFont);
            SignFont.AssignFallbackFonts(GuiStyle.TopMap.Sign.FallbackFonts.AsStringArray);

            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.UpdateSignFont(SignFont.Font);
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
            SignFont.AssignFallbackFonts(fallbackFonts);

            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.ForceSignMeshUpdate();
            }
        }

        private void OnTitleFontConfigChanged(object sender, EventArgs e)
        {
            TitleFont = FontManager.CreateMainFont(GuiStyle.TopMap.Title.Font, TitleFont);
            TitleFont.AssignFallbackFonts(GuiStyle.TopMap.Title.FallbackFonts.AsStringArray);

            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.UpdateTitleFont(TitleFont.Font);
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
            TitleFont.AssignFallbackFonts(fallbackFonts);

            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.ForceTitleMeshUpdate();
            }
        }

        private void OnCountFontConfigChanged(object sender, EventArgs e)
        {
            CountFont = FontManager.CreateMainFont(GuiStyle.TopMap.Count.Font, CountFont);
            CountFont.AssignFallbackFonts(GuiStyle.TopMap.Count.FallbackFonts.AsStringArray);

            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.UpdateCountFont(CountFont.Font);
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
            CountFont.AssignFallbackFonts(fallbackFonts);

            foreach (var pair in _view.MapMarkers)
            {
                pair.Value.ForceCountMeshUpdate();
            }
        }
    }
}