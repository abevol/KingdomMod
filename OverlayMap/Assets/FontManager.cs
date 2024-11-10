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
        private readonly Dictionary<string, (TMP_FontAsset font, HashSet<char> chars)> _fonts = new();

        public TMP_FontAsset GetFont(string fontName, TMP_FontAsset oldFontAsset)
        {
            var font = GetFont(fontName);
            if (font != null && oldFontAsset != null)
                TransferCharacters(oldFontAsset, font, true);

            return font;
        }

        public TMP_FontAsset GetFont(string fontName)
        {
            if (_fonts.TryGetValue(fontName, out var fontExisted))
            {
                return fontExisted.font;
            }

            var font = CreateFont(fontName);
            if (font != null)
                _fonts.Add(fontName, (font, new HashSet<char>()));


            return font;
        }

        public void AddFallbackFonts(TMP_FontAsset mainFont, string[] fontNames, bool replace)
        {
            if (mainFont == null)
            {
                LogError("mainFont is null.");
                return;
            }

            if (replace)
                mainFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

            foreach (var fontName in fontNames)
            {
                AddFallbackFont(mainFont, fontName);
            }

            TransferCharacters(mainFont, mainFont, true);
        }

        public void AddFallbackFont(TMP_FontAsset mainFont, string fontName)
        {
            if (mainFont == null)
            {
                LogError("mainFont is null.");
                return;
            }

            var fallbackFont = GetFont(fontName);
            if (fallbackFont == null)
            {
                LogError($"Failed to get fallback font: {fontName}");
                return;
            }

            AddFallbackFont(mainFont, fallbackFont);
        }

        public static void AddFallbackFont(TMP_FontAsset mainFont, TMP_FontAsset fontAsset)
        {
            if (mainFont.fallbackFontAssetTable == null)
                mainFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

            if (!mainFont.fallbackFontAssetTable.Contains(fontAsset))
            {
                mainFont.fallbackFontAssetTable.Add(fontAsset);
                LogMessage($"Added fallback font: {fontAsset.faceInfo.familyName}, {fontAsset.faceInfo.styleName}");
            }
            else
            {
                LogWarning($"Fallback font: {fontAsset.faceInfo.familyName}, {fontAsset.faceInfo.styleName} already exists.");
            }
        }

        private string GetSystemFontFile(string fontName)
        {
            string fontFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fontName);
            if (!File.Exists(fontFilePath))
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                fontFilePath = Path.Combine(userProfile, "AppData", "Local", "Microsoft", "Windows", "Fonts", fontName);
            }

            return fontFilePath;
        }

        private TMP_FontAsset CreateFont(string fontName)
        {
            var fontFilePath = Path.Combine(AssetsDir, "Fonts", fontName);
            if (!File.Exists(fontFilePath))
            {
                fontFilePath = GetSystemFontFile(fontName);
            }

            Font font;
            if (File.Exists(fontFilePath))
            {
                LogMessage($"fontFilePath: {fontFilePath}");

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

        public void TryAddCharacters(string fontName, string characters)
        {
            var font = GetFont(fontName);
            if (font == null)
            {
                LogError($"Failed to get font: {fontName}");
                return;
            }

            TryAddCharacters(font, characters);
        }

        public void TryAddCharacters(TMP_FontAsset fontAsset, string characters)
        {
            LogMessage($"TryAddCharacters: {characters}, ThreadId: {Thread.CurrentThread.ManagedThreadId}, {Thread.CurrentThread.Name}");
            if (fontAsset == null)
            {
                LogError("fontAsset is null.");
                return;
            }

            if (string.IsNullOrEmpty(characters))
                return;

            var chars = _fonts
                .Where(entry => entry.Value.font == fontAsset)
                .Select(entry => entry.Value.chars)
                .FirstOrDefault();
            if (chars == null)
                LogError("chars is null");
            else
                chars.UnionWith(characters);

            bool result = fontAsset.TryAddCharacters(characters, out var missingCharacters, false);
            if (result)
            {
                LogMessage($"successfully added characters: {characters}");
            }
            else
            {
                LogError($"failed to add characters: {characters}, missing characters: {missingCharacters}");
            }
        }

        private void TransferCharacters(TMP_FontAsset source, TMP_FontAsset target, bool replace)
        {
            LogMessage($"TransferCharacters, ThreadId: {Thread.CurrentThread.ManagedThreadId}, {Thread.CurrentThread.Name}");

            if (source == null || target == null)
            {
                LogError("Source or Target font asset is null.");
                return;
            }

            var sourceChars = _fonts
                .Where(entry => entry.Value.font == source)
                .Select(entry => entry.Value.chars)
                .FirstOrDefault();
            if (sourceChars == null)
            {
                LogError("sourceChars is null");
                return;
            }

            if (replace)
            {
                target.characterTable.Clear();
                target.characterLookupTable.Clear();

                var targetChars = _fonts
                    .Where(entry => entry.Value.font == target)
                    .Select(entry => entry.Value.chars)
                    .FirstOrDefault();
                if (targetChars != null)
                {
                    sourceChars.UnionWith(targetChars);
                    targetChars.Clear();
                }
            }

            var result = new string(sourceChars.ToArray());
            LogWarning($"TransferCharacters: {result}");

            TryAddCharacters(target, result);
        }
    }
}