using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Assets
{
    public class FontData
    {
        public string FontName;
        public TMP_FontAsset Font;
        public Font SourceFont;
        public HashSet<char> Chars;
        public HashSet<char> MissingChars;

        public void AssignFallbackFonts(string[] fontNames)
        {
            if (Font == null)
            {
                LogError("Font is null.");
                return;
            }

            Font.fallbackFontAssetTable = new ();

            foreach (var fontName in fontNames)
            {
                AddFallbackFont(fontName);
            }
        }

        public void AddFallbackFont(string fontName)
        {
            if (Font == null)
            {
                LogError("Font is null.");
                return;
            }

            var sourceFont = FontManager.CreateSourceFont(fontName);
            if (sourceFont == null)
            {
                LogError($"Failed to create source font: {fontName}");
                return;
            }

            var fallbackFont = FontManager.CreateFont(sourceFont);
            if (fallbackFont == null)
            {
                LogError($"Failed to get fallback font: {fontName}");
                return;
            }

            if (fallbackFont == Font)
            {
                LogError($"Failed to Add fallback font: {fontName} same as the main font.");
                return;
            }

            AddFallbackFont(Font, fallbackFont);

            var missingChars = TryAddCharacters(fallbackFont, new string(MissingChars.ToArray()));
            if (!string.IsNullOrEmpty(missingChars))
                LogError($"failed to add chars to fallbackFont: {fallbackFont.faceInfo.familyName}, missing chars: {missingChars}");
        }

        private static void AddFallbackFont(TMP_FontAsset mainFont, TMP_FontAsset fontAsset)
        {
            if (mainFont.fallbackFontAssetTable == null)
                mainFont.fallbackFontAssetTable = new ();

            if (!mainFont.fallbackFontAssetTable.Contains(fontAsset))
            {
                mainFont.fallbackFontAssetTable.Add(fontAsset);
                LogTrace($"Added fallback font: {fontAsset.faceInfo.familyName}, {fontAsset.faceInfo.styleName}");
            }
            else
            {
                LogWarning($"Fallback font: {fontAsset.faceInfo.familyName}, {fontAsset.faceInfo.styleName} already exists.");
            }
        }

        public void TryAddCharacters(string characters)
        {
            if (string.IsNullOrEmpty(characters))
                return;

            LogTrace($"TryAddCharacters: {FontName}, {Font.faceInfo.familyName}, chars: {characters}");

            if (Font.HasCharacters(characters, out var notOwnedChars))
                return;

            Chars.UnionWith(characters);

            var charsToAdd = new string(notOwnedChars.ToArray());
            var missingChars = TryAddCharacters(Font, charsToAdd);
            MissingChars.UnionWith(missingChars);

            if (!string.IsNullOrEmpty(missingChars) && Font.fallbackFontAssetTable != null)
            {
                foreach (var fallbackFont in Font.fallbackFontAssetTable)
                {
                    LogTrace($"Try add missing chars to fallbackFont: {fallbackFont.faceInfo.familyName}, chars: {missingChars}");
                    missingChars = TryAddCharacters(fallbackFont, missingChars);
                    if (string.IsNullOrEmpty(missingChars))
                        break;
                }
            }
        }

        private static string TryAddCharacters(TMP_FontAsset fontAsset, string chars)
        {
            if (string.IsNullOrEmpty(chars))
                return string.Empty;

            fontAsset.TryAddCharacters(chars, out var missingChars, true);

            if (!string.IsNullOrEmpty(missingChars))
            {
                LogWarning($"Font '{fontAsset.faceInfo.familyName}' is missing characters: {missingChars}");
                LogWarning($"sourceFontFile '{fontAsset.sourceFontFile == null}' '{!fontAsset.sourceFontFile}'");
            }

            return missingChars;
        }
    }
}