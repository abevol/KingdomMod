using KingdomMod.OverlayMap.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Assets
{
    public class FontManager : MonoBehaviour
    {
        private static string GetSystemFontFile(string fontName)
        {
            string fontFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fontName);
            if (!File.Exists(fontFilePath))
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                fontFilePath = Path.Combine(userProfile, "AppData", "Local", "Microsoft", "Windows", "Fonts", fontName);
            }

            return fontFilePath;
        }

        public static FontData CreateMainFont(string fontName, FontData lastFont)
        {
            var newFont = CreateMainFont(fontName);
            if (newFont == null)
            {
                LogError($"Failed to create font: {fontName}");
                return null;
            }

            if (lastFont != null)
                newFont.TryAddCharacters(new string(lastFont.Chars.ToArray()));

            return newFont;
        }

        private static FontData CreateMainFont(string fontName)
        {
            var font = CreateFont(fontName);
            if (font == null)
            {
                LogError($"Failed to create font: {fontName}");
                return null;
            }

            return new FontData {FontName = fontName, Font = font, Chars = [], MissingChars = [] };
        }

        public static TMP_FontAsset CreateFont(string fontName)
        {
            var fontFilePath = Path.Combine(AssetsDir, "Fonts", fontName);
            if (!File.Exists(fontFilePath))
            {
                fontFilePath = GetSystemFontFile(fontName);
            }

            Font font;
            if (File.Exists(fontFilePath))
            {
                LogTrace($"fontFilePath: {fontFilePath}");

                font = new Font(fontFilePath);
            }
            else
            {
                LogError($"Failed to find font file: {fontFilePath}, try to load font '{fontName}' from game resources.");

                font = Resources.Load<Font>(fontName);
                if (font == null)
                    font = Resources.GetBuiltinResource<Font>(fontName);
            }

            // Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            // Font font = Resources.Load<Font>("Fonts/Zpix");
            
            if (font == null)
            {
                LogError($"Failed to load font: {fontName}");
                return null;
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(font, 48, 3, GlyphRenderMode.SDFAA, 1024, 1024);
            if (fontAsset == null)
                LogError($"Failed to create font.");

            return fontAsset;
        }
    }
}