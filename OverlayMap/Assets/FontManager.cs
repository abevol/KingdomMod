using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Assets
{
    public class FontManager
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

            return new FontData {FontName = fontName, Font = font, Chars = new HashSet<char>(), MissingChars = new HashSet<char>() };
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

#if IL2CPP
                font = new Font();
                if (string.IsNullOrEmpty(Path.GetDirectoryName(fontFilePath)))
                    Font.Internal_CreateFont(font, fontFilePath);
                else
                    Font.Internal_CreateFontFromPath(font, fontFilePath);
#else
                font = new Font(fontFilePath);
#endif
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